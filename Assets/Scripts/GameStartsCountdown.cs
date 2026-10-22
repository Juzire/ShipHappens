using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameStartsCountdown : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Settings")]
    [SerializeField] private float countdownSeconds = 3f;

    [Header("Spawner Prefab")]
    [SerializeField] private GameObject waveSpawnerPrefab;

    private void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        float timer = countdownSeconds;

        while (timer > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = $"GET READY!\n{Mathf.Ceil(timer)}";
            }
            yield return new WaitForSecondsRealtime(1f);
            timer -= 1f;
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            yield return new WaitForSecondsRealtime(0.5f);
            countdownText.DOFade(0f, 0.5f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayBGM();
        }

        if (waveSpawnerPrefab != null)
        {
            Instantiate(waveSpawnerPrefab, Vector3.zero, Quaternion.identity);
        }

        Destroy(gameObject, 1f);
    }
}