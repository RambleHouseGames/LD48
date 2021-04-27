using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicHandler : MonoBehaviour
{
    [SerializeField]
    private AudioMixer mixer;

    [SerializeField]
    private float fadeSpeed = 1f;

    private bool drumsAdded = false;
    private bool skipped = false;
    private bool pianoAdded = false;
    private bool orchestraAdded = false;

    private void Start()
    {
        mixer.SetFloat("Drum", -80f);
        mixer.SetFloat("Piano", -80f);
        mixer.SetFloat("Orchestral", -80f);
        SignalManager.Inst.AddListener<CutSceneFinishedSignal>(onCutSceneFinished);
        SignalManager.Inst.AddListener<CutSceneStartingSignal>(onCutSceneStarting);
    }

    public void Update()
    {
        float currentDrum;
        mixer.GetFloat("Drum", out currentDrum);
        if (drumsAdded && (currentDrum < 0))
        {
            float newDrum = currentDrum + (fadeSpeed * Time.deltaTime);
            if (newDrum > 0f)
                newDrum = 0f;
            mixer.SetFloat("Drum", newDrum);
        }

        float currentPiano;
        mixer.GetFloat("Piano", out currentPiano);
        if (pianoAdded && (currentPiano < 0))
        {
            float newPiano = currentPiano + (fadeSpeed * Time.deltaTime);
            if (newPiano > 0f)
                newPiano = 0f;
            mixer.SetFloat("Piano", newPiano);
        }

        float currentOrchestra;
        mixer.GetFloat("Orchestral", out currentOrchestra);
        if (orchestraAdded && (currentOrchestra < 0))
        {
            float newOrchestra = currentOrchestra + (fadeSpeed * Time.deltaTime);
            if (newOrchestra > 0f)
                newOrchestra = 0f;
            mixer.SetFloat("Orchestral", newOrchestra);
        }
    }

    private void onCutSceneFinished(Signal signal)
    {
        increaseMusicLayers();
    }

    private void onCutSceneStarting(Signal signal)
    {
        increaseMusicLayers();
    }

    private void increaseMusicLayers()
    {
        if (drumsAdded)
        {
            if (skipped)
            {
                if (pianoAdded)
                {
                    if (orchestraAdded)
                        return;
                    else
                    {
                        orchestraAdded = true;
                    }
                }
                else
                {
                    pianoAdded = true;
                }
            }
            else
                skipped = true;
        }
        else
        {
            drumsAdded = true;
        }
    }
}
