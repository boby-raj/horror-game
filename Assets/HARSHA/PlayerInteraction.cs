using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 6f;
    public LayerMask interactableLayer;

    [SerializeField] private Camera playerCamera;

    // This remembers what object we are currently looking at
    private IInteractable currentInteractable;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = GetComponent<Camera>();
        if (playerCamera == null) playerCamera = GetComponentInParent<Camera>();
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;

        // If no layer mask is assigned (0), default to everything so interaction doesn't silently fail
        if (interactableLayer.value == 0)
        {
            interactableLayer = ~0;
        }
    }

    void Update()
    {
        // If reading a note or if a note was just closed this frame, clear hover state and skip interaction
        if (NoteManager.Instance != null && (NoteManager.Instance.IsReading || NoteManager.Instance.JustClosedThisFrame))
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnHoverExit();
                currentInteractable = null;
            }
            return;
        }

        // 1. Constantly check what we are looking at
        CheckForInteractable();

        // 2. If we press E, and we are looking at something, Interact with it!
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            IInteractable target = currentInteractable;
            currentInteractable = null;
            target.Interact();
        }
    }

    void CheckForInteractable()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return;

        // Shoot a ray from the exact center of the screen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        IInteractable interactableObj = null;
        
        // 1. Direct raycast
        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactionDistance, interactableLayer))
        {
            interactableObj = hitInfo.collider.GetComponentInParent<IInteractable>();
            if (interactableObj == null)
            {
                interactableObj = hitInfo.collider.GetComponentInChildren<IInteractable>();
            }
        }

        // 2. Forgiving spherecast if direct ray didn't hit an interactable
        if (interactableObj == null)
        {
            if (Physics.SphereCast(ray, 0.25f, out RaycastHit sphereHit, interactionDistance, interactableLayer))
            {
                interactableObj = sphereHit.collider.GetComponentInParent<IInteractable>();
                if (interactableObj == null)
                {
                    interactableObj = sphereHit.collider.GetComponentInChildren<IInteractable>();
                }
            }
        }
        
        // Update hover state
        if (interactableObj != null)
        {
            if (interactableObj != currentInteractable)
            {
                if (currentInteractable != null) currentInteractable.OnHoverExit();
                currentInteractable = interactableObj;
                currentInteractable.OnHoverEnter();
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnHoverExit();
                currentInteractable = null;
            }
        }
    }
}