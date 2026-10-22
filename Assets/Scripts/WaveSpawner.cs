using System.Collections;
using DG.Tweening;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Hazard Prefabs")]
    [SerializeField] private GameObject standardDebrisPrefab;
    [SerializeField] private GameObject quickMeteorPrefab;
    [SerializeField] private GameObject laserPrefab;

    [Header("Pickup Prefabs")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private GameObject shieldPickupPrefab;
    [SerializeField] private float minPickupSpawnTime = 6f;
    [SerializeField] private float maxPickupSpawnTime = 12f;

    [Header("UI Reference")]
    [SerializeField] private GameUIManager uiManager;

    [Header("Wave Progression (1 to 8)")]
    [SerializeField] private int currentWave = 1;
    [SerializeField] private float waveDuration = 12f;

    [Header("Screen Bounds")]
    [SerializeField] private float boundX = 14f;
    [SerializeField] private float boundY = 6f;

    [Header("Spawn Safety Buffer")]
    [SerializeField] private float minSpawnDistanceFromPlayer = 4f;


    private void Start()
    {
        if (uiManager == null)
        {
            uiManager = GameUIManager.Instance;
        }

        StartCoroutine(MasterWaveProgressionRoutine());
        StartCoroutine(PickupSpawnRoutine());
    }

    private IEnumerator MasterWaveProgressionRoutine()
    {
        GameUIManager uiManagerInstance = FindAnyObjectByType<GameUIManager>();
        if (uiManagerInstance == null)
        {
            uiManagerInstance = GameUIManager.Instance;
        }

        while (currentWave <= 8)
        {
            if (uiManagerInstance != null)
            {
                uiManagerInstance.UpdateWaveUI(currentWave, 8);
                Debug.Log($"Successfully updated UI to Wave {currentWave}");
            }
            else
            {
                Debug.LogError("CRITICAL: Still cannot find GameUIManager in the scene!");
            }

            Debug.Log($"=== WAVE {currentWave} STARTED ===");

            float timer = 0f;
            while (timer < waveDuration)
            {
                ExecuteWavePattern(currentWave);

                float interval = GetSpawnRateForWave(currentWave);
                yield return new WaitForSeconds(interval);
                timer += interval;
            }

            currentWave++;
        }

        Debug.Log("All 8 waves complete! Rescue Ship extraction zone is now active.");

        if (uiManagerInstance != null)
        {
            uiManagerInstance.TriggerWin();
        }
    }

    private IEnumerator PickupSpawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        while (true)
        {
            float waitTime = Random.Range(minPickupSpawnTime, maxPickupSpawnTime);
            yield return new WaitForSeconds(waitTime);

            GameObject prefabToSpawn = (Random.value > 0.5f) ? healthPickupPrefab : shieldPickupPrefab;
            if (prefabToSpawn != null)
            {
                Vector3 randomPos = new Vector3(Random.Range(-6f, 6f), Random.Range(-4f, 4f), 0f);
                GameObject pickup = Instantiate(prefabToSpawn, randomPos, Quaternion.identity);

                // Uses the prefab's correct, natural size
                Vector3 targetScale = prefabToSpawn.transform.localScale;
                pickup.transform.localScale = Vector3.zero;
                pickup.transform.DOScale(targetScale, 0.3f).SetEase(Ease.OutBack).SetLink(pickup);

                Debug.Log($"Successfully spawned pickup: {prefabToSpawn.name}");
            }
        }
    }

    private float GetSpawnRateForWave(int wave)
    {
        switch (wave)
        {
            case 1: return 2.2f;
            case 2: return 1.8f;
            case 3: return 1.4f;
            case 4: return 1.0f;
            case 5: return 0.8f;
            case 6: return 0.7f;
            case 7: return 0.55f;
            case 8: return 0.45f; 
            default: return 0.5f;
        }
    }

    private void ExecuteWavePattern(int wave)
    {
        switch (wave)
        {
            case 1:
                SpawnSpreadingRing();
                break;
            case 2:
                SpawnTargetedOrQuickMeteor();
                break;
            case 3:
                SpawnLinearSweep();
                break;
            case 4:
                SpawnLinearSweep();
                SpawnQuickMeteorBarrage();
                break;
            case 5:
                SpawnSpreadingRing();
                SpawnLaserSweep();
                break;
            case 6:
                SpawnTargetedOrQuickMeteor();
                SpawnLaserSweep();
                break;
            case 7:
                SpawnLinearSweep();
                SpawnQuickMeteorBarrage();
                SpawnLaserSweep();
                break;
            case 8:
                SpawnSpreadingRing();
                SpawnLinearSweep();
                SpawnQuickMeteorBarrage();
                SpawnLaserSweep();
                break;
        }
    }

    private void SpawnSpreadingRing()
    {
        if (standardDebrisPrefab == null) return;

        Vector2 center = new Vector2(Random.Range(-4f, 4f), Random.Range(-3f, 3f));
        int count = 6;
        float radius = 1.5f;

        for (int i = 0; i < count; i++)
        {
            float angle = i * (Mathf.PI * 2f / count);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Vector3 spawnPos = (Vector3)center + offset;

            GameObject debris = Instantiate(standardDebrisPrefab, spawnPos, Quaternion.identity);
            PlaySpawnAnimation(debris);

            Vector2 driftDir = offset.normalized;
            Vector2 velocity = (driftDir * 4.5f) + (driftDir * 2f);

            if (debris.TryGetComponent<HazardMover>(out var mover))
            {
                mover.InitializeVelocity(velocity);
            }
        }
    }

    private void SpawnTargetedOrQuickMeteor()
    {
        GameObject prefabToUse = (Random.value > 0.4f) ? standardDebrisPrefab : quickMeteorPrefab;
        if (prefabToUse == null) prefabToUse = standardDebrisPrefab;
        if (prefabToUse == null) return;

        Vector3 spawnPos = GetSafeSpawnPosition();
        GameObject hazard = Instantiate(prefabToUse, spawnPos, Quaternion.identity);
        PlaySpawnAnimation(hazard);

        Vector2 targetDir = -spawnPos.normalized;
        GameObject player = GetClosestPlayer(spawnPos);
        if (player != null)
        {
            targetDir = (player.transform.position - spawnPos).normalized;
        }

        float speed = (prefabToUse == quickMeteorPrefab) ? 18f : 8f;
        Vector2 velocity = targetDir * speed;

        if (hazard.TryGetComponent<HazardMover>(out var mover))
        {
            mover.InitializeVelocity(velocity);
        }
    }

    private void SpawnLinearSweep()
    {
        if (standardDebrisPrefab == null) return;

        Vector3 spawnPos = GetSafeSpawnPosition();
        GameObject debris = Instantiate(standardDebrisPrefab, spawnPos, Quaternion.identity);
        PlaySpawnAnimation(debris);

        Vector2 dir = -spawnPos.normalized;
        Vector2 velocity = dir * 11f;

        if (debris.TryGetComponent<HazardMover>(out var mover))
        {
            mover.InitializeVelocity(velocity);
        }
    }

    private void SpawnQuickMeteorBarrage()
    {
        if (quickMeteorPrefab == null) return;

        Vector3 spawnPos = GetSafeSpawnPosition();
        GameObject meteor = Instantiate(quickMeteorPrefab, spawnPos, Quaternion.identity);
        PlaySpawnAnimation(meteor);

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector2 velocity = randomDir * 22f;

        if (meteor.TryGetComponent<HazardMover>(out var mover))
        {
            mover.InitializeVelocity(velocity);
        }
    }

    private void SpawnLaserSweep()
    {
        if (laserPrefab == null) return;

        Vector3 spawnPos = GetSafeSpawnPosition();
        GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);
        PlaySpawnAnimation(laser);

        Vector2 laserDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        Vector2 velocity = laserDir * 15f;

        if (laser.TryGetComponent<HazardMover>(out var mover))
        {
            mover.InitializeVelocity(velocity);
        }
    }

    private void PlaySpawnAnimation(GameObject hazard)
    {
        if (hazard == null) return;

        Vector3 originalScale = hazard.transform.localScale;
        hazard.transform.localScale = Vector3.zero;

        hazard.transform.DOScale(originalScale, 0.35f)
           .SetEase(Ease.OutBack)
           .SetLink(hazard);
    }

    private Vector3 GetSafeSpawnPosition()
    {
        Vector3 spawnPos = GetRandomEdgePosition();
        int safetyCounter = 0;

        while (IsTooCloseToAnyPlayer(spawnPos) && safetyCounter < 10)
        {
            spawnPos = GetRandomEdgePosition();
            safetyCounter++;
        }

        return spawnPos;
    }

    private bool IsTooCloseToAnyPlayer(Vector3 pos)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p != null && Vector3.Distance(pos, p.transform.position) < minSpawnDistanceFromPlayer)
            {
                return true;
            }
        }
        return false;
    }

    private GameObject GetClosestPlayer(Vector3 pos)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject p in players)
        {
            if (p != null)
            {
                float dist = Vector3.Distance(pos, p.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = p;
                }
            }
        }
        return closest;
    }

    private Vector3 GetRandomEdgePosition()
    {
        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: return new Vector3(boundX, Random.Range(-boundY, boundY), 0f);
            case 1: return new Vector3(-boundX, Random.Range(-boundY, boundY), 0f);
            case 2: return new Vector3(Random.Range(-boundX, boundX), boundY, 0f);
            default: return new Vector3(Random.Range(-boundX, boundX), -boundY, 0f);
        }
    }
}