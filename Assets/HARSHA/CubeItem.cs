using UnityEngine;

public class CubeItem : MonoBehaviour, IInteractable 
{
    private Renderer meshRenderer;
    private Color originalColor;

    void Start()
    {
        // Get the renderer so we can change the material color
        meshRenderer = GetComponent<Renderer>();
        originalColor = meshRenderer.material.color;
    }

    public void OnHoverEnter()
    {
        // Change color to Yellow to show we are in range and aiming at it
        meshRenderer.material.color = Color.yellow;
    }

    public void OnHoverExit()
    {
        // Change it back to normal when we look away or step out of range
        meshRenderer.material.color = originalColor;
    }

    public void Interact()
    {
        Debug.Log("Cube was interacted with!");
        Destroy(gameObject);
    }
}