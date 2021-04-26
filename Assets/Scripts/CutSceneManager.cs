using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutSceneManager : MonoBehaviour
{
    public static CutSceneManager Inst;

    [SerializeField]
    private List<CutSceneSequence> cutSceneSequences;

    [SerializeField]
    private SpriteRenderer backdrop;

    [SerializeField]
    private SpriteRenderer MainSlide;

    [SerializeField]
    private SpriteRenderer CrossFadeSlide;

    [SerializeField]
    private float fadeSpeed = 1f;

    [SerializeField]
    private float blackDelay = 1f;
    public float BlackDelay { get { return blackDelay; } }

    [SerializeField]
    private float inputDelayDuration = 2f;

    private CutSceneState currentState = null;
    private CutSceneSequence activeSequence = null;
    private int activeSlideIndex = 0;
    private float inputDelay;

    public bool InputDelayExpired
    {
        get
        {
            return inputDelay <= 0f;
        }
    }

    public bool ActiveSlideIsLastInSequence
    {
        get
        {
            return activeSequence.Slides.Count == activeSlideIndex + 1;
        }
    }

    public bool ActiveSlideIsEndGame
    {
        get
        {
            if (activeSequence == null)
                return false;
            else
                return activeSequence.Slides[activeSlideIndex].isEndGame;
        }
    }

    private void Awake()
    {
        inputDelay = inputDelayDuration;
        Inst = this;
    }

    private void Update()
    {
        if(inputDelay > 0f)
            inputDelay -= Time.deltaTime;

        if (currentState == null)
        {
            currentState = new BlackState();
            currentState.Start();
        }
        CutSceneState nextState = currentState.Update();
        if(nextState != currentState)
        {
            currentState.End();
            currentState = nextState;
            currentState.Start();
        }
    }

    public void SetActiveSequence(int sequenceIndex)
    {
        activeSequence = cutSceneSequences[sequenceIndex];
        activeSlideIndex = 0;
    }

    public void SetActiveSequence(CutSceneSequence sequence)
    {
        activeSequence = sequence;
        activeSlideIndex = 0;
    }

    public void SetBackDropOpacity(float percentage)
    {
        backdrop.color = new Color(backdrop.color.r, backdrop.color.g, backdrop.color.b, percentage);
    }

    public void PlaceActiveSlide()
    {
        MainSlide.color = new Color(MainSlide.color.r, MainSlide.color.g, MainSlide.color.b, 0f);
        MainSlide.sprite = activeSequence.Slides[activeSlideIndex].slide;
    }

    public bool FadeInActiveSlide()
    {
        float newAlpha = MainSlide.color.a + (fadeSpeed * Time.deltaTime);
        if (newAlpha >= 1f)
        {
            MainSlide.color = new Color(MainSlide.color.r, MainSlide.color.g, MainSlide.color.b, 1f);
            return true;
        }
        else
        {
            MainSlide.color = new Color(MainSlide.color.r, MainSlide.color.g, MainSlide.color.b, newAlpha);
            return false;
        }
    }

    public bool FadeToBlack()
    {
        float newAlpha = MainSlide.color.a - (fadeSpeed * Time.deltaTime);
        if(newAlpha <= 0f)
        {
            MainSlide.color = new Color(MainSlide.color.r, MainSlide.color.g, MainSlide.color.b, 0f);
            return true;
        }
        else
        {
            MainSlide.color = new Color(MainSlide.color.r, MainSlide.color.g, MainSlide.color.b, newAlpha);
            return false;
        }
    }

    public void IncrementActiveSlide()
    {
        activeSlideIndex++;
    }

    public void ResetInputDelay()
    {
        inputDelay = inputDelayDuration;
    }

    public FadeType GetNextFadeType()
    {
        return activeSequence.Slides[activeSlideIndex].fadeType;
    }

    public void PlaceNextSlideOnCrossFade()
    {
        CrossFadeSlide.color = new Color(CrossFadeSlide.color.r, CrossFadeSlide.color.g, CrossFadeSlide.color.b, 0f);
        CrossFadeSlide.sprite = activeSequence.Slides[activeSlideIndex + 1].slide;
    }

    public bool CrossFade()
    {
        float risingAlpha = CrossFadeSlide.color.a + (fadeSpeed * Time.deltaTime);
        if(risingAlpha >= 1f)
        {
            CrossFadeSlide.color = new Color(CrossFadeSlide.color.r, CrossFadeSlide.color.g, CrossFadeSlide.color.b, 1f);
            MainSlide.color = new Color(MainSlide.color.r, MainSlide.color.g, MainSlide.color.b, 0f);
            return true;
        }
        else
        {
            CrossFadeSlide.color = new Color(CrossFadeSlide.color.r, CrossFadeSlide.color.g, CrossFadeSlide.color.b, risingAlpha);
            MainSlide.color = new Color(MainSlide.color.r, MainSlide.color.g, MainSlide.color.b, 1f - risingAlpha);
            return false;
        }
    }

    public void PromoteCrossFadeToMain()
    {
        MainSlide.sprite = CrossFadeSlide.sprite;
        MainSlide.color = new Color(MainSlide.color.r, MainSlide.color.g, MainSlide.color.b, 1f);
        CrossFadeSlide.sprite = null;
    }

    public bool FadeInBackDrop()
    {
        float newAlpha = backdrop.color.a + (fadeSpeed * Time.deltaTime);
        if (newAlpha >= 0f)
        {
            backdrop.color = new Color(backdrop.color.r, backdrop.color.g, backdrop.color.b, 1f);
            return true;
        }
        else
        {
            backdrop.color = new Color(backdrop.color.r, backdrop.color.g, backdrop.color.b, newAlpha);
            return false;
        }
    }

    public bool FadeOutBackDrop()
    {
        float newAlpha = backdrop.color.a - (fadeSpeed * Time.deltaTime);
        if(newAlpha <= 0f)
        {
            backdrop.color = new Color(backdrop.color.r, backdrop.color.g, backdrop.color.b, 0f);
            return true;
        }
        else
        {
            backdrop.color = new Color(backdrop.color.r, backdrop.color.g, backdrop.color.b, newAlpha);
            return false;
        }
    }

    public void MarkCurrentSequenceComplete()
    {
        activeSequence.FinishedWatching = true;
    }

    public CutSceneSequence GetUnplayedSequenceFor(JelloPlate plate)
    {
        foreach(CutSceneSequence sequence in cutSceneSequences)
        {
            if (sequence.TriggerPlate == plate && !sequence.FinishedWatching)
                return sequence;
        }
        return null;
    }

    public void ClearSlides()
    {
        MainSlide.sprite = null;
        CrossFadeSlide.sprite = null;
    }
}

public class CutSceneState : State
{
    public virtual CutSceneState Update()
    {
        return (CutSceneState)base.Update();
    }
}

public class BlackState : CutSceneState
{
    private CutSceneState nextState;

    public BlackState()
    {
        nextState = this;
    }

    public override void Start()
    {
        CutSceneManager.Inst.SetBackDropOpacity(1f);
        SignalManager.Inst.AddListener<StateStartedSignal>(onStateStarted);
    }

    public override CutSceneState Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<StateStartedSignal>(onStateStarted);
    }

    private void onStateStarted(Signal signal)
    {
        StateStartedSignal stateStartedSignal = (StateStartedSignal)signal;
        if (stateStartedSignal.startedState.GetType() == typeof(IntroState))
        {
            CutSceneManager.Inst.SetActiveSequence(0);
            nextState = new FadeInState();
        }
    }
}

public class FadeInState : CutSceneState
{
    public override void Start()
    {
        CutSceneManager.Inst.PlaceActiveSlide();
    }

    public override CutSceneState Update()
    {
        if (CutSceneManager.Inst.FadeInActiveSlide())
            return new WaitForUserState();
        return this;
    }
}

public class WaitForUserState : CutSceneState
{
    private CutSceneState nextState;

    public WaitForUserState()
    {
        nextState = this;
    }

    public override void Start()
    {
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
    }

    public override CutSceneState Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.AddListener<MoveButtonPressedSignal>(onMoveButtonPressed);
    }

    private void onMoveButtonPressed(Signal signal)
    {
        if (CutSceneManager.Inst.ActiveSlideIsEndGame)
            SceneManager.LoadScene("MainScene");
        if (CutSceneManager.Inst.InputDelayExpired)
        {
            MoveButtonPressedSignal moveButtonPressedSignal = (MoveButtonPressedSignal)signal;
            if (moveButtonPressedSignal.moveButton == MoveButton.ATTACK || moveButtonPressedSignal.moveButton == MoveButton.SWITCH)
            {
                if (CutSceneManager.Inst.ActiveSlideIsLastInSequence)
                    nextState = new FinalFadeOutState();
                else if (CutSceneManager.Inst.GetNextFadeType() == FadeType.CROSS)
                    nextState = new CrossFadeState();
                else
                    nextState = new FadeThroughBlackState();
            }
        }
    }
}

public class CrossFadeState : CutSceneState
{
    public override void Start()
    {
        CutSceneManager.Inst.PlaceNextSlideOnCrossFade();
    }

    public override CutSceneState Update()
    {
        if (CutSceneManager.Inst.CrossFade())
            return new WaitForUserState();
        else
            return this;
    }

    public override void End()
    {
        CutSceneManager.Inst.IncrementActiveSlide();
        CutSceneManager.Inst.PromoteCrossFadeToMain();
    }
}

public class FadeThroughBlackState : CutSceneState
{
    private bool didFadeOut = false;
    private float remainingDelay;
    private bool didPlaceNextSlide = false;

    public FadeThroughBlackState()
    {
        remainingDelay = CutSceneManager.Inst.BlackDelay;
    }

    public override CutSceneState Update()
    {
        if (!didFadeOut)
            didFadeOut = CutSceneManager.Inst.FadeToBlack();
        else if (remainingDelay > 0f)
            remainingDelay -= Time.deltaTime;
        else if (!didPlaceNextSlide)
        {
            CutSceneManager.Inst.IncrementActiveSlide();
            CutSceneManager.Inst.PlaceActiveSlide();
            didPlaceNextSlide = true;
        }
        else if (CutSceneManager.Inst.FadeInActiveSlide())
            return new WaitForUserState();

        return this;
    }
}

public class FinalFadeOutState : CutSceneState
{
    public override CutSceneState Update()
    {
        if(CutSceneManager.Inst.FadeToBlack())
        {
            if (CutSceneManager.Inst.FadeOutBackDrop())
                return new WaitForTriggerState();
        }
        return this;
    }

    public override void End()
    {
        CutSceneManager.Inst.MarkCurrentSequenceComplete();
        CutSceneManager.Inst.ClearSlides();
        SignalManager.Inst.FireSignal(new CutSceneFinishedSignal());
    }
}

public class WaitForTriggerState : CutSceneState
{
    private CutSceneState nextState;

    public WaitForTriggerState()
    {
        nextState = this;
    }

    public override void Start()
    {
        SignalManager.Inst.AddListener<PlayerExitingJelloporterSignal>(onPlayerExitingJelloporter);
    }

    public override CutSceneState Update()
    {
        return nextState;
    }

    public override void End()
    {
        SignalManager.Inst.RemoveListener<PlayerExitingJelloporterSignal>(onPlayerExitingJelloporter);
    }

    private void onPlayerExitingJelloporter(Signal signal)
    {
        PlayerExitingJelloporterSignal playerExitingJelloporterSignal = (PlayerExitingJelloporterSignal)signal;
        JelloPlate plate = playerExitingJelloporterSignal.jelloporter.GetCurrentPlate();
        CutSceneSequence sequence = CutSceneManager.Inst.GetUnplayedSequenceFor(plate);
        if (sequence != null)
        {
            CutSceneManager.Inst.SetActiveSequence(sequence);
            nextState = new FirstFadeInState();
        }

    }
}

public class FirstFadeInState : CutSceneState
{
    private bool didPlaceFirstSlide = false;

    public override void Start()
    {
        SignalManager.Inst.FireSignal(new CutSceneStartingSignal());
    }

    public override CutSceneState Update()
    {
        if (CutSceneManager.Inst.FadeInBackDrop())
        {
            if (!didPlaceFirstSlide)
            { 
                CutSceneManager.Inst.PlaceActiveSlide();
                didPlaceFirstSlide = true;
            }
            if (CutSceneManager.Inst.FadeInActiveSlide())
                return new WaitForUserState();
        }
        return this;
    }
}

[Serializable]
public class CutSceneSequence
{
    public JelloPlate TriggerPlate;
    public List<Slide> Slides;

    [NonSerialized]
    public bool FinishedWatching;
}

[Serializable]
public class Slide
{
    public Sprite slide;
    public FadeType fadeType;
    public bool isEndGame = false;
}

public enum FadeType { CROSS, BLACK }