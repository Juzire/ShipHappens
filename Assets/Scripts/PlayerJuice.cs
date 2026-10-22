using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerJuice : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private PlayerController playerController;
    private Color originalColor = Color.white;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void OnEnable()
    {
        if (playerController != null)
        {
            playerController.OnMoveInputUpdated += HandleSpriteFlipping;
            playerController.OnPlayerHit += PlayHitAnimation;
            playerController.OnPlayerDeath += PlayDeathAnimation;
            playerController.OnPlayerDash += PlayDashJuice;
        }
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.OnMoveInputUpdated -= HandleSpriteFlipping;
            playerController.OnPlayerHit -= PlayHitAnimation;
            playerController.OnPlayerDeath -= PlayDeathAnimation;
            playerController.OnPlayerDash -= PlayDashJuice;
        }
    }

    private void HandleSpriteFlipping(Vector2 moveInput)
    {
        if (spriteRenderer == null) return;

        if (moveInput.x > 0.01f)
        {
            spriteRenderer.flipX = false; 
        }
        else if (moveInput.x < -0.01f)
        {
            spriteRenderer.flipX = true;  
        }
    }

    private void PlayDashJuice()
    {
        transform.DOComplete();

        Sequence dashJuiceSeq = DOTween.Sequence();
        dashJuiceSeq.Append(transform.DOScale(new Vector3(1.4f, 0.6f, 1f), 0.04f));
        dashJuiceSeq.Append(transform.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutBack));

        if (spriteRenderer != null)
        {
            dashJuiceSeq.Join(spriteRenderer.DOFade(0.2f, 0.04f).SetLoops(4, LoopType.Yoyo));
            dashJuiceSeq.OnComplete(() => spriteRenderer.color = originalColor);
        }
    }

    private void PlayHitAnimation()
    {
        transform.DOComplete();

        transform.DOPunchScale(new Vector3(0.6f, 0.6f, 0f), 0.3f, 12, 1.2f);

        if (spriteRenderer != null)
        {
            spriteRenderer.DOColor(Color.white, 0.05f).OnComplete(() =>
            {
                spriteRenderer.DOColor(originalColor, 0.2f);
            });
        }
    }

    private void PlayDeathAnimation()
    {
        transform.DOComplete();

        Sequence deathSeq = DOTween.Sequence();
        deathSeq.Append(transform.DOScale(transform.localScale * 1.3f, 0.15f).SetEase(Ease.OutBack));
        deathSeq.Append(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
        deathSeq.Join(transform.DORotate(new Vector3(0f, 0f, 720f), 0.5f, RotateMode.LocalAxisAdd));

        if (spriteRenderer != null)
        {
            deathSeq.Join(spriteRenderer.DOFade(0f, 0.5f));
        }

        deathSeq.OnComplete(() =>
        {
            GameObject[] remainingPlayers = GameObject.FindGameObjectsWithTag("Player");
            if (remainingPlayers.Length <= 1)
            {
                GameUIManager uiManager = FindAnyObjectByType<GameUIManager>();
                if (uiManager != null)
                {
                    uiManager.TriggerLose();
                }
            }

            Destroy(gameObject);
        });
    }
}