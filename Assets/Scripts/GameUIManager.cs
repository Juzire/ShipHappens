using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Wave UI")]
    [SerializeField] private TMP_Text waveCountText;

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Pause Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    private bool isPaused = false;
    private bool isGameOverTriggered = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if ((winPanel == null || !winPanel.activeSelf) && (losePanel == null || !losePanel.activeSelf))
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            AudioManager.Instance?.PauseBGM(); 
        }
        else
        {
            AudioManager.Instance?.ResumeBGM(); 
        }

        if (pausePanel != null)
        {
            if (isPaused)
            {
                pausePanel.SetActive(true);
                AnimatePanelIn(pausePanel);
            }
            else
            {
                AnimatePanelOut(pausePanel, () => pausePanel.SetActive(false));
            }
        }
    }

    public void UpdateWaveUI(int currentWave, int maxWaves)
    {
        if (waveCountText != null)
        {
            waveCountText.text = "Wave " + currentWave;
        }
    }

    public void TriggerWin()
    {
        if (isGameOverTriggered) return;
        isGameOverTriggered = true;

        AudioManager.Instance?.StopBGM();    
        AudioManager.Instance?.PlayWinSFX(); 

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            AnimatePanelIn(winPanel);
        }
        Time.timeScale = 0f;
    }

    public void TriggerLose()
    {
        if (isGameOverTriggered) return;
        isGameOverTriggered = true;

        AudioManager.Instance?.StopBGM();  
        AudioManager.Instance?.PlayLoseSFX(); 

        if (losePanel != null)
        {
            losePanel.SetActive(true);
            AnimatePanelIn(losePanel);
        }
        Time.timeScale = 0f;
    }

    private void AnimatePanelIn(GameObject panel)
    {
        panel.transform.localScale = Vector3.zero;

        if (panel.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.3f).SetUpdate(true).SetLink(panel);
        }

        panel.transform.DOScale(Vector3.one, 0.35f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .SetLink(panel);

        Button[] panelButtons = panel.GetComponentsInChildren<Button>(true);
        foreach (var btn in panelButtons)
        {
            btn.transform.localScale = Vector3.one;
            btn.transform.DOScale(Vector3.one * 1.06f, 0.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true)
                .SetLink(btn.gameObject);
        }

        Image[] panelImages = panel.GetComponentsInChildren<Image>(true);
        foreach (var img in panelImages)
        {
            if (img.gameObject == panel) continue;

            img.transform.localScale = Vector3.one;
            img.transform.DOScale(Vector3.one * 1.05f, 0.7f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true)
                .SetLink(img.gameObject);
        }
    }

    private void AnimatePanelOut(GameObject panel, TweenCallback onCompleteCallback)
    {
        if (panel.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup.DOFade(0f, 0.2f).SetUpdate(true).SetLink(panel);
        }

        panel.transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .SetLink(panel)
            .OnComplete(onCompleteCallback);
    }

    public void OnContinueClicked()
    {
        AudioManager.Instance?.PlayButtonSFX();
        TogglePause();
    }

    public void OnPlayAgainClicked()
    {
        AudioManager.Instance?.PlayButtonSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuClicked(string mainMenuSceneName = "TitleScreen")
    {
        AudioManager.Instance?.PlayButtonSFX();
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            mainMenuSceneName = "TitleScreen";
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnExitClicked()
    {
        AudioManager.Instance?.PlayButtonSFX();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}