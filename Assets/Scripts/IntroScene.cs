using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Characters (Singleplayer & Co-op)")]
    [SerializeField] private GameObject charac1;
    [SerializeField] private GameObject charac2; 

    [Header("Visual Effects")]
    [SerializeField] private GameObject fullScreenExplosion;     
    [SerializeField] private Image screenFlashOverlay;            

    [Header("UI Story Elements")]
    [SerializeField] private TMP_Text storyTextDisplay;    
    [SerializeField] private TMP_Text promptTextDisplay;   

    [Header("Audio")]
    [SerializeField] private AudioSource musicAudioSource;     
    [SerializeField] private AudioSource sfxAudioSource;     
    [SerializeField] private AudioClip cutsceneBGM;         
    [SerializeField] private AudioClip textPopSFX;           
    [SerializeField] private AudioClip explosionSFX;       

    [Header("Story Lines")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] storyLines = new string[] {
        "Everything was fine... until *Ship Happens!*",
        "A massive breach hit our SHIP!",
        "Now we're stranded in deep space. Time to survive!",
        "AVOID OBSTACLES" 
    };

    private int currentLineIndex = 0;
    private bool isTransitioning = false;
    private bool cutsceneFinished = false;
    private Tween promptFadeTween;

    private void Start()
    {
        if (musicAudioSource != null && cutsceneBGM != null)
        {
            musicAudioSource.clip = cutsceneBGM;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }

        int gameMode = PlayerPrefs.GetInt("GameMode", 1);

        if (gameMode == 1)
        {
            if (charac2 != null) charac2.SetActive(false);
        }
        else
        {
            if (charac1 != null) charac1.SetActive(true);
            if (charac2 != null) charac2.SetActive(true);
        }

        if (storyTextDisplay != null)
        {
            Color c = storyTextDisplay.color;
            c.a = 0f;
            storyTextDisplay.color = c;
        }

        if (promptTextDisplay != null)
        {
            Color c = promptTextDisplay.color;
            c.a = 0f;
            promptTextDisplay.color = c;
        }

        if (fullScreenExplosion != null) fullScreenExplosion.SetActive(false);

        if (screenFlashOverlay != null)
        {
            screenFlashOverlay.color = new Color(1f, 0f, 0f, 0f);
        }

        ShowNextStoryLine();
    }

    private void Update()
    {
        if (!isTransitioning && !cutsceneFinished && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            OnContinueTriggered();
        }
    }

    private void ShowNextStoryLine()
    {
        if (currentLineIndex < storyLines.Length)
        {
            storyTextDisplay.text = storyLines[currentLineIndex];

            if (sfxAudioSource != null && textPopSFX != null)
            {
                sfxAudioSource.PlayOneShot(textPopSFX);
            }

            storyTextDisplay.DOFade(1f, 0.5f).OnComplete(() =>
            {
                isTransitioning = false;

                if (promptTextDisplay != null)
                {
                    promptFadeTween?.Kill();
                    promptFadeTween = promptTextDisplay.DOFade(0.2f, 0.8f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                }
            });

            storyTextDisplay.transform.localScale = Vector3.one * 0.95f;
            storyTextDisplay.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            currentLineIndex++;
        }
        else
        {
            TriggerUIExplosionAndTransition();
        }
    }

    private void OnContinueTriggered()
    {
        isTransitioning = true;

        promptFadeTween?.Kill();
        if (promptTextDisplay != null) promptTextDisplay.DOFade(0f, 0.2f);

        storyTextDisplay.DOFade(0f, 0.3f).OnComplete(() =>
        {
            ShowNextStoryLine();
        });
    }

    private void TriggerUIExplosionAndTransition()
    {
        cutsceneFinished = true;

        storyTextDisplay.DOKill();
        promptFadeTween?.Kill();
        if (promptTextDisplay != null) promptTextDisplay.gameObject.SetActive(false);

        Sequence textExplosionSeq = DOTween.Sequence();
        textExplosionSeq.Append(storyTextDisplay.transform.DOShakePosition(0.4f, 30f, 35, 90, false));
        textExplosionSeq.Join(storyTextDisplay.transform.DOScale(Vector3.one * 1.8f, 0.4f).SetEase(Ease.InBack));
        textExplosionSeq.Join(storyTextDisplay.transform.DORotate(new Vector3(0, 0, Random.Range(-60f, 60f)), 0.4f));
        textExplosionSeq.Join(storyTextDisplay.DOFade(0f, 0.35f));

        if (sfxAudioSource != null && explosionSFX != null)
        {
            sfxAudioSource.PlayOneShot(explosionSFX);
        }

        if (fullScreenExplosion != null) fullScreenExplosion.SetActive(true);

        if (screenFlashOverlay != null)
        {
            screenFlashOverlay.color = new Color(1f, 0f, 0f, 0.8f);
            screenFlashOverlay.DOFade(0f, 0.4f);
        }

        if (Camera.main != null)
        {
            Camera.main.transform.DOShakePosition(0.5f, 0.7f, 30, 90, false);
        }

        DOVirtual.DelayedCall(0.2f, () =>
        {
            SceneManager.LoadScene(gameSceneName);
        });
    }
}