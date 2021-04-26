using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Inst;

    private State currentState = null;
    public State CurrentState { get { return currentState; } }

    private void Awake()
    {
        Inst = this;
    }

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
        SignalManager.Inst.AddListener<MenuClosedSignal>(onMenuClosed);
    }

    public override State Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.AddListener<MenuClosedSignal>(onMenuClosed);
    }

    private void onMenuClosed(Signal signal)
    {
        nextState = new IntroState();
    }
}

public class IntroState : State
{
    private State nextState;

    public IntroState()
    {
        nextState = this;
    }

    public override void Start()
    {
        SignalManager.Inst.AddListener<CutSceneFinishedSignal>(onCutSceneFinished);
    }

    public override State Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<CutSceneFinishedSignal>(onCutSceneFinished);
    }

    private void onCutSceneFinished(Signal signal)
    {
        nextState = new PlayState(Character.ICE_GIRL);
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
        SignalManager.Inst.AddListener<CutSceneStartingSignal>(onCutSceneStarting);
    }

    public override State Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<JelloportationStartedSignal>(onJelloportationStarted);
        SignalManager.Inst.RemoveListener<CharacterSwitchSignal>(onCharacterSwitched);
        SignalManager.Inst.RemoveListener<CutSceneStartingSignal>(onCutSceneStarting);
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

    private void onCutSceneStarting(Signal signal)
    {
        nextState = new WaitForCutSceneState(character);
    }

}

public class WaitForCutSceneState : State
{
    private Character activeCharacter;
    private State nextState;

    public WaitForCutSceneState(Character activeCharacter)
    {
        this.activeCharacter = activeCharacter;
        this.nextState = this;
    }

    public override void Start()
    {
        SignalManager.Inst.AddListener<CutSceneFinishedSignal>(onCutSceneFinished);
    }

    public override State Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<CutSceneFinishedSignal>(onCutSceneFinished);
    }

    private void onCutSceneFinished(Signal signal)
    {
        nextState = new PlayState(activeCharacter);
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