using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
    void Start()
    {
        SignalManager.Inst.AddListener<StateStartedSignal>(onStateStarted);
    }

    private void onStateStarted(Signal signal)
    {
        StateStartedSignal stateStartedSignal = (StateStartedSignal)signal;
        gameObject.SetActive(stateStartedSignal.startedState.GetType() == typeof(StartMenuState));
    }
}
