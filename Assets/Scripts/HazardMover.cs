using UnityEngine;

public class HazardMover : MonoBehaviour
{
    public enum HazardType { Asteroid, Meteor, Laser }

    [Header("Hazard Configuration")]
    [SerializeField] private HazardType hazardType;
    [SerializeField] private float moveSpeed = 6f;

    [Header("Rotation Settings (Optional)")]
    [SerializeField] private float asteroidSpinSpeed = 45f;

    private Vector2 moveDirection;

    public void InitializeVelocity(Vector2 direction, float speed = -1f)
    {
        if (speed > 0f)
        {
            moveSpeed = speed;
        }

        SetDirection(direction);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

        switch (hazardType)
        {
            case HazardType.Asteroid:
                transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                break;

            case HazardType.Meteor:
                transform.rotation = Quaternion.Euler(0f, 0f, angle + 180f);
                break;

            case HazardType.Laser:
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
                break;
        }
    }

    private void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        if (hazardType == HazardType.Asteroid)
        {
            transform.Rotate(0f, 0f, asteroidSpinSpeed * Time.deltaTime);
        }
    }
}