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
            nextState = new PlayState(Character.ICE_GIRL);
    }
}

public class IntroState : State
{
    public override void Start()
    {
        
    }
}

public class PlayState : State
{
    private State nextState;

    public Character character { get; private set; }

    public PlayState(Character character)
    {
        this.nextState = this;
        this.character = character;
    }

    public override void Start()
    {
        SignalManager.Inst.AddListener<JelloportationStartedSignal>(onJelloportationStarted);
    }

    public override State Update()
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
        nextState = new JelloportState(jelloportationStartedSignal.destinationJelloporter, jelloportationStartedSignal.character);
    }
}

public class JelloportState : State
{
    private Jelloporter destinationJelloporter;
    private MainCharacter character;

    public JelloportState(Jelloporter destinationJelloporter, MainCharacter character)
    {
        this.destinationJelloporter = destinationJelloporter;
        this.character = character;
    }

    public override void Start()
    {
        CameraController.Inst.SetParent(destinationJelloporter.CameraHolder);
    }

    public override State Update()
    {
        float perportion = CameraController.Inst.MoveTowardParent();
        character.MoveTowardJelloporter(destinationJelloporter, perportion);
        if (perportion >= 1f)
        {
            if (character.Character == Character.ICE_GIRL)
                return new PlayState(Character.BEEF_CAKE);
            else
                return new PlayState(Character.ICE_GIRL);
        }
        else
            return this;
    }
}

public enum Character { ICE_GIRL, BEEF_CAKE }