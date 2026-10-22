using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Tracking Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Dynamic Zoom Settings (Co-Op)")]
    [SerializeField] private float minZoom = 5f;          // Closest zoom when players are near
    [SerializeField] private float maxZoom = 12f;         // Furthest zoom when players are far apart
    [SerializeField] private float defaultSoloZoom = 6f;  // Standard camera size for Solo mode

    private Camera cam;
    private Transform currentTarget;
    private Transform target1;
    private Transform target2;
    private bool isCoOp = false;

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

        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (isCoOp)
        {
            HandleCoOpTrackingAndZoom();
        }
        else
        {
            HandleSoloTrackingAndZoom();
        }
    }

    private void HandleSoloTrackingAndZoom()
    {
        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            // Position tracking
            Vector3 targetPosition = currentTarget.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            // Smoothly maintain default zoom
            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, defaultSoloZoom, smoothSpeed * Time.deltaTime);
            }
        }
        else
        {
            FindActivePlayer();
        }
    }

    private void HandleCoOpTrackingAndZoom()
    {
        // If both players are alive, track midpoint and dynamically zoom based on distance
        if (target1 != null && target1.gameObject.activeInHierarchy && target2 != null && target2.gameObject.activeInHierarchy)
        {
            // Midpoint calculation
            Vector3 midpoint = (target1.position + target2.position) / 2f;
            Vector3 targetPosition = midpoint + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            // Dynamic Zoom based on distance between the two players
            if (cam != null)
            {
                float distance = Vector3.Distance(target1.position, target2.position);
                float targetZoom = Mathf.Clamp(distance / 2f + minZoom, minZoom, maxZoom);
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, smoothSpeed * Time.deltaTime);
            }
        }
        else if (target1 != null && target1.gameObject.activeInHierarchy)
        {
            // Player 1 is alive, Player 2 is dead -> Zoom back in and track Player 1
            Vector3 targetPosition = target1.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, defaultSoloZoom, smoothSpeed * Time.deltaTime);
            }
        }
        else if (target2 != null && target2.gameObject.activeInHierarchy)
        {
            // Player 2 is alive, Player 1 is dead -> Zoom back in and track Player 2
            Vector3 targetPosition = target2.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, defaultSoloZoom, smoothSpeed * Time.deltaTime);
            }
        }
        else
        {
            FindActivePlayer();
        }
    }

    public void SetSoloTarget(Transform player)
    {
        isCoOp = false;
        currentTarget = player;
    }

    public void SetCoOpTargets(Transform player1, Transform player2)
    {
        isCoOp = true;
        target1 = player1;
        target2 = player2;
    }

    private void FindActivePlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            currentTarget = players[0].transform;
            isCoOp = false;
        }
        else
        {
            currentTarget = null;
        }
    }
}