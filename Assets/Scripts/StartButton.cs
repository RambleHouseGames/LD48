using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    [SerializeField]
    private Sprite selectedSprite;

    [SerializeField]
    private Sprite unselectedSprite;

    private bool selected = true;

    private SpriteRenderer myRenderer;

    private bool infoMode = false;

    private void Start()
    {
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
        if (!infoMode && (moveButtonPressedSignal.moveButton == MoveButton.JUMP || moveButtonPressedSignal.moveButton == MoveButton.DOWN))
            setSelected(!selected);
        if(moveButtonPressedSignal.moveButton == MoveButton.ATTACK ||  moveButtonPressedSignal.moveButton == MoveButton.SWITCH)
        {
            if (selected)
            {
                SignalManager.Inst.RemoveListener<MoveButtonPressedSignal>(onMoveButtonPressed);
                SignalManager.Inst.FireSignal(new MenuClosedSignal());
            }
            else
                setInfoMode(!infoMode);
        }
    }

    private void setSelected(bool nextSelected)
    {
        if (myRenderer == null)
            myRenderer = GetComponent<SpriteRenderer>();

        if (nextSelected)
            myRenderer.sprite = selectedSprite;
        else
            myRenderer.sprite = unselectedSprite;

        selected = nextSelected;
    }

    private void setInfoMode(bool newInfoMode)
    {
        infoMode = newInfoMode;
    }
}
