using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchTrigger : MonoBehaviour
{
    public bool IsTriggered { get; private set; }
    public GameObject blockingObject { get; private set; }

    private void OnTriggerStay2D(Collider2D collision)
    {
        IsTriggered = (collision != null) && (collision.tag == "ground" || collision.tag == "IceBlock");
        if (IsTriggered)
            blockingObject = collision.gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IsTriggered = false;
        blockingObject = null;
    }
}
