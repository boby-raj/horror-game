using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("How far the player can reach to press E")]
    public float interactionDistance = 5f;
    [Tooltip("Layers the raycast can interact with (automatically ensures Layer 8 is included)")]
    public LayerMask interactableLayer = ~0;

    private IInteractable currentInteractable;

    void Awake()
    {
        // Ensure distance is comfortable for first-person interaction
        if (interactionDistance < 4.5f) interactionDistance = 4.5f;

        // If layer mask was unconfigured or 0, default to hitting all layers
        if (interactableLayer.value == 0)
        {
            interactableLayer = ~0;
        }
        else
        {
            // Ensure Layer 8 (Interactable) is always included
            interactableLayer |= (1 << 8);
        }
    }

    void Update()
    {
        CheckForInteractable();

        // When pressing 'E' while looking at an interactable object
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void CheckForInteractable()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        // Shoot ray from exact center of screen crosshair
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactionDistance, interactableLayer, QueryTriggerInteraction.Collide))
        {
            // Check the hit collider, its parents, or its children for IInteractable
            IInteractable interactableObj = hitInfo.collider.GetComponentInParent<IInteractable>();
            if (interactableObj == null)
            {
                interactableObj = hitInfo.collider.GetComponentInChildren<IInteractable>();
            }

            if (interactableObj != null)
            {
                if (interactableObj != currentInteractable)
                {
                    if (currentInteractable != null) currentInteractable.OnHoverExit();
                    currentInteractable = interactableObj;
                    currentInteractable.OnHoverEnter();
                }
                return;
            }
        }

        // Looking at nothing interactable
        if (currentInteractable != null)
        {
            currentInteractable.OnHoverExit();
            currentInteractable = null;
        }
    }
}