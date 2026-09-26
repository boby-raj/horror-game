using UnityEngine;
using TMPro; // Required for TextMeshPro

[RequireComponent(typeof(Renderer))]
public class BodyPart : MonoBehaviour, IInteractable
{
    [Tooltip("0 = Head, 1 = Left Arm, 2 = Right Arm, etc.")]
    public int partID;

    [Header("Glow Settings")]
    [Tooltip("The color the object will glow when looked at.")]
    [ColorUsage(true, true)]
    public Color glowColor = Color.yellow * 2f;

    [Header("UI Settings")]
    [Tooltip("Drag your Canvas TextMeshPro element here.")]
    public TMP_Text promptTextUI;
    [Tooltip("The message to show when looking at this object.")]
    public string hoverMessage = "Press [E] to collect part";

    private WallPuzzleManager puzzleManager;
    private Renderer meshRenderer;
    private Material partMaterial;
    
    private Color originalEmissionColor;
    private bool originallyHadEmission;

    void Start()
    {
        puzzleManager = Object.FindFirstObjectByType<WallPuzzleManager>();
        meshRenderer = GetComponent<Renderer>();

        // Material setup for the glow
        if (meshRenderer != null)
        {
            partMaterial = meshRenderer.material;
            originallyHadEmission = partMaterial.IsKeywordEnabled("_EMISSION");
            if (partMaterial.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = partMaterial.GetColor("_EmissionColor");
            }
        }

        // Ensure the text prompt is hidden when the game starts
        if (promptTextUI != null)
        {
            promptTextUI.gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        if (puzzleManager != null)
        {
            // Hide the text right before we collect/destroy the object
            // so the text doesn't get permanently stuck on the screen
            if (promptTextUI != null)
            {
                promptTextUI.gameObject.SetActive(false);
            }

            puzzleManager.CollectPart(this);
        }
        else
        {
            Debug.LogError("Could not find WallPuzzleManager in the scene!");
        }
    }

    public void OnHoverEnter()
    {
        // 1. Turn on the glow
        if (partMaterial != null)
        {
            partMaterial.EnableKeyword("_EMISSION");
            partMaterial.SetColor("_EmissionColor", glowColor);
        }

        // 2. Show the text prompt
        if (promptTextUI != null)
        {
            promptTextUI.text = hoverMessage;
            promptTextUI.gameObject.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        // 1. Revert the glow
        if (partMaterial != null)
        {
            if (!originallyHadEmission)
            {
                partMaterial.DisableKeyword("_EMISSION");
            }
            
            if (partMaterial.HasProperty("_EmissionColor"))
            {
                partMaterial.SetColor("_EmissionColor", originalEmissionColor);
            }
        }

        // 2. Hide the text prompt
        if (promptTextUI != null)
        {
            promptTextUI.gameObject.SetActive(false);
        }
    }

    // Failsafe: If the object is destroyed or disabled while you are still looking at it, 
    // this ensures the text doesn't get stuck on your screen forever.
    private void OnDisable()
    {
        if (promptTextUI != null && promptTextUI.gameObject.activeSelf)
        {
            promptTextUI.gameObject.SetActive(false);
        }
    }
}