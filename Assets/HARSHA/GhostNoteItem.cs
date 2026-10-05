using UnityEngine;

public class GhostNoteItem : MonoBehaviour, IInteractable
{
    [Header("UI Connection")]
    public GhostHUD hudToTrigger;

    private Renderer meshRenderer;
    private Color originalColor;

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null) originalColor = meshRenderer.material.color;
    }

    public void OnHoverEnter()
    {

        if (meshRenderer != null) meshRenderer.material.color = Color.yellow;
    }

    public void OnHoverExit()
    {
        if (meshRenderer != null) meshRenderer.material.color = originalColor;
    }

    public void Interact()
    {

        if (hudToTrigger != null)
        {
            hudToTrigger.TriggerOpen();

            if (meshRenderer != null) meshRenderer.material.color = originalColor;
        }
        else
        {
            Debug.LogError("You forgot to drag the GhostHUD into the GhostNoteItem script!");
        }
    }
}