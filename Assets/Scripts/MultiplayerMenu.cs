using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string introSceneName = "IntroScene";

    [Header("DOTween UI Elements")]
    [SerializeField] private RectTransform titleLogo;
    [SerializeField] private RectTransform[] menuButtons;

    [Header("UI Displays (Optional)")]
    [SerializeField] private TMP_Text statusText;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    private AudioSource menuAudioSource;

    private Vector3 originalLogoScale;
    private Vector3[] originalButtonScales;

    private void Awake()
    {
        menuAudioSource = GetComponent<AudioSource>();
        if (menuAudioSource == null)
        {
            menuAudioSource = gameObject.AddComponent<AudioSource>();
        }
        menuAudioSource.playOnAwake = false;

        if (titleLogo != null)
        {
            originalLogoScale = titleLogo.localScale;
        }

        if (menuButtons != null && menuButtons.Length > 0)
        {
            originalButtonScales = new Vector3[menuButtons.Length];
            for (int i = 0; i < menuButtons.Length; i++)
            {
                if (menuButtons[i] != null)
                {
                    originalButtonScales[i] = menuButtons[i].localScale;
                }
            }
        }
    }

    private void Start()
    {
        AnimateMenuIn();
    }

    private void AnimateMenuIn()
    {
        if (titleLogo != null)
        {
            titleLogo.localScale = Vector3.zero;
            titleLogo.DOScale(originalLogoScale, 1f).SetEase(Ease.OutElastic).SetUpdate(true).OnComplete(() =>
            {
                titleLogo.DOScale(originalLogoScale * 1.05f, 0.8f)
                       .SetEase(Ease.InOutSine)
                       .SetLoops(-1, LoopType.Yoyo)
                       .SetUpdate(true);
            });
        }

        if (menuButtons != null && menuButtons.Length > 0)
        {
            float delay = 0.2f;

            for (int i = 0; i < menuButtons.Length; i++)
            {
                var btn = menuButtons[i];
                if (btn == null) continue;

                Vector3 targetScale = originalButtonScales[i];
                btn.localScale = Vector3.zero;

                btn.DOScale(targetScale, 0.6f)
                   .SetDelay(delay)
                   .SetEase(Ease.OutBack)
                   .SetUpdate(true)
                   .OnComplete(() =>
                   {
                       btn.DOScale(targetScale * 1.03f, 1f)
                          .SetEase(Ease.InOutSine)
                          .SetLoops(-1, LoopType.Yoyo)
                          .SetUpdate(true);
                   });

                delay += 0.15f;
            }
        }
    }

    private void PlayClickSound()
    {
        if (buttonClickClip != null && menuAudioSource != null)
        {
            menuAudioSource.PlayOneShot(buttonClickClip, sfxVolume);
        }
    }

    public void OnClickSinglePlayer()
    {
        PlayClickSound();
        if (statusText != null) statusText.text = "Starting Solo Game...";
        PlayerPrefs.SetInt("GameMode", 1);
        SceneManager.LoadScene(introSceneName);
    }

    public void OnClickLocalCoOp()
    {
        PlayClickSound();
        if (statusText != null) statusText.text = "Starting Local Co-Op...";
        PlayerPrefs.SetInt("GameMode", 2);
        SceneManager.LoadScene(introSceneName);
    }

    public void OnClickExitGame()
    {
        PlayClickSound();
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}