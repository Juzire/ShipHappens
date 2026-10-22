using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private bool isHealthPickup = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckPickup(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckPickup(collision.gameObject);
    }

    private void CheckPickup(GameObject hitObject)
    {
        if (hitObject.CompareTag("Player"))
        {
            PlayerController player = hitObject.GetComponent<PlayerController>();
            if (player != null)
            {
                if (isHealthPickup)
                {
                    player.Heal(healAmount);
                }
                else
                {
                    player.ActivateShieldPickup();
                }
                AudioManager.Instance?.PlayPickupSFX();
            }

            Destroy(gameObject);
        }
    }
}