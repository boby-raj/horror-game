using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 6f;
    public LayerMask interactableLayer;

    // This remembers what object we are currently looking at
    private IInteractable currentInteractable;

    void Update()
    {
        // 1. Constantly check what we are looking at
        CheckForInteractable();

        // 2. If we press E, and we are looking at something, Interact with it!
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void CheckForInteractable()
{
    // Shoot a ray from the exact pixel center of the screen
    // (Screen.width / 2 is the exact middle horizontal pixel, Screen.height / 2 is vertical)
    Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
    
    if (Physics.Raycast(ray, out RaycastHit hitInfo, interactionDistance, interactableLayer))
    {
        IInteractable interactableObj = hitInfo.collider.GetComponent<IInteractable>();
        
        // If we are looking at a valid object...
        if (interactableObj != null)
        {
            // If it is a NEW object we just looked at...
            if (interactableObj != currentInteractable)
            {
                // Turn off the old object (if there was one)
                if (currentInteractable != null) currentInteractable.OnHoverExit();
                
                // Turn on the new object
                currentInteractable = interactableObj;
                currentInteractable.OnHoverEnter();
            }
        }
    }
    else
    {
        // If the raycast hits empty air, turn off whatever we were looking at
        if (currentInteractable != null)
        {
            currentInteractable.OnHoverExit();
            currentInteractable = null;
        }
    }
}
}