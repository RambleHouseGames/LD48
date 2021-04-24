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

    private Animator myAnimator = null;

    private void Start()
    {
        SignalManager.Inst.AddListener<PlayerExitingJelloporterSignal>(onPlayerExitingJelloporter);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            SignalManager.Inst.FireSignal(new PlayerHitJelloporterSignal(this));
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
