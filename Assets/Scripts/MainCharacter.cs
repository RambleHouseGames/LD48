using System;
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
    private float kickBoostVelocity = 2f;

    [SerializeField]
    private float enterJelloporterSpeed = 5f;

    [SerializeField]
    private TouchTrigger groundCollider;

    [SerializeField]
    private TouchTrigger leftBlockCollider;

    [SerializeField]
    private TouchTrigger rightBlockCollider;

    [SerializeField]
    private Collider2D mainCollider;

    [SerializeField]
    private Character character;
    public Character Character { get { return character; } }

    [SerializeField]
    private ParticleSystem deathBlast;

    private CharacterState currentState =  null;
    private Animator myAnimator = null;
    private SpriteRenderer myRenderer = null;
    private Rigidbody2D myRigidbody = null;

    public bool IsGrounded { get
        {
            if (groundCollider.IsTriggered)
                CanJumpKick = true;
            return groundCollider.IsTriggered;
        }
    }
    public bool IsBlockedLeft { get { return leftBlockCollider.IsTriggered; } }
    public bool IsBlockedRight { get { return rightBlockCollider.IsTriggered; } }

    [NonSerialized]
    public bool CanJumpKick = true;

    [NonSerialized]
    public JelloPlate wayPoint = null;

    private void Update()
    {
        if (currentState == null)
        {
            currentState = new WaitToStart(this);
            currentState.Start();
        }
        CharacterState nextState = currentState.Update();
        if (nextState != currentState)
        {
            currentState.End();
            currentState = nextState;
            nextState.Start();
        }
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

    public void KickBoost()
    {
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        float newYVelocity = myRigidbody.velocity.y + kickBoostVelocity;
        if (newYVelocity < kickBoostVelocity)
            newYVelocity = kickBoostVelocity;
        myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, newYVelocity);
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

    public void SnapToPlateAndSetExitVelocity(JelloPlate plate, Vector2 exitVelocity)
    {
        transform.position = plate.transform.position;
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.velocity = exitVelocity;
    }

    public void SetKinematic(bool shouldBeKinematic)
    {
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        myRigidbody.isKinematic = shouldBeKinematic;
    }

    public void OnAttackLockReleased()
    {
        SignalManager.Inst.FireSignal(new AttackLockReleasedSignal());
    }

    public void OnAttackAnimationFinsished()
    {
        SignalManager.Inst.FireSignal(new AttackAnimationFinishedSignal());
    }

    public void FireDeathBlast()
    {
        deathBlast.Play();
    }

    public void DisableAllColliders()
    {
        mainCollider.enabled = false;
    }

    public void EnableMainCollider()
    {
        mainCollider.enabled = true;
    }

    public JelloPlate GetWaypoint()
    {
        return wayPoint;
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

public class WaitToStart : CharacterState
{
    private CharacterState nextState;

    public WaitToStart(MainCharacter thisCharacter) : base(thisCharacter)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.StopMove();
        thisCharacter.FireAnimationTrigger("Idle");
        SignalManager.Inst.AddListener<StateStartedSignal>(onStateStarted);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<StateStartedSignal>(onStateStarted);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onStateStarted(Signal signal)
    {

        StateStartedSignal stateStartedSignal = (StateStartedSignal)signal;
        if (stateStartedSignal.startedState.GetType() == typeof(PlayState))
        {
            PlayState playState = (PlayState)stateStartedSignal.startedState;
            if (playState.character == thisCharacter.Character)
                nextState = new IdleState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
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
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
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
            case MoveButton.ATTACK:
                if (thisCharacter.IsFacingLeft())
                    nextState = new StandingKickLeftState(thisCharacter);
                else
                    nextState = new StandingKickRightState(thisCharacter);
                break;
            case MoveButton.SWITCH:
                SignalManager.Inst.FireSignal(new CharacterSwitchSignal());
                nextState = new WaitToStart(thisCharacter);
                break;
        }
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter) 
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class StandingKickLeftState : CharacterState
{
    private bool attackLocked = true;
    private CharacterState nextState;

    public StandingKickLeftState(MainCharacter thisCharacter) : base (thisCharacter)
    {
        nextState = this;
    }

    public override void Start()
    {
        this.thisCharacter.SetHorizFlip(true);
        SignalManager.Inst.AddListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.AddListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
        thisCharacter.FireAnimationTrigger("StandingKick");
    }

    public override CharacterState Update()
    {
        if (attackLocked)
            return this;
        else
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else
                return nextState;
        }
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.RemoveListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onAttackLockReleased(Signal signal)
    {
        attackLocked = false;
    }

    private void onAttackAnimationFinished(Signal signal)
    {
        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            nextState = new RunLeftState(thisCharacter);
        else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            nextState = new RunRightState(thisCharacter);
        else
            nextState = new IdleState(thisCharacter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if(!attackLocked && moveButtonPressedSignal.moveButton == MoveButton.JUMP)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                nextState = new JumpRightState(thisCharacter);
            else
                nextState = new JumpLeftState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class StandingKickRightState : CharacterState
{
    private bool attackLocked = true;
    private CharacterState nextState;

    public StandingKickRightState(MainCharacter thisCharacter) : base(thisCharacter)
    {
        nextState = this;
    }

    public override void Start()
    {
        this.thisCharacter.SetHorizFlip(false);
        SignalManager.Inst.AddListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.AddListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
        thisCharacter.FireAnimationTrigger("StandingKick");
    }

    public override CharacterState Update()
    {
        if (attackLocked)
            return this;
        else
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
            else
                return nextState;
        }
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.RemoveListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onAttackLockReleased(Signal signal)
    {
        attackLocked = false;
    }

    private void onAttackAnimationFinished(Signal signal)
    {
        if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            nextState = new RunRightState(thisCharacter);
        else if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            nextState = new RunLeftState(thisCharacter);
        else
            nextState = new IdleState(thisCharacter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (!attackLocked && moveButtonPressedSignal.moveButton == MoveButton.JUMP)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                nextState = new JumpLeftState(thisCharacter);
            else
                nextState = new JumpRightState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
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
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (thisCharacter.IsBlockedLeft)
            return new PushLeftState(thisCharacter);
        if(nextState == this)
            thisCharacter.MoveLeft();
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MoveButtonReleasedSignal>(onMoveButtonReleased);
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
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
            case MoveButton.ATTACK:
                nextState = new RunningKickLeftState(thisCharacter);
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
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
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
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (thisCharacter.IsBlockedRight)
            return new PushRightState(thisCharacter);
        if (nextState == this)
            thisCharacter.MoveRight();
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MoveButtonReleasedSignal>(onMoveButtonReleased);
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
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
            case MoveButton.ATTACK:
                nextState = new RunningKickRightState(thisCharacter);
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
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class PushLeftState : CharacterState
{
    private CharacterState nextState;

    public PushLeftState(MainCharacter thisCharacter) : base(thisCharacter)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.FireAnimationTrigger("Push");
        thisCharacter.SetHorizFlip(true);
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (nextState != this)
            return nextState;
        if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            return new RunRightState(thisCharacter);
        else if (!InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            return new IdleState(thisCharacter);
        else if (thisCharacter.IsBlockedLeft)
            return this;
        else
            return new RunLeftState(thisCharacter);
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);

    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (moveButtonPressedSignal.moveButton == MoveButton.JUMP)
            nextState = new JumpLeftState(thisCharacter);
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class PushRightState : CharacterState
{
    private CharacterState nextState;

    public PushRightState(MainCharacter thisCharacter) : base(thisCharacter)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.FireAnimationTrigger("Push");
        thisCharacter.SetHorizFlip(false);
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (nextState != this)
            return nextState;
        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            return new RunLeftState(thisCharacter);
        else if (!InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            return new IdleState(thisCharacter);
        else if (thisCharacter.IsBlockedRight)
            return this;
        else
            return new RunRightState(thisCharacter);
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (moveButtonPressedSignal.moveButton == MoveButton.JUMP)
            nextState = new JumpRightState(thisCharacter);
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class RunningKickLeftState : CharacterState
{
    private bool attackLocked = true;
    private CharacterState nextState;

    public RunningKickLeftState(MainCharacter thisCharacter) : base(thisCharacter)
    {
        nextState = this;
    }

    public override void Start()
    {
        this.thisCharacter.SetHorizFlip(true);
        SignalManager.Inst.AddListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.AddListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
        thisCharacter.FireAnimationTrigger("RunningKick");
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(DieState))
            return nextState;
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;
        else if (attackLocked)
            return this;
        else if (nextState == this)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return nextState;
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else
                return new IdleState(thisCharacter);
        }
        else
            return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.RemoveListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onAttackLockReleased(Signal signal)
    {
        attackLocked = false;
    }

    private void onAttackAnimationFinished(Signal signal)
    {
        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            nextState = new RunLeftState(thisCharacter);
        else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            nextState = new RunRightState(thisCharacter);
        else
            nextState = new IdleState(thisCharacter);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (!attackLocked && moveButtonPressedSignal.moveButton == MoveButton.JUMP)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                nextState = new JumpRightState(thisCharacter);
            else
                nextState = new JumpLeftState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class RunningKickRightState : CharacterState
{
    private bool attackLocked = true;
    private CharacterState nextState;

    public RunningKickRightState(MainCharacter thisCharacter) : base(thisCharacter)
    {
        nextState = this;
    }

    public override void Start()
    {
        this.thisCharacter.SetHorizFlip(false);
        SignalManager.Inst.AddListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.AddListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
        thisCharacter.FireAnimationTrigger("RunningKick");
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(DieState))
            return nextState;
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;
        else if (attackLocked)
            return this;
        else if (nextState == this)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return nextState;
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
            else
                return new IdleState(thisCharacter);
        }
        else
            return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.RemoveListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onAttackLockReleased(Signal signal)
    {
        attackLocked = false;
    }

    private void onAttackAnimationFinished(Signal signal)
    {
        if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
            nextState = new RunLeftState(thisCharacter);
        else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
            nextState = new RunRightState(thisCharacter);
        else
            nextState = new IdleState(thisCharacter);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (!attackLocked && moveButtonPressedSignal.moveButton == MoveButton.JUMP)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                nextState = new JumpLeftState(thisCharacter);
            else
                nextState = new JumpRightState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
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
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(DieState))
            return nextState;
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
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
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
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(DieState))
            return nextState;
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
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
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
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(DieState))
            return nextState;
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;
        if (nextState.GetType() == typeof(FlyingKickLeftState) || nextState.GetType() == typeof(FlyingKickRightState))
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
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (moveButtonPressedSignal.moveButton == MoveButton.ATTACK)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                nextState = new FlyingKickRightState(thisCharacter);
            else
                nextState = new FlyingKickLeftState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
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
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(DieState))
            return nextState;
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;
        if (nextState.GetType() == typeof(FlyingKickLeftState) || nextState.GetType() == typeof(FlyingKickRightState))
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
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (moveButtonPressedSignal.moveButton == MoveButton.ATTACK)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                nextState = new FlyingKickLeftState(thisCharacter);
            else
                nextState = new FlyingKickRightState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
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
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(DieState))
            return nextState;
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;
        if (nextState.GetType() == typeof(FlyingKickLeftState) || nextState.GetType() == typeof(FlyingKickRightState))
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
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (moveButtonPressedSignal.moveButton == MoveButton.ATTACK)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                nextState = new FlyingKickRightState(thisCharacter);
            else
                nextState = new FlyingKickLeftState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
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
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(DieState))
            return nextState;
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;
        if (nextState.GetType() == typeof(FlyingKickLeftState) || nextState.GetType() == typeof(FlyingKickRightState))
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
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (moveButtonPressedSignal.moveButton == MoveButton.ATTACK)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                nextState = new FlyingKickLeftState(thisCharacter);
            else
                nextState = new FlyingKickRightState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class FlyDownLeftState : CharacterState
{
    private CharacterState nextState;
    private bool canJumpKick = false;

    public FlyDownLeftState(MainCharacter character) : base(character)
    {
        nextState = this;
    }

    public override void Start()
    {
        this.canJumpKick = thisCharacter.CanJumpKick;
        thisCharacter.SetHorizFlip(true);
        thisCharacter.FireAnimationTrigger("JumpDown");
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(DieState))
            return nextState;
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;
        if (nextState.GetType() == typeof(FlyingKickLeftState) || nextState.GetType() == typeof(FlyingKickRightState))
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
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (moveButtonPressedSignal.moveButton == MoveButton.ATTACK)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                nextState = new FlyingKickRightState(thisCharacter);
            else
                nextState = new FlyingKickLeftState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class FlyDownRightState : CharacterState
{
    private CharacterState nextState;
    private bool canJumpKick = false;

    public FlyDownRightState(MainCharacter character) : base(character)
    {
        nextState = this;
    }

    public override void Start()
    {
        this.canJumpKick = thisCharacter.CanJumpKick;
        thisCharacter.SetHorizFlip(false);
        thisCharacter.FireAnimationTrigger("JumpDown");
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(DieState))
            return nextState;
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;
        if (nextState.GetType() == typeof(FlyingKickLeftState) || nextState.GetType() == typeof(FlyingKickRightState))
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
        SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (moveButtonPressedSignal.moveButton == MoveButton.ATTACK)
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                nextState = new FlyingKickLeftState(thisCharacter);
            else
                nextState = new FlyingKickRightState(thisCharacter);
        }
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class FlyingKickLeftState : CharacterState
{
    private bool attackLocked = true;
    private CharacterState nextState;

    public FlyingKickLeftState(MainCharacter thisCharacter) : base(thisCharacter)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.CanJumpKick = false;
        this.thisCharacter.KickBoost();
        this.thisCharacter.SetHorizFlip(true);
        SignalManager.Inst.AddListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.AddListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
        thisCharacter.FireAnimationTrigger("FlyingKick");
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;
        else if (attackLocked)
            return this;
        else
        {
            if(thisCharacter.IsGrounded)
            {
                if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                    return new RunLeftState(thisCharacter);
                else if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                    return new RunRightState(thisCharacter);
                else
                    return new IdleState(thisCharacter);
            }
            return nextState;
        }
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.RemoveListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onAttackLockReleased(Signal signal)
    {
        attackLocked = false;
    }

    private void onAttackAnimationFinished(Signal signal)
    {
        if (thisCharacter.IsUpping())
        {
            if(InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                nextState = new FlyUpRightState(thisCharacter);
            else
                nextState = new FlyUpLeftState(thisCharacter);
        }
        else if (thisCharacter.IsTopping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                nextState = new FlyTopRightState(thisCharacter);
            else
                nextState = new FlyTopLeftState(thisCharacter);
        }
        else if (thisCharacter.IsDowning())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                nextState = new FlyDownRightState(thisCharacter);
            else
                nextState = new FlyDownLeftState(thisCharacter);
        }
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class FlyingKickRightState : CharacterState
{
    private bool attackLocked = true;
    private CharacterState nextState;

    public FlyingKickRightState(MainCharacter thisCharacter) : base(thisCharacter)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisCharacter.CanJumpKick = false;
        this.thisCharacter.KickBoost();
        this.thisCharacter.SetHorizFlip(false);
        SignalManager.Inst.AddListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.AddListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.AddListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.AddListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
        thisCharacter.FireAnimationTrigger("FlyingKick");
    }

    public override CharacterState Update()
    {
        if (nextState.GetType() == typeof(EnterJelloporterState))
            return nextState;
        else if (attackLocked)
            return this;
        else
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
            return nextState;
        }
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<AttackLockReleasedSignal>(onAttackLockReleased);
        SignalManager.Inst.RemoveListener<AttackAnimationFinishedSignal>(onAttackAnimationFinished);
        SignalManager.Inst.RemoveListener<PlayerHitJelloporterSignal>(onPlayerHitJelloporter);
        SignalManager.Inst.RemoveListener<MonsterAttackedPlayerSignal>(onMonsterAttackedPlayer);
    }

    private void onAttackLockReleased(Signal signal)
    {
        attackLocked = false;
    }

    private void onAttackAnimationFinished(Signal signal)
    {
        if (thisCharacter.IsUpping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                nextState = new FlyUpLeftState(thisCharacter);
            else
                nextState = new FlyUpRightState(thisCharacter);
        }
        else if (thisCharacter.IsTopping())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                nextState = new FlyTopLeftState(thisCharacter);
            else
                nextState = new FlyTopRightState(thisCharacter);
        }
        else if (thisCharacter.IsDowning())
        {
            if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                nextState = new FlyDownLeftState(thisCharacter);
            else
                nextState = new FlyDownRightState(thisCharacter);
        }
    }

    private void onPlayerHitJelloporter(Signal signal)
    {
        PlayerHitJelloporterSignal playerHitJelloporterSignal = (PlayerHitJelloporterSignal)signal;
        if (playerHitJelloporterSignal.player == thisCharacter)
            nextState = new EnterJelloporterState(thisCharacter, playerHitJelloporterSignal.jelloporter);
    }

    private void onMonsterAttackedPlayer(Signal signal)
    {
        MonsterAttackedPlayerSignal monsterAttackedPlayerSignal = (MonsterAttackedPlayerSignal)signal;
        if (monsterAttackedPlayerSignal.player == thisCharacter)
            nextState = new DieState(thisCharacter);
    }
}

public class EnterJelloporterState : CharacterState {
    public Jelloporter jelloporter;
    public EnterJelloporterState (MainCharacter thisCharacter, Jelloporter jelloporter) : base(thisCharacter)
    {
        this.jelloporter = jelloporter;
    }

    public override void Start()
    {
        thisCharacter.transform.parent = jelloporter.transform;
        thisCharacter.StopMove();
        thisCharacter.StopJump();
        thisCharacter.SetKinematic(true);
        thisCharacter.FireAnimationTrigger("Idle");
    }

    public override CharacterState Update()
    {
        if (thisCharacter.EnterJelloporter(jelloporter))
            return new RideJelloporterState(thisCharacter, jelloporter);
        else
            return this;
    }
}

public class RideJelloporterState : CharacterState
{
    Jelloporter jelloporter;
    CharacterState nextState;

    public RideJelloporterState(MainCharacter thisCharacter, Jelloporter jelloporter) : base(thisCharacter)
    {
        this.jelloporter = jelloporter;
        this.nextState = this;
    }

    public override void Start()
    {
        SignalManager.Inst.AddListener<JelloporterArrivedAtPlateSignal>(onJelloporterArrivedAtPlate);
        JelloState nextJelloState = jelloporter.ChooseNextState();
        SignalManager.Inst.FireSignal(new JelloportationStartedSignal(nextJelloState, jelloporter.ChooseDestination(nextJelloState)));
    }

    public override CharacterState Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<JelloporterArrivedAtPlateSignal>(onJelloporterArrivedAtPlate);
    }

    private void onJelloporterArrivedAtPlate(Signal signal)
    {
        JelloporterArrivedAtPlateSignal jelloporterArrivedAtPlateSignal = (JelloporterArrivedAtPlateSignal)signal;
        if (jelloporterArrivedAtPlateSignal.jelloporter == jelloporter)
            nextState = new ExitJelloporterState(thisCharacter, jelloporter);
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
        thisCharacter.wayPoint = jelloporter.GetCurrentPlate();
        SignalManager.Inst.FireSignal(new PlayerExitingJelloporterSignal(jelloporter));
        thisCharacter.transform.SetParent(null);
        thisCharacter.SetKinematic(false);
        Vector2 exitVelocity = new Vector2(-5f, 10f);
        if (jelloporter.ExitRight)
        {
            exitVelocity = new Vector2(5f, 10f);
            thisCharacter.SetHorizFlip(false);
        }
        else
            thisCharacter.SetHorizFlip(true);
        thisCharacter.SnapToJelloporterAndSetExitVelocity(jelloporter, exitVelocity);
    }

    public override CharacterState Update()
    {
        if (thisCharacter.IsGrounded)
        {
            SignalManager.Inst.FireSignal(new JelloportationFinishedSignal());
            if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                return new RunRightState(thisCharacter);
            else if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                return new RunLeftState(thisCharacter);
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

public class DieState : CharacterState
{
    private float deathTimer = 2f;

    public DieState(MainCharacter thisCharacter) : base(thisCharacter)
    {

    }

    public override void Start()
    {
        thisCharacter.FireAnimationTrigger("Invisible");
        thisCharacter.DisableAllColliders();
        thisCharacter.SetKinematic(true);
        thisCharacter.FireDeathBlast();
    }

    public override CharacterState Update()
    {
        deathTimer -= Time.deltaTime;
        if (deathTimer <= 0f)
            return new RespawnState(thisCharacter);
        else
            return this;
    }
}

public class RespawnState : CharacterState
{
    JelloPlate spawnPlate;
    float HackyColliderDelay = .1f;

    public RespawnState(MainCharacter thisCharacter) : base(thisCharacter)
    {
        spawnPlate = thisCharacter.GetWaypoint();
    }

    public override void Start()
    {
        thisCharacter.transform.SetParent(null);
        thisCharacter.EnableMainCollider();
        thisCharacter.SetKinematic(false);
        Vector2 exitVelocity = new Vector2(-5f, 10f);
        if (spawnPlate.ExitRight)
        {
            exitVelocity = new Vector2(5f, 10f);
            thisCharacter.SetHorizFlip(false);
        }
        else
            thisCharacter.SetHorizFlip(true);
        thisCharacter.SnapToPlateAndSetExitVelocity(spawnPlate, exitVelocity);
    }

    public override CharacterState Update()
    {
        HackyColliderDelay -= Time.deltaTime;
        if (HackyColliderDelay < 0f && thisCharacter.IsGrounded)
        {
            if(GameStateManager.Inst.CurrentState.GetType() == typeof(PlayState))
            {
                PlayState currentState = (PlayState)GameStateManager.Inst.CurrentState;
                if (currentState.character == thisCharacter.Character)
                {
                    if (InputHandler.Inst.ButtonIsDown(MoveButton.RIGHT))
                        return new RunRightState(thisCharacter);
                    else if (InputHandler.Inst.ButtonIsDown(MoveButton.LEFT))
                        return new RunLeftState(thisCharacter);
                    else
                        return new IdleState(thisCharacter);
                }
            }
            return new WaitToStart(thisCharacter);
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

    public override void End()
    {
        thisCharacter.EnableMainCollider();
    }
}