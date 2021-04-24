using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    [SerializeField]
    private float runVelocity = 1f;

    [SerializeField]
    private float jumpVelocity = 1f;

    [SerializeField]
    private float apexVelocity = .25f;

    [SerializeField]
    private float enterJelloporterSpeed = 5f;

    [SerializeField]
    private Collider2D groundCollider;

    private CharacterState currentState =  null;
    private Animator myAnimator = null;
    private SpriteRenderer myRenderer = null;
    private Rigidbody2D myRigidbody = null;

    public bool IsGrounded { get; private set; }

    private void Update()
    {
        if (currentState == null)
        {
            currentState = new IdleState(this);
            currentState.Start();
            SignalManager.Inst.FireSignal(new StateStartedSignal(currentState));
        }
        CharacterState nextState = currentState.Update();
        if (nextState != currentState)
        {
            currentState.End();
            SignalManager.Inst.FireSignal(new StateEndingSignal(currentState));
            currentState = nextState;
            nextState.Start();
            SignalManager.Inst.FireSignal(new StateStartedSignal(currentState));
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        IsGrounded = (collision != null) && (collision.tag == "ground");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IsGrounded = false;
    }

    public void FireAnimationTrigger(string trigger)
    {
        if (myAnimator == null)
            myAnimator = GetComponent<Animator>();
        myAnimator.SetTrigger(trigger);
    }

    public void SetHorizFlip(bool shouldFlip)
    {
        if (myRenderer == null)
            myRenderer = GetComponent<SpriteRenderer>();
        myRenderer.flipX = shouldFlip;
    }
    public void MoveRight()
    {
        if(myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.velocity = new Vector2(runVelocity, myRigidbody.velocity.y);
    }

    public void MoveLeft()
    {
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.velocity = new Vector2(-runVelocity, myRigidbody.velocity.y);
    }

    public void StopMove()
    {
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.velocity = new Vector2(0f, myRigidbody.velocity.y);
    }

    public void StopJump()
    {
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, 0f);
    }

    public void Jump()
    {
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpVelocity);
    }

    public bool IsUpping()
    {
        return myRigidbody.velocity.y > apexVelocity;
    }

    public bool IsTopping()
    {
        return Mathf.Abs(myRigidbody.velocity.y) < apexVelocity;
    }

    public bool IsDowning()
    {
        return myRigidbody.velocity.y < -apexVelocity;
    }

    public bool IsFacingLeft()
    {
        if (myRenderer == null)
            myRenderer = GetComponent<SpriteRenderer>();
        return myRenderer.flipX;
    }

    public bool EnterJelloporter(Jelloporter jelloporter)
    {
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.velocity = Vector2.zero;
        float distance = Vector2.Distance(transform.position, jelloporter.transform.position);
        if(distance < enterJelloporterSpeed * Time.deltaTime)
        {
            transform.position = jelloporter.transform.position;
            return true;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, jelloporter.transform.position, enterJelloporterSpeed * Time.deltaTime);
            return false;
        }
    }

    public void SnapToJelloporterAndSetExitVelocity(Jelloporter jelloporter, Vector2 exitVelocity)
    {
        transform.position = jelloporter.transform.position;
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.velocity = exitVelocity;
    }
}

public abstract class CharacterState : State
{
    protected MainCharacter thisCharacter;
    public CharacterState(MainCharacter character)
    {
        this.thisCharacter = character;
    }

    public virtual CharacterState Update()
    {
        return (CharacterState)base.Update();
    }
}

public class IdleState : CharacterState
{
    private CharacterState nextState;

    public IdleState(MainCharacter thisCharacter) : base(thisCharacter)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.StopMove();
        thisCharacter.FireAnimationTrigger("Idle");
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        switch (moveButtonPressedSignal.moveButton)
        {
            case MoveButton.LEFT:
                nextState = new RunLeftState(thisCharacter);
                break;
            case MoveButton.RIGHT:
                nextState = new RunRightState(thisCharacter);
                break;
            case MoveButton.JUMP:
                if (thisCharacter.IsFacingLeft())
                    nextState = new JumpLeftState(thisCharacter);
                else
                    nextState = new JumpRightState(thisCharacter);
                break;
        }
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class RunLeftState : CharacterState
{
    private CharacterState nextState;
    private bool canRelease = true;

    public RunLeftState(MainCharacter character) : base(character)
    {
        this.nextState = this;
    }

    public override void Start()
    {
        this.thisCharacter.SetHorizFlip(true);
        thisCharacter.FireAnimationTrigger("Run");
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MoveButtonReleasedSignal>(onMoveButtonReleased);
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        if(nextState == this)
            thisCharacter.MoveLeft();
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MoveButtonReleasedSignal>(onMoveButtonReleased);
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        switch (moveButtonPressedSignal.moveButton)
        {
            case MoveButton.RIGHT:
                nextState = new RunRightState(thisCharacter);
                canRelease = false;
                break;
            case MoveButton.JUMP:
                nextState = new JumpLeftState(thisCharacter);
                canRelease = false;
                break;
        }
    }

    private void onMoveButtonReleased(Signal signal)
    {
        if (canRelease)
        {
            MoveButtonReleasedSignal moveButtonReleasedSignal = (MoveButtonReleasedSignal)signal;
            if (moveButtonReleasedSignal.moveButton == MoveButton.LEFT)
            {
                nextState = new IdleState(thisCharacter);
            }
        }
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class RunRightState : CharacterState
{
    private CharacterState nextState;
    private bool canRelease = true;

    public RunRightState(MainCharacter character) : base (character)
    {
        this.nextState = this;
    }

    public override void Start()
    {
        this.thisCharacter.SetHorizFlip(false);
        thisCharacter.FireAnimationTrigger("Run");
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MoveButtonReleasedSignal>(onMoveButtonReleased);
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        if(nextState == this)
            thisCharacter.MoveRight();
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MoveButtonReleasedSignal>(onMoveButtonReleased);
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        switch (moveButtonPressedSignal.moveButton)
        {
            case MoveButton.LEFT:
                nextState = new RunLeftState(thisCharacter);
                canRelease = false;
                break;
            case MoveButton.JUMP:
                nextState = new JumpRightState(thisCharacter);
                canRelease = false;
                break;
        }
    }

    private void onMoveButtonReleased(Signal signal)
    {
        if (canRelease)
        {
            MoveButtonReleasedSignal moveButtonReleasedSignal = (MoveButtonReleasedSignal)signal;
            if (moveButtonReleasedSignal.moveButton == MoveButton.RIGHT)
            {
                nextState = new IdleState(thisCharacter);
            }
        }
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class JumpRightState : CharacterState
{
    private CharacterState nextState;

    public JumpRightState(MainCharacter character) : base(character)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.Jump();
        thisCharacter.SetHorizFlip(false);
        thisCharacter.FireAnimationTrigger("JumpUp");
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;

        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT) && !InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            thisCharacter.MoveLeft();
        if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.MoveRight();

        if (thisCharacter.IsGrounded)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new JumpLeftState(thisCharacter);
            else
                return this;
        }
        else
            return new FlyUpRightState(thisCharacter);
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class JumpLeftState : CharacterState
{
    private CharacterState nextState;

    public JumpLeftState(MainCharacter character) : base(character)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.Jump();
        thisCharacter.SetHorizFlip(true);
        thisCharacter.FireAnimationTrigger("JumpUp");
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;

        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT) && !InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            thisCharacter.MoveLeft();
        if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.MoveRight();

        if (thisCharacter.IsGrounded)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new JumpRightState(thisCharacter);
            else
                return this;
        }
        else
            return new FlyUpLeftState(thisCharacter);
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class FlyUpLeftState : CharacterState
{
    private CharacterState nextState;

    public FlyUpLeftState(MainCharacter character) : base(character)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(true);
        thisCharacter.FireAnimationTrigger("JumpUp");
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;

        if (!InputHandler.Inst.ButtonIsDown(MoveButton.JUMP))
            thisCharacter.StopJump();
        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT) && !InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            thisCharacter.MoveLeft();
        if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.MoveRight();
        if (!InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.StopMove();

            if (thisCharacter.IsGrounded)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else
                return new IdleState(thisCharacter);
        }
        else if (thisCharacter.IsTopping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new FlyTopRightState(thisCharacter);
            else
                return new FlyTopLeftState(thisCharacter);
        }
        else if (thisCharacter.IsDowning())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new FlyDownRightState(thisCharacter);
            else
                return new FlyDownLeftState(thisCharacter);
        }
        else
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new FlyUpRightState(thisCharacter);
            else
                return this;
        }
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class FlyUpRightState : CharacterState
{
    private CharacterState nextState;

    public FlyUpRightState(MainCharacter character) : base(character)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(false);
        thisCharacter.FireAnimationTrigger("JumpUp");
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;

        if (!InputHandler.Inst.ButtonIsDown(MoveButton.JUMP))
            thisCharacter.StopJump();
        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT) && !InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            thisCharacter.MoveLeft();
        if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.MoveRight();
        if (!InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.StopMove();

        if (thisCharacter.IsGrounded)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else
                return new IdleState(thisCharacter);
        }
        else if (thisCharacter.IsTopping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new FlyTopLeftState(thisCharacter);
            else
                return new FlyTopRightState(thisCharacter);
        }
        else if (thisCharacter.IsDowning())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new FlyDownLeftState(thisCharacter);
            else
                return new FlyDownRightState(thisCharacter);
        }
        else
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new FlyUpLeftState(thisCharacter);
            else
                return this;
        }
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class FlyTopLeftState : CharacterState
{
    private CharacterState nextState;

    public FlyTopLeftState(MainCharacter character) : base(character)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(true);
        thisCharacter.FireAnimationTrigger("JumpTop");
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;

        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT) && !InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            thisCharacter.MoveLeft();
        if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.MoveRight();
        if (!InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.StopMove();

        if (thisCharacter.IsGrounded)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else
                return new IdleState(thisCharacter);
        }
        else if (thisCharacter.IsUpping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new FlyUpRightState(thisCharacter);
            else
                return new FlyUpLeftState(thisCharacter);
        }
        else if (thisCharacter.IsDowning())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new FlyDownRightState(thisCharacter);
            else
                return new FlyDownLeftState(thisCharacter);
        }
        else
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new FlyTopRightState(thisCharacter);
            else
                return this;
        }
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class FlyTopRightState : CharacterState
{
    private CharacterState nextState;

    public FlyTopRightState(MainCharacter character) : base(character)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(false);
        thisCharacter.FireAnimationTrigger("JumpTop");
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;

        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT) && !InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            thisCharacter.MoveLeft();
        if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.MoveRight();
        if (!InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.StopMove();

        if (thisCharacter.IsGrounded)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else
                return new IdleState(thisCharacter);
        }
        else if (thisCharacter.IsUpping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new FlyUpLeftState(thisCharacter);
            else
                return new FlyUpRightState(thisCharacter);
        }
        else if (thisCharacter.IsDowning())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new FlyDownLeftState(thisCharacter);
            else
                return new FlyDownRightState(thisCharacter);
        }
        else
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new FlyTopLeftState(thisCharacter);
            else
                return this;
        }
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class FlyDownLeftState : CharacterState
{
    private CharacterState nextState;

    public FlyDownLeftState(MainCharacter character) : base(character)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(true);
        thisCharacter.FireAnimationTrigger("JumpDown");
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;

        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT) && !InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            thisCharacter.MoveLeft();
        if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.MoveRight();
        if (!InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.StopMove();

        if (thisCharacter.IsGrounded)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else
                return new IdleState(thisCharacter);
        }
        else if (thisCharacter.IsUpping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new FlyUpRightState(thisCharacter);
            else
                return new FlyUpLeftState(thisCharacter);
        }
        else if (thisCharacter.IsTopping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new FlyTopRightState(thisCharacter);
            else
                return new FlyTopLeftState(thisCharacter);
        }
        else
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new FlyDownRightState(thisCharacter);
            else
                return this;
        }
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class FlyDownRightState : CharacterState
{
    private CharacterState nextState;

    public FlyDownRightState(MainCharacter character) : base(character)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(false);
        thisCharacter.FireAnimationTrigger("JumpDown");
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;

        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT) && !InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            thisCharacter.MoveLeft();
        if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.MoveRight();
        if (!InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT) && !InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            thisCharacter.StopMove();

        if (thisCharacter.IsGrounded)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else
                return new IdleState(thisCharacter);
        }
        else if (thisCharacter.IsUpping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new FlyUpLeftState(thisCharacter);
            else
                return new FlyUpRightState(thisCharacter);
        }
        else if (thisCharacter.IsTopping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new FlyTopLeftState(thisCharacter);
            else
                return new FlyTopRightState(thisCharacter);
        }
        else
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new FlyDownLeftState(thisCharacter);
            else
                return this;
        }
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }
}

public class EnterJelloporterState : CharacterState {
    public Jelloporter jelloporter;
    public EnterJelloporterState (MainCharacter thisCharacter, Jelloporter jelloporter) : base(thisCharacter)
    {
        this.jelloporter = jelloporter;
    }

    public override CharacterState Update()
    {
        if (thisCharacter.EnterJelloporter(jelloporter))
            return new ExitJelloporterState(thisCharacter, jelloporter.ConnectedJelloporter);
        else
            return this;
    }
}

public class ExitJelloporterState : CharacterState
{
    public Jelloporter jelloporter;
    public ExitJelloporterState(MainCharacter thisCharacter, Jelloporter jelloporter) : base(thisCharacter)
    {
        this.jelloporter = jelloporter;
    }

    public override void Start()
    {
        Vector2 exitVelocity = new Vector2(-5f, 10f);
        if (jelloporter.ExitRight)
        {
            exitVelocity = new Vector2(5f, 10f);
            thisCharacter.SetHorizFlip(false);
        }
        else
            thisCharacter.SetHorizFlip(true);
        thisCharacter.SnapToJelloporterAndSetExitVelocity(jelloporter, exitVelocity);
        SignalManager.Inst.FireSignal(new PlayerExitingJelloporterSignal(jelloporter));
    }

    public override CharacterState Update()
    {
        if (thisCharacter.IsGrounded)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else
                return new IdleState(thisCharacter);
        }
        else
        {
            if (thisCharacter.IsUpping())
                thisCharacter.FireAnimationTrigger("JumpUp");
            else if (thisCharacter.IsTopping())
                thisCharacter.FireAnimationTrigger("JumpTop");
            else if (thisCharacter.IsDowning())
                thisCharacter.FireAnimationTrigger("JumpDown");
            return this;
        }
    }
}