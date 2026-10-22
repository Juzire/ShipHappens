using DG.Tweening;
using UnityEngine;

public class CollectibleAnimation : MonoBehaviour
{
    [Header("Floating & Bobbing")]
    [SerializeField] private float floatAmount = 0.3f;  
    [SerializeField] private float floatDuration = 1.2f; 

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 60f;

    private void Start()
    {
        transform.DOMoveY(transform.position.y + floatAmount, floatDuration)
             .SetEase(Ease.InOutSine)
             .SetLoops(-1, LoopType.Yoyo)
             .SetLink(gameObject);
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}