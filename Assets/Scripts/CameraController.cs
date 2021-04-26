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

    public void Start()
    {
        SignalManager.Inst.AddListener<JelloportationStartedSignal>(onJelloportationStarted);
    }

    private void Update()
    {
        MoveTowardParent();
    }

    private void onJelloportationStarted(Signal signal)
    {
        JelloportationStartedSignal jelloportationStartedSignal = (JelloportationStartedSignal)signal;
        SetParent(jelloportationStartedSignal.destinationPlate.CameraHolder);
    }

    public void SetParent(GameObject newParent)
    {
        transform.SetParent(newParent.transform);
    }

    public void MoveTowardParent()
    {
        float distance = transform.localPosition.magnitude;
        if(distance < moveSpeed * Time.deltaTime)
            transform.localPosition = Vector3.zero;
        else
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime);
    }
}
