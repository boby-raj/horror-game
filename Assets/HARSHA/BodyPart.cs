using UnityEngine;
using TMPro;

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

        if (meshRenderer != null)
        {
            partMaterial = meshRenderer.material;
            originallyHadEmission = partMaterial.IsKeywordEnabled("_EMISSION");
            if (partMaterial.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = partMaterial.GetColor("_EmissionColor");
            }
        }

        if (promptTextUI != null)
        {
            promptTextUI.gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        if (puzzleManager != null)
        {

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

        if (partMaterial != null)
        {
            partMaterial.EnableKeyword("_EMISSION");
            partMaterial.SetColor("_EmissionColor", glowColor);
        }

        if (promptTextUI != null)
        {
            promptTextUI.text = hoverMessage;
            promptTextUI.gameObject.SetActive(true);
        }
    }

    public void OnHoverExit()
    {

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

        if (promptTextUI != null)
        {
            promptTextUI.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (promptTextUI != null && promptTextUI.gameObject.activeSelf)
        {
            promptTextUI.gameObject.SetActive(false);
        }
    }
}