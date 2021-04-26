using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jelloporter : MonoBehaviour
{
    [SerializeField]
    private JelloPlate topPlate;
    public JelloPlate TopPlate { get { return topPlate; } }

    [SerializeField]
    private JelloPlate bottomPlate;
    public JelloPlate BottomPlate { get { return bottomPlate; } }

    [SerializeField]
    private bool exitRight = true;
    public bool ExitRight { get { return exitRight; } }

    [SerializeField]
    private JelloporterColor jelloporterColor;
    public JelloporterColor JelloporterColor { get { return jelloporterColor; } }

    private Animator myAnimator = null;

    private JelloporterState currentState = null;

    private float moveSpeed = 5f;

    public bool RespondToCollisions = false;

    private void Update()
    {
        if (currentState == null)
        {
            currentState = new JelloIdleState(this);
            currentState.Start();
        }

        JelloporterState nextState = currentState.Update();
        if(nextState != currentState)
        {
            currentState.End();
            currentState = nextState;
            currentState.Start();
        }

    }

    public JelloPlate ChooseDestination(JelloState jelloState)
    {
        if(jelloporterColor == JelloporterColor.PINK)
        {
            if (jelloState == JelloState.PINK_UP_GREEN_DOWN)
                return topPlate;
            else
                return bottomPlate;
        }
        else
        {
            if (jelloState == JelloState.PINK_DOWN_GREEN_UP)
                return topPlate;
            else
                return bottomPlate;
        }
    }

    public JelloState ChooseNextState()
    {
        
        float distanceToTop = Vector2.Distance(transform.position, topPlate.transform.position);
        float distanceToBottom = Vector2.Distance(transform.position, bottomPlate.transform.position);
        if (distanceToTop < distanceToBottom)
        {
            if (jelloporterColor == JelloporterColor.PINK)
                return JelloState.PINK_DOWN_GREEN_UP;
            else
                return JelloState.PINK_UP_GREEN_DOWN;
        }
        else
        {
            if (jelloporterColor == JelloporterColor.PINK)
                return JelloState.PINK_UP_GREEN_DOWN;
            else
                return JelloState.PINK_DOWN_GREEN_UP;
        }
    }

    public bool moveTowardTopPlate()
    {
        float distance = Vector2.Distance(transform.position, topPlate.transform.position);
        if (distance < moveSpeed * Time.deltaTime)
        {
            transform.position = topPlate.transform.position;
            return true;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, topPlate.transform.position, moveSpeed * Time.deltaTime);
            return false;
        }
    }

    public bool moveTowardBottomPlate()
    {
        float distance = Vector2.Distance(transform.position, bottomPlate.transform.position);
        if (distance < moveSpeed * Time.deltaTime)
        {
            transform.position = bottomPlate.transform.position;
            return true;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, bottomPlate.transform.position, moveSpeed * Time.deltaTime);
            return false;
        }
    }

    public void SetAnimationTrigger(string trigger)
    {
        if (myAnimator == null)
            myAnimator = GetComponent<Animator>();
        myAnimator.SetTrigger(trigger);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (RespondToCollisions)
        {
            MainCharacter player = other.GetComponent<MainCharacter>();
            SignalManager.Inst.FireSignal(new PlayerHitJelloporterSignal(this, player));
            if (myAnimator == null)
                myAnimator = GetComponent<Animator>();
            myAnimator.SetTrigger("GetEntered");
        }
    }
}

public abstract class JelloporterState : State
{
    protected Jelloporter thisJelloporter;
    public JelloporterState(Jelloporter thisJelloporter)
    {
        this.thisJelloporter = thisJelloporter;
    }

    public virtual JelloporterState Update()
    {
        return (JelloporterState)base.Update();
    }
}

public class JelloIdleState : JelloporterState
{
    private JelloporterState nextState;

    public JelloIdleState(Jelloporter thisJelloporter) : base (thisJelloporter)
    {
        this.nextState = this;
    }

    public override void Start()
    {
        thisJelloporter.RespondToCollisions = true;
        SignalManager.Inst.AddListener<JelloportationStartedSignal>(onJelloportationStarted);
    }

    public override JelloporterState Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<JelloportationStartedSignal>(onJelloportationStarted);
    }

    private void onJelloportationStarted(Signal signal)
    {
        JelloportationStartedSignal jelloportationStartedSignal = (JelloportationStartedSignal)signal;
        JelloPlate destination = thisJelloporter.ChooseDestination(jelloportationStartedSignal.newJelloState);
        if (destination == thisJelloporter.TopPlate)
            nextState = new JumpToTopPlate(thisJelloporter);
        else
            nextState = new JumpToBottomPlate(thisJelloporter);
    }
}

public class JumpToTopPlate : JelloporterState
{
    private bool hasArrived = false;
    private JelloporterState nextState;

    public JumpToTopPlate(Jelloporter thisJelloporter) : base(thisJelloporter)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisJelloporter.RespondToCollisions = false;
        SignalManager.Inst.AddListener<JelloportationFinishedSignal>(onJelloportationFinished);
    }

    public override JelloporterState Update()
    {
        if (!hasArrived && thisJelloporter.moveTowardTopPlate())
        {
            hasArrived = true;
            SignalManager.Inst.FireSignal(new JelloporterArrivedAtPlateSignal(thisJelloporter));
        }
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<JelloportationFinishedSignal>(onJelloportationFinished);
    }

    private void onJelloportationFinished(Signal signal)
    {
        nextState = new JelloIdleState(thisJelloporter);
    }
}

public class JumpToBottomPlate : JelloporterState
{
    private bool hasArrived = false;
    private JelloporterState nextState;

    public JumpToBottomPlate(Jelloporter thisJelloporter) : base(thisJelloporter)
    {
        nextState = this;
    }

    public override void Start()
    {
        thisJelloporter.RespondToCollisions = false;
        SignalManager.Inst.AddListener<JelloportationFinishedSignal>(onJelloportationFinished);
        SignalManager.Inst.AddListener<PlayerExitingJelloporterSignal>(onPlayerExitingJelloporter);
    }

    public override JelloporterState Update()
    {
        if (!hasArrived && thisJelloporter.moveTowardBottomPlate())
        {
            hasArrived = true;
            SignalManager.Inst.FireSignal(new JelloporterArrivedAtPlateSignal(thisJelloporter));
        }
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<JelloportationFinishedSignal>(onJelloportationFinished);
    }

    private void onJelloportationFinished(Signal signal)
    {
        nextState = new JelloIdleState(thisJelloporter);
    }

    private void onPlayerExitingJelloporter(Signal signal)
    {
        PlayerExitingJelloporterSignal playerExitingJelloporterSignal = (PlayerExitingJelloporterSignal)signal;
        if (playerExitingJelloporterSignal.jelloporter == thisJelloporter)
            thisJelloporter.SetAnimationTrigger("GetExited");
    }
}




public enum JelloporterColor { GREEN, PINK }