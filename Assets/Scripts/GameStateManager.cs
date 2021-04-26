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
        SignalManager.Inst.AddListener<CharacterSwitchSignal>(onCharacterSwitched);
    }

    public override State Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<JelloportationStartedSignal>(onJelloportationStarted);
        SignalManager.Inst.RemoveListener<CharacterSwitchSignal>(onCharacterSwitched);
    }

    private void onJelloportationStarted(Signal signal)
    {
        JelloportationStartedSignal jelloportationStartedSignal = (JelloportationStartedSignal)signal;
        nextState = new JelloportState(character, jelloportationStartedSignal.newJelloState);
    }

    private void onCharacterSwitched(Signal signal)
    {
        if (character == Character.ICE_GIRL)
            nextState = new PlayState(Character.BEEF_CAKE);
        else
            nextState = new PlayState(Character.ICE_GIRL);
    }
}

public class JelloportState : State
{
    public Character activeCharacter;
    public JelloState newJelloState;

    private State nextState;

    public JelloportState(Character activeCharacter, JelloState newJelloState)
    {
        this.activeCharacter = activeCharacter;
        this.newJelloState = newJelloState;
        nextState = this;
    }

    public override void Start()
    {
        SignalManager.Inst.AddListener<JelloportationFinishedSignal>(onJelloportationFinished);
    }

    public override State Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<JelloportationFinishedSignal>(onJelloportationFinished);
    }

    private void onJelloportationFinished(Signal signal)
    {
        nextState = new PlayState(activeCharacter);
    }
}

public enum Character { ICE_GIRL, BEEF_CAKE }
public enum JelloState { PINK_UP_GREEN_DOWN, PINK_DOWN_GREEN_UP }