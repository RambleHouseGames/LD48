using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Inst;

    [SerializeField]
    private float moveSpeed = 5f;

    private void Awake()
    {
        Inst = this;
    }

    public void SetParent(GameObject newParent)
    {
        transform.SetParent(newParent.transform);
    }

    public float MoveTowardParent()
    {
        float distance = transform.localPosition.magnitude;
        if(distance < moveSpeed * Time.deltaTime)
        {
            transform.localPosition = Vector3.zero;
            return 1.1f;
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime);
            return (moveSpeed * Time.deltaTime) / distance;
        }
    }
}
