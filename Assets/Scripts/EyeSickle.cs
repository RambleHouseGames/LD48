using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeSickle : Monster
{
    [SerializeField]
    private GameObject LeftBoundary;

    [SerializeField]
    private GameObject RightBoundary;

    [SerializeField]
    private float moveSpeed = 1f;

    private SpriteRenderer myRenderer = null;

    private void Start()
    {
        LeftBoundary.transform.SetParent(null);
        RightBoundary.transform.SetParent(null);
    }

    public override void Update()
    {
        if (myRenderer == null)
            myRenderer = GetComponent<SpriteRenderer>();
        if (myRenderer.flipX)
            walkRight();
        else
            walkLeft();
        base.Update();
    }

    private void walkLeft()
    {
        float distanceToBoundary = Mathf.Abs(transform.position.x - LeftBoundary.transform.position.x);
        if (distanceToBoundary < moveSpeed * Time.deltaTime)
        {

            transform.position = new Vector2(LeftBoundary.transform.position.x, transform.position.y);
            if (myRenderer == null)
                myRenderer = GetComponent<SpriteRenderer>();
            myRenderer.flipX = true;
        }
        else
        {
            transform.position = new Vector2(transform.position.x - (moveSpeed * Time.deltaTime), transform.position.y);
        }
    }

    private void walkRight()
    {
        float distanceToBoundary = Mathf.Abs(transform.position.x - RightBoundary.transform.position.x);
        if (distanceToBoundary < moveSpeed * Time.deltaTime)
        {
            transform.position = new Vector2(RightBoundary.transform.position.x, transform.position.y);
            if (myRenderer == null)
                myRenderer = GetComponent<SpriteRenderer>();
            myRenderer.flipX = false;
        }
        else
        {
            transform.position = new Vector2(transform.position.x + (moveSpeed * Time.deltaTime), transform.position.y);
        }
    }
}
