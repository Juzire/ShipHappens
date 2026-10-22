using DG.Tweening;
using UnityEngine;

public class HazardAnimation : MonoBehaviour
{
    [Header("Base Scale (Match your prefab's normal size)")]
    [SerializeField] private Vector3 baseScale = Vector3.one;

    [Header("Movement Visual Juice")]
    [SerializeField] private float pulseScaleMultiplier = 1.15f;
    [SerializeField] private float pulseDuration = 0.6f;
    [SerializeField] private float rotationSpeed = 45f;

    private void Start()
    {
        if (transform.localScale != Vector3.zero && baseScale == Vector3.one)
        {
            baseScale = transform.localScale;
        }

        DOVirtual.DelayedCall(0.35f, () =>
        {
            if (this != null && gameObject != null)
            {
                transform.DOScale(baseScale * pulseScaleMultiplier, pulseDuration)
                      .SetEase(Ease.InOutSine)
                      .SetLoops(-1, LoopType.Yoyo)
                      .SetLink(gameObject);
            }
        });
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}