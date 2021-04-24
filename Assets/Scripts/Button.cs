using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField]
    private ButtonType buttonType;

    public void OnPressed()
    {
        SignalManager.Inst.FireSignal(new ButtonPressedSignal(buttonType));
    }

}

public enum ButtonType { START }