using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        SignalManager.Inst.AddListener<MenuClosedSignal>(onMenuClosed);
    }

    private void onMenuClosed(Signal signal)
    {
        gameObject.SetActive(false);
        SignalManager.Inst.AddListener<MenuClosedSignal>(onMenuClosed);
    }
}
