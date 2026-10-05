using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI buttonText;
    private Vector3 originalScale;
    private Color originalColor;

    [Header("Hover Settings")]
    public Color hoverColor = new Color(0.6f, 0f, 0f, 1f);
    public float scaleMultiplier = 1.1f;

    void Start()
    {

        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        originalScale = transform.localScale;

        if (buttonText != null)
        {
            originalColor = buttonText.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * scaleMultiplier;

        if (buttonText != null)
        {
            buttonText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;

        if (buttonText != null)
        {
            buttonText.color = originalColor;
        }
    }
}