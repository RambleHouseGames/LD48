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
        bool downIsPressed = Input.GetKey(KeyCode.S) || Input.GetAxis("Vertical") < -.2f || Input.GetAxis("DPad Y") < -.2f;
        bool upIsPressed = Input.GetKey(KeyCode.W) || Input.GetAxis("Vertical") > .2f || Input.GetAxis("DPad Y") > .2f;
        if (buttonsDown[MoveButton.LEFT] != (Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < -.2f || Input.GetAxis("DPad X") < -.2f))
        {
            if (Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < -.2f || Input.GetAxis("DPad X") < -.2f)
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
        if (buttonsDown[MoveButton.RIGHT] != Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > .2f || Input.GetAxis("DPad X") > .2f)
        {
            if (Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > .2f || Input.GetAxis("DPad X") > .2f)
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
        if (buttonsDown[MoveButton.JUMP] != upIsPressed)
        {
            if (upIsPressed)
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
        if (buttonsDown[MoveButton.DOWN] != downIsPressed)
        {
            if (downIsPressed)
            {
                SignalManager.Inst.FireSignal(new MoveButtonPressedSignal(MoveButton.DOWN));
                buttonsDown[MoveButton.DOWN] = true;
            }
            else
            {
                SignalManager.Inst.FireSignal(new MoveButtonReleasedSignal(MoveButton.DOWN));
                buttonsDown[MoveButton.DOWN] = false;
            }
        }
        if (buttonsDown[MoveButton.ATTACK] != (Input.GetKey(KeyCode.Space) || Input.GetKey("joystick button 0")))
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetKey("joystick button 0"))
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
        if (buttonsDown[MoveButton.SWITCH] != (Input.GetKey(KeyCode.E) || Input.GetKey("joystick button 1")))
        {
            if (Input.GetKey(KeyCode.E) || Input.GetKey("joystick button 1"))
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

public enum MoveButton { LEFT, RIGHT, JUMP, ATTACK, SWITCH, DOWN }