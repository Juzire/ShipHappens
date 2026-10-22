using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    [Header("Player Prefabs")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform soloSpawnPoint;
    [SerializeField] private Transform p1CoOpSpawnPoint;
    [SerializeField] private Transform p2CoOpSpawnPoint;

    private int activePlayerCount = 0;

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

    private void Start()
    {
        int modeInt = PlayerPrefs.GetInt("GameMode", 1);

        if (modeInt == 1)
        {
            SpawnSoloPlayer();
        }
        else
        {
            SpawnCoOpPlayers();
        }
    }

    private void SpawnSoloPlayer()
    {
        activePlayerCount = 1;
        Vector3 spawnPos = soloSpawnPoint != null ? soloSpawnPoint.position : Vector3.zero;
        GameObject p1 = Instantiate(player1Prefab, spawnPos, Quaternion.identity);

        PlayerController controller = p1.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.currentPlayMode = PlayerController.PlayMode.Solo;
            controller.localPlayerIndex = 0;
            controller.OnPlayerDeathCallback = HandlePlayerDeath;
        }

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.SetSoloTarget(p1.transform);
        }
    }

    private void SpawnCoOpPlayers()
    {
        activePlayerCount = 2;
        Vector3 p1Pos = p1CoOpSpawnPoint != null ? p1CoOpSpawnPoint.position : new Vector3(-2f, 0f, 0f);
        Vector3 p2Pos = p2CoOpSpawnPoint != null ? p2CoOpSpawnPoint.position : new Vector3(2f, 0f, 0f);

        // Spawn Player 1
        GameObject p1 = Instantiate(player1Prefab, p1Pos, Quaternion.identity);
        PlayerController c1 = p1.GetComponent<PlayerController>();
        if (c1 != null)
        {
            c1.currentPlayMode = PlayerController.PlayMode.LocalCoOp;
            c1.localPlayerIndex = 0;
            c1.OnPlayerDeathCallback = HandlePlayerDeath;
        }

        // Spawn Player 2
        GameObject p2 = Instantiate(player2Prefab, p2Pos, Quaternion.identity);
        PlayerController c2 = p2.GetComponent<PlayerController>();
        if (c2 != null)
        {
            c2.currentPlayMode = PlayerController.PlayMode.LocalCoOp;
            c2.localPlayerIndex = 1;
            c2.OnPlayerDeathCallback = HandlePlayerDeath;
        }

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.SetCoOpTargets(p1.transform, p2.transform);
        }
    }

    private void HandlePlayerDeath()
    {
        activePlayerCount--;
        if (activePlayerCount <= 0)
        {
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.TriggerLose();
            }
        }
    }
}