using UnityEngine;

public class CubeItem : MonoBehaviour, IInteractable
{
    private Renderer meshRenderer;
    private Color originalColor;

    void Start()
    {

        meshRenderer = GetComponent<Renderer>();
        originalColor = meshRenderer.material.color;
    }

    public void OnHoverEnter()
    {

        meshRenderer.material.color = Color.yellow;
    }

    public void OnHoverExit()
    {

        meshRenderer.material.color = originalColor;
    }

    public void Interact()
    {
        Debug.Log("Cube was interacted with!");
        Destroy(gameObject);
    }
}