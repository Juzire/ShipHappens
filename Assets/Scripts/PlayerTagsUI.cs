using TMPro;
using UnityEngine;

public class PlayerTagsUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI controlsTagText;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, 0f); 

    [Header("Custom Control Strings (Type Manually)")]
    [TextArea]
    [SerializeField] private string player1Text = "WASD = Move | L-Shift = Dash";
    [TextArea]
    [SerializeField] private string player2Text = "Arrows = Move | R-Shift = Dash";
    [TextArea]
    [SerializeField] private string soloText = "WASD = Move | Shift = Dash";

    [Header("Display Duration")]
    [SerializeField] private float displayDuration = 5f;

    private PlayerController playerController;
    private float visibleTimer = 0f;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        UpdateControlText();
        visibleTimer = displayDuration; 
    }

    private void Update()
    {
        if (controlsTagText == null) return;

        controlsTagText.transform.position = transform.position + offset;

        if (visibleTimer > 0f)
        {
            visibleTimer -= Time.deltaTime;
            if (visibleTimer <= 0f && controlsTagText.gameObject.activeSelf)
            {
                controlsTagText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateControlText()
    {
        if (controlsTagText == null) return;

        if (playerController != null && playerController.currentPlayMode == PlayerController.PlayMode.LocalCoOp)
        {
            if (playerController.localPlayerIndex == 0)
            {
                controlsTagText.text = player1Text;
                controlsTagText.color = Color.blue; 
            }
            else
            {
                controlsTagText.text = player2Text;
                controlsTagText.color = Color.red; 
            }
        }
        else
        {
            controlsTagText.text = soloText;
            controlsTagText.color = Color.blue;
        }
    }
}