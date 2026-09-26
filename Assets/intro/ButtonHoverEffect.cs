using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Required for TextMeshPro

// The interfaces IPointerEnterHandler and IPointerExitHandler tell Unity to listen for the mouse
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI buttonText;
    private Vector3 originalScale;
    private Color originalColor;

    [Header("Hover Settings")]
    public Color hoverColor = new Color(0.6f, 0f, 0f, 1f); // Default is dark red
    public float scaleMultiplier = 1.1f; // Makes the text 10% larger on hover

    void Start()
    {
        // Find the TextMeshPro component attached to this button
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        
        // Save the starting size and color so we can revert back to them
        originalScale = transform.localScale;
        
        if (buttonText != null)
        {
            originalColor = buttonText.color;
        }
    }

    // This triggers the exact frame the mouse touches the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * scaleMultiplier;
        
        if (buttonText != null)
        {
            buttonText.color = hoverColor;
        }
    }

    // This triggers the exact frame the mouse leaves the button
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        
        if (buttonText != null)
        {
            buttonText.color = originalColor;
        }
    }
}