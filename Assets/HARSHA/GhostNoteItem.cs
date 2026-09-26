using UnityEngine;

public class GhostNoteItem : MonoBehaviour, IInteractable
{
    [Header("UI Connection")]
    public GhostHUD hudToTrigger; // Drag your UI Canvas with the GhostHUD script into here!

    private Renderer meshRenderer;
    private Color originalColor;

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null) originalColor = meshRenderer.material.color;
    }

    public void OnHoverEnter()
    {
        // Optional: Make the paper glow so the player knows they can read it
        if (meshRenderer != null) meshRenderer.material.color = Color.yellow;
    }

    public void OnHoverExit()
    {
        if (meshRenderer != null) meshRenderer.material.color = originalColor;
    }

    public void Interact()
    {
        // When the player looks at the note and presses 'E', trigger the spooky UI popup!
        if (hudToTrigger != null)
        {
            hudToTrigger.TriggerOpen();
            
            // Turn off the yellow highlight while reading
            if (meshRenderer != null) meshRenderer.material.color = originalColor; 
        }
        else
        {
            Debug.LogError("You forgot to drag the GhostHUD into the GhostNoteItem script!");
        }
    }
}