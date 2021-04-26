using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Inst;

    private Dictionary<MoveButton, bool> buttonsDown = new Dictionary<MoveButton, bool>();

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        foreach(MoveButton moveButton in Enum.GetValues(typeof(MoveButton)))
        {
            buttonsDown.Add(moveButton, false);
        }
    }

    private void Update()
    {
        if(buttonsDown[MoveButton.LEFT] != Input.GetKey(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.A))
            {
                SignalManager.Inst.FireSignal(new MoveButtonPressedSignal(MoveButton.LEFT));
                buttonsDown[MoveButton.LEFT] = true;
            }
            else
            {
                SignalManager.Inst.FireSignal(new MoveButtonReleasedSignal(MoveButton.LEFT));
                buttonsDown[MoveButton.LEFT] = false;
            }
        }
        if (buttonsDown[MoveButton.RIGHT] != Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.D))
            {
                SignalManager.Inst.FireSignal(new MoveButtonPressedSignal(MoveButton.RIGHT));
                buttonsDown[MoveButton.RIGHT] = true;
            }
            else
            {
                SignalManager.Inst.FireSignal(new MoveButtonReleasedSignal(MoveButton.RIGHT));
                buttonsDown[MoveButton.RIGHT] = false;
            }
        }
        if (buttonsDown[MoveButton.JUMP] != Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.W))
            {
                SignalManager.Inst.FireSignal(new MoveButtonPressedSignal(MoveButton.JUMP));
                buttonsDown[MoveButton.JUMP] = true;
            }
            else
            {
                SignalManager.Inst.FireSignal(new MoveButtonReleasedSignal(MoveButton.JUMP));
                buttonsDown[MoveButton.JUMP] = false;
            }
        }
        if (buttonsDown[MoveButton.ATTACK] != Input.GetKey(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.Space))
            {
                SignalManager.Inst.FireSignal(new MoveButtonPressedSignal(MoveButton.ATTACK));
                buttonsDown[MoveButton.ATTACK] = true;
            }
            else
            {
                SignalManager.Inst.FireSignal(new MoveButtonReleasedSignal(MoveButton.ATTACK));
                buttonsDown[MoveButton.ATTACK] = false;
            }
        }
        if (buttonsDown[MoveButton.SWITCH] != Input.GetKey(KeyCode.E))
        {
            if (Input.GetKey(KeyCode.E))
            {
                SignalManager.Inst.FireSignal(new MoveButtonPressedSignal(MoveButton.SWITCH));
                buttonsDown[MoveButton.SWITCH] = true;
            }
            else
            {
                SignalManager.Inst.FireSignal(new MoveButtonReleasedSignal(MoveButton.SWITCH));
                buttonsDown[MoveButton.SWITCH] = false;
            }
        }
    }

    public bool ButtonIsDown(MoveButton moveButton)
    {
        return buttonsDown[moveButton];
    }

}

public enum MoveButton { LEFT, RIGHT, JUMP, ATTACK, SWITCH }