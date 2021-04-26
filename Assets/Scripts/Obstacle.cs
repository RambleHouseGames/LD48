using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField]
    private ObstacleButton controlButton;

    [SerializeField]
    private GameObject pressedPosition;

    [SerializeField]
    private GameObject unpressedPosition;

    [SerializeField]
    private float moveSpeed;

    public void Update()
    {
        if (controlButton.IsPressed)
            moveTowardPosition(pressedPosition);
        else
            moveTowardPosition(unpressedPosition);
    }

    private void moveTowardPosition(GameObject position)
    {
        float distance = Vector2.Distance(transform.position, position.transform.position);
        if (distance < moveSpeed * Time.deltaTime)
            transform.position = position.transform.position;
        else
            transform.position = Vector2.MoveTowards(transform.position, position.transform.position, moveSpeed * Time.deltaTime);
    }
}
