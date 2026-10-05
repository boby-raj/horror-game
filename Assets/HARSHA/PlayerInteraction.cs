using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 6f;
    public LayerMask interactableLayer;

    [SerializeField] private Camera playerCamera;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = GetComponent<Camera>();
        if (playerCamera == null) playerCamera = GetComponentInParent<Camera>();
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;

        if (interactableLayer.value == 0)
        {
            interactableLayer = ~0;
        }
    }

    void Update()
    {

        if (NoteManager.Instance != null && (NoteManager.Instance.IsReading || NoteManager.Instance.JustClosedThisFrame))
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnHoverExit();
                currentInteractable = null;
            }
            return;
        }

        CheckForInteractable();

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

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        IInteractable interactableObj = null;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactionDistance, interactableLayer))
        {
            interactableObj = hitInfo.collider.GetComponentInParent<IInteractable>();
            if (interactableObj == null)
            {
                interactableObj = hitInfo.collider.GetComponentInChildren<IInteractable>();
            }
        }

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