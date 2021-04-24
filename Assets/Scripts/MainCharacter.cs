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

    public void Stop()
    {
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.velocity = new Vector2(0, myRigidbody.velocity.y);
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
        thisCharacter.Stop();
        thisCharacter.FireAnimationTrigger("Idle");
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
    }

    public override CharacterState Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
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
        }
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
}

public class JumpRightState : CharacterState
{
    public JumpRightState(MainCharacter character) : base(character)
    {}

    public override void Start()
    {
        thisCharacter.Jump();
        thisCharacter.SetHorizFlip(false);
        thisCharacter.FireAnimationTrigger("JumpUp");
    }

    public override CharacterState Update()
    {
        if (thisCharacter.IsGrounded)
            return this;
        else
            return new FlyUpRightState(thisCharacter);
    }
}

public class JumpLeftState : CharacterState
{
    public JumpLeftState(MainCharacter character) : base(character)
    { }

    public override void Start()
    {
        thisCharacter.Jump();
        thisCharacter.SetHorizFlip(true);
        thisCharacter.FireAnimationTrigger("JumpUp");
    }

    public override CharacterState Update()
    {
        if (thisCharacter.IsGrounded)
            return this;
        else
            return new FlyUpLeftState(thisCharacter);
    }
}

public class FlyUpLeftState : CharacterState
{
    public FlyUpLeftState(MainCharacter character) : base(character)
    { }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(true);
        thisCharacter.FireAnimationTrigger("JumpUp");
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
        else if (thisCharacter.IsTopping())
            return new FlyTopLeftState(thisCharacter);
        else if (thisCharacter.IsDowning())
            return new FlyDownLeftState(thisCharacter);
        else
            return this;
    }
}

public class FlyUpRightState : CharacterState
{
    public FlyUpRightState(MainCharacter character) : base(character)
    { }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(false);
        thisCharacter.FireAnimationTrigger("JumpUp");
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
        else if (thisCharacter.IsTopping())
            return new FlyTopRightState(thisCharacter);
        else if (thisCharacter.IsDowning())
            return new FlyDownRightState(thisCharacter);
        else
            return this;
    }
}

public class FlyTopLeftState : CharacterState
{
    public FlyTopLeftState(MainCharacter character) : base(character)
    { }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(true);
        thisCharacter.FireAnimationTrigger("JumpTop");
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
        else if (thisCharacter.IsUpping())
            return new FlyUpLeftState(thisCharacter);
        else if (thisCharacter.IsDowning())
            return new FlyDownLeftState(thisCharacter);
        else
            return this;
    }
}

public class FlyTopRightState : CharacterState
{
    public FlyTopRightState(MainCharacter character) : base(character)
    { }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(false);
        thisCharacter.FireAnimationTrigger("JumpTop");
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
        else if (thisCharacter.IsUpping())
            return new FlyUpRightState(thisCharacter);
        else if (thisCharacter.IsDowning())
            return new FlyDownRightState(thisCharacter);
        else
            return this;
    }
}

public class FlyDownLeftState : CharacterState
{
    public FlyDownLeftState(MainCharacter character) : base(character)
    { }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(true);
        thisCharacter.FireAnimationTrigger("JumpDown");
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
        else if (thisCharacter.IsUpping())
            return new FlyUpLeftState(thisCharacter);
        else if (thisCharacter.IsTopping())
            return new FlyTopLeftState(thisCharacter);
        else
            return this;
    }
}

public class FlyDownRightState : CharacterState
{
    public FlyDownRightState(MainCharacter character) : base(character)
    { }

    public override void Start()
    {
        thisCharacter.SetHorizFlip(false);
        thisCharacter.FireAnimationTrigger("JumpDown");
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
        else if (thisCharacter.IsUpping())
            return new FlyUpRightState(thisCharacter);
        else if (thisCharacter.IsTopping())
            return new FlyTopRightState(thisCharacter);
        else
            return this;
    }
}