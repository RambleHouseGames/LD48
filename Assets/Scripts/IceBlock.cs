using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBlock : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 1;

    [SerializeField]
    private TouchTrigger rightTouchTrigger;

    [SerializeField]
    private TouchTrigger leftTouchTrigger;

    private Rigidbody2D myRigidBody = null;

    public void Update()
    {
        if (myRigidBody == null)
            myRigidBody = GetComponent<Rigidbody2D>();
        myRigidBody.velocity = new Vector2(0f, myRigidBody.velocity.y);
    }

    public void PushRight()
    {
        if(!rightTouchTrigger.IsTriggered)
            transform.position = new Vector2(transform.position.x - (moveSpeed * Time.deltaTime), transform.position.y);
    }

    public void PushLeft()
    {
        Debug.Log("Gettin Pushed");
        if (!leftTouchTrigger.IsTriggered)
            transform.position = new Vector2(transform.position.x + (moveSpeed * Time.deltaTime), transform.position.y);
    }
}
