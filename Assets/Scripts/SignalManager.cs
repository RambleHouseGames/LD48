using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalManager : MonoBehaviour
{
    public static SignalManager Inst;

    private Dictionary<Type, Action<Signal>> listeners = null;

    private void Awake()
    {
        Inst = this;
    }

    public void AddListener<T>(Action<Signal> callback)
    {
        if (listeners == null)
            listeners = new Dictionary<Type, Action<Signal>>();
        if (listeners.ContainsKey(typeof(T)))
            listeners[typeof(T)] += callback;
        else
            listeners.Add(typeof(T), callback);
    }

    public void RemoveListener<T>(Action<Signal> callback)
    {
        listeners[typeof(T)] -= callback;
        if (listeners[typeof(T)] == null)
            listeners.Remove(typeof(T));
    }

    public void FireSignal(Signal signal)
    {
        if (listeners != null && listeners.ContainsKey(signal.GetType()))
            listeners[signal.GetType()](signal);
    }
}

public abstract class Signal { }
public class StateStartedSignal : Signal{
    public State startedState { get; private set; }
    public StateStartedSignal(State startedState)
    {
        this.startedState = startedState;
    }
}
public class StateEndingSignal : Signal {
    public State endingState { get; private set; }
    public StateEndingSignal(State endingState)
    {
        this.endingState = endingState;
    }
}
public class ButtonPressedSignal : Signal
{
    public ButtonType buttonType;
    public ButtonPressedSignal(ButtonType buttonType)
    {
        this.buttonType = buttonType;
    }
}
public class MoveButtonPressedSignal : Signal
{
    public MoveButton moveButton { get; private set; }
    public MoveButtonPressedSignal(MoveButton moveButton)
    {
        this.moveButton = moveButton;
    }
}
public class MoveButtonReleasedSignal : Signal
{
    public MoveButton moveButton { get; private set; }
    public MoveButtonReleasedSignal(MoveButton moveButton)
    {
        this.moveButton = moveButton;
    }
}
public class PlayerHitJelloporterSignal : Signal
{
    public Jelloporter jelloporter { get; private set; }
    public PlayerHitJelloporterSignal(Jelloporter jelloporter)
    {
        this.jelloporter = jelloporter;
    }
}
public class PlayerExitingJelloporterSignal : Signal
{
    public Jelloporter jelloporter { get; private set; }
    public PlayerExitingJelloporterSignal(Jelloporter jelloporter)
    {
        this.jelloporter = jelloporter;
    }
}
