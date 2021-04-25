using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jelloporter : MonoBehaviour
{
    [SerializeField]
    private Jelloporter connectedJelloporter;
    public Jelloporter ConnectedJelloporter { get { return connectedJelloporter; } }

    [SerializeField]
    private bool exitRight = true;
    public bool ExitRight { get { return exitRight; } }

    [SerializeField]
    private GameObject cameraHolder;
    public GameObject CameraHolder { get { return cameraHolder; } }

    private Animator myAnimator = null;

    private void Start()
    {
        SignalManager.Inst.AddListener<PlayerExitingJelloporterSignal>(onPlayerExitingJelloporter);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (connectedJelloporter != null && other.tag == "Player")
        {
            MainCharacter player = other.GetComponent<MainCharacter>();
            SignalManager.Inst.FireSignal(new PlayerHitJelloporterSignal(this, player));
            if (myAnimator == null)
                myAnimator = GetComponent<Animator>();
            myAnimator.SetTrigger("GetEntered");
        }
        
    }

    private void onPlayerExitingJelloporter(Signal signal)
    {
        PlayerExitingJelloporterSignal playerExitingJelloporterSignal = (PlayerExitingJelloporterSignal)signal;
        if(playerExitingJelloporterSignal.jelloporter == this)
        {
            if (myAnimator == null)
                myAnimator = GetComponent<Animator>();

            myAnimator.SetTrigger("GetExited");
        }
    }
}
