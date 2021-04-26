using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleButton : MonoBehaviour
{
    [SerializeField]
    private Sprite unpressedSprite;

    [SerializeField]
    private Sprite pressedSprite;

    private bool isPressed = false;
    public bool IsPressed { get { return isPressed; } }

    private float spamTimer = 0f;

    private SpriteRenderer myRenderer = null;

    public void Update()
    {
        if (spamTimer > 0f)
            spamTimer -= Time.deltaTime;
    }

    private void SetIsPressed(bool newIsPressed)
    {
        if (myRenderer == null)
            myRenderer = GetComponent<SpriteRenderer>();

        if (newIsPressed)
            myRenderer.sprite = pressedSprite;
        else
            myRenderer.sprite = unpressedSprite;

        isPressed = newIsPressed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (spamTimer <= 0f)
        {
            if (collision.tag == "Player" || collision.tag == "PlayerAttack")
            {
                spamTimer = .5f;
                SetIsPressed(!isPressed);
            }
        }
    }
}
