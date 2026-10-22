using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music Clips")]
    [SerializeField] private AudioClip gameplayBGM;

    [Header("Sound Effect Clips")]
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip pickupSFX;
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip orbShieldSFX;
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private AudioClip winSFX;
    [SerializeField] private AudioClip loseSFX;

    [Header("Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float defaultBgmVolume = 0.75f;
    [Range(0f, 1f)][SerializeField] private float defaultSfxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (sfxSource != null)
        {
            sfxSource.volume = defaultSfxVolume;
        }
    }

    public void PlayGameplayBGM()
    {
        if (bgmSource != null && gameplayBGM != null)
        {
            bgmSource.volume = defaultBgmVolume;
            bgmSource.clip = gameplayBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.UnPause();
        }
    }

    #region SFX Triggers

    public void PlaySFX(AudioClip clip, float pitchVariation = 0.05f)
    {
        if (sfxSource == null || clip == null) return;

        sfxSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonSFX() => PlaySFX(buttonClickSFX, 0.02f);
    public void PlayPickupSFX() => PlaySFX(pickupSFX, 0.08f);
    public void PlayDashSFX() => PlaySFX(dashSFX, 0.05f);
    public void PlayOrbSFX() => PlaySFX(orbShieldSFX, 0.05f);
    public void PlayHitSFX() => PlaySFX(hitSFX, 0.1f);

    public void PlayWinSFX()
    {
        if (sfxSource != null && winSFX != null) sfxSource.PlayOneShot(winSFX);
    }

    public void PlayLoseSFX()
    {
        if (sfxSource != null && loseSFX != null) sfxSource.PlayOneShot(loseSFX);
    }

    #endregion
}