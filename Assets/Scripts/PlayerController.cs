using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    private Rigidbody2D rb;

    [Header("Health & Stats")]
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;
    [SerializeField] private Slider healthSlider;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 25f;
    [SerializeField] private float dashStaminaCost = 35f;
    private float currentStamina;
    [SerializeField] private Slider staminaSlider;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 4f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Shield Settings")]
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private float shieldDuration = 2f;
    [SerializeField] private float shieldScaleMultiplier = 1.5f;
    private bool isShieldActive = false;
    private Vector3 originalShieldScale = Vector3.one;

    [Header("Combat & Invulnerability")]
    [SerializeField] private float invulnerableDuration = 1f;
    private bool isInvulnerable = false;

    [Header("Screen Boundaries")]
    [SerializeField] private float boundX = 40f;
    [SerializeField] private float boundY = 40f;

    public enum PlayMode { Solo, LocalCoOp }
    [HideInInspector] public PlayMode currentPlayMode = PlayMode.Solo;
    [HideInInspector] public int localPlayerIndex = 0;

    private Vector2 moveInput;
    private bool isDashing = false;

    public System.Action<Vector2> OnMoveInputUpdated;
    public System.Action OnPlayerHit;
    public System.Action OnPlayerDeath;
    public System.Action OnPlayerDash;
    public System.Action OnPlayerDeathCallback;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        if (shieldVisual != null)
        {
            originalShieldScale = shieldVisual.transform.localScale;
            shieldVisual.SetActive(false);
        }
        UpdateUI();
    }

    private void Update()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }

        UpdateUI();

        float clampedX = Mathf.Clamp(transform.position.x, -boundX, boundX);
        float clampedY = Mathf.Clamp(transform.position.y, -boundY, boundY);
        transform.position = new Vector3(clampedX, clampedY, 0f);

        if (isDashing) return;

        switch (currentPlayMode)
        {
            case PlayMode.Solo:
                ReadSoloInput();
                break;
            case PlayMode.LocalCoOp:
                ReadLocalCoOpInput();
                break;
        }

        CheckAbilityInputs();
        OnMoveInputUpdated?.Invoke(moveInput);
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        if (rb != null)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    private void ReadSoloInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;
    }

    private void ReadLocalCoOpInput()
    {
        float x = 0f;
        float y = 0f;

        if (localPlayerIndex == 0)
        {
            if (Input.GetKey(KeyCode.D)) x += 1f;
            if (Input.GetKey(KeyCode.A)) x -= 1f;
            if (Input.GetKey(KeyCode.W)) y += 1f;
            if (Input.GetKey(KeyCode.S)) y -= 1f;
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.UpArrow)) y += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) y -= 1f;
        }

        moveInput = new Vector2(x, y).normalized;
    }

    private void CheckAbilityInputs()
    {
        if (localPlayerIndex == 0 || currentPlayMode == PlayMode.Solo)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) TriggerDash();
        }

        if (localPlayerIndex == 1 && currentPlayMode == PlayMode.LocalCoOp)
        {
            if (Input.GetKeyDown(KeyCode.RightShift)) TriggerDash();
        }
    }

    private void TriggerDash()
    {
        if (isDashing) return;

        if (currentStamina >= dashStaminaCost)
        {
            currentStamina -= dashStaminaCost;
            UpdateUI();

            isDashing = true;
            AudioManager.Instance?.PlayDashSFX();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            Vector2 dashDir = moveInput != Vector2.zero ? moveInput : (spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right);

            OnPlayerDash?.Invoke();

            Vector3 targetPosition = transform.position + (Vector3)(dashDir * dashDistance);

            transform.DOMove(targetPosition, dashDuration)
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    if (rb != null) rb.simulated = true;
                    isDashing = false;
                });
        }
    }

    public void ActivateShieldPickup()
    {
        if (isShieldActive) return;
        isShieldActive = true;

        AudioManager.Instance?.PlayOrbSFX();

        if (shieldVisual != null)
        {
            shieldVisual.transform.DOKill();
            shieldVisual.SetActive(true);

            shieldVisual.transform.localScale = Vector3.zero;
            shieldVisual.transform.localRotation = Quaternion.Euler(0f, 0f, -180f);

            Vector3 targetScale = originalShieldScale * shieldScaleMultiplier;
            Vector3 punchScale = targetScale * 1.15f;

            Sequence shieldJuiceSeq = DOTween.Sequence();
            shieldJuiceSeq.Append(shieldVisual.transform.DOScale(punchScale, 0.2f).SetEase(Ease.OutBack));
            shieldJuiceSeq.Join(shieldVisual.transform.DORotate(Vector3.zero, 0.25f, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack));
            shieldJuiceSeq.Append(shieldVisual.transform.DOScale(targetScale, 0.1f));
            shieldJuiceSeq.Append(shieldVisual.transform.DOScale(targetScale * 1.08f, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));
        }

        DOVirtual.DelayedCall(shieldDuration, () =>
        {
            if (shieldVisual != null)
            {
                shieldVisual.transform.DOKill();

                Sequence shieldExitSeq = DOTween.Sequence();
                shieldExitSeq.Append(shieldVisual.transform.DOScale(Vector3.zero, 0.18f).SetEase(Ease.InBack));
                shieldExitSeq.Join(shieldVisual.transform.DORotate(new Vector3(0f, 0f, 180f), 0.18f, RotateMode.LocalAxisAdd));
                shieldExitSeq.OnComplete(() =>
                {
                    shieldVisual.SetActive(false);
                });
            }
            isShieldActive = false;
        });
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<HazardMover>() != null || collision.gameObject.CompareTag("Hazard"))
        {
            if (isInvulnerable)
            {
                Destroy(collision.gameObject);
                return;
            }

            if (!isShieldActive)
            {
                TakeDamage(1);
                StartCoroutine(InvulnerabilityRoutine());
                Destroy(collision.gameObject);
            }
            else
            {
                if (shieldVisual != null)
                {
                    shieldVisual.transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 10, 1f);
                }
                Destroy(collision.gameObject);
            }
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;
        while (elapsed < invulnerableDuration)
        {
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = (c.a == 1f) ? 0.3f : 1f;
                spriteRenderer.color = c;
            }
            yield return new WaitForSeconds(0.12f);
            elapsed += 0.12f;
        }

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        isInvulnerable = false;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        UpdateUI();

        AudioManager.Instance?.PlayHitSFX();
        OnPlayerHit?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateUI();
    }

    public void RestoreStamina(float staminaAmount)
    {
        currentStamina = Mathf.Min(currentStamina + staminaAmount, maxStamina);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f;
            healthSlider.value = (float)currentHealth / maxHealth;
        }

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = 1f;
            staminaSlider.value = currentStamina / maxStamina;
        }
    }

    private void Die()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (shieldVisual != null)
        {
            shieldVisual.transform.DOKill();
            shieldVisual.SetActive(false);
        }

        OnPlayerDeath?.Invoke();
        OnPlayerDeathCallback?.Invoke();

        enabled = false;
    }
}