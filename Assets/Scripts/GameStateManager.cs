using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    private State currentState = null;

    private void Update()
    {
        if(currentState == null)
        {
            currentState = new StartMenuState();
            currentState.Start();
            SignalManager.Inst.FireSignal(new StateStartedSignal(currentState));
        }
        State nextState = currentState.Update();
        if(nextState != currentState)
        {
            currentState.End();
            SignalManager.Inst.FireSignal(new StateEndingSignal(currentState));
            currentState = nextState;
            nextState.Start();
            SignalManager.Inst.FireSignal(new StateStartedSignal(currentState));
        }
    }
}

public abstract class State
{
    public virtual void Start() { }
    public virtual State Update() { return this; }
    public virtual void End() { }
}

public class StartMenuState : State
{
    private State nextState;

    public StartMenuState()
    {
        this.nextState = this;
    }

    public override void Start()
    {
        SignalManager.Inst.AddListener<ButtonPressedSignal>(onButtonPressed);
    }

    public override State Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.AddListener<ButtonPressedSignal>(onButtonPressed);
    }

    private void onButtonPressed(Signal signal)
    {
        ButtonPressedSignal buttonPressedSignal = (ButtonPressedSignal)signal;
        if (buttonPressedSignal.buttonType == ButtonType.START)
            nextState = new IntroState();
    }
}

public class IntroState : State
{
    public override void Start()
    {
        
    }
}