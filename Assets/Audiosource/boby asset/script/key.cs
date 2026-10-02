using UnityEngine;
using System.Collections;

public class key : MonoBehaviour, IInteractable
{
    [Header("Key Settings")]
    public box_open_withkey doorToUnlock; 
    public AudioClip pickupSound;
    
    [Header("UI Settings")]
    public GameObject keyAcquiredText; 
    public float textDisplayTime = 2.5f; 
    [Tooltip("Inventory icon shown when key is held")]
    public GameObject inventoryIcon;

    private bool isPickedUp = false;

    void Start()
    {
        if (keyAcquiredText != null) keyAcquiredText.SetActive(false);
        if (inventoryIcon != null) inventoryIcon.SetActive(false);
    }

    // Triggers the exact moment your raycast/crosshair looks at the object
    public void OnHoverEnter() 
    { 
        if (isPickedUp) return;
        
        Debug.Log("1. You looked at the key! Starting automatic pickup...");
        StartCoroutine(PickupSequence());
    }

    public void OnHoverExit() 
    { 
        // Required by IInteractable, but left empty because pickup is instant
    }
    
    public void Interact() 
    {
        // Left empty. We don't need a button press anymore.
    }

    private IEnumerator PickupSequence()
    {
        isPickedUp = true;
        
        if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        
        // CHECK IF THE DOOR IS LINKED PROPERLY
        if (doorToUnlock != null)
        {
            doorToUnlock.have_key = true;
            doorToUnlock.keyIconToDisable = inventoryIcon;
            Debug.Log("2. SUCCESS! The key successfully sent the unlock signal to the door/box.");
        }
        else
        {
            Debug.LogError("2. FAIL! The doorToUnlock slot is EMPTY in the inspector! The key doesn't know what to unlock.");
        }

        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        if (inventoryIcon != null) inventoryIcon.SetActive(true);

        if (keyAcquiredText != null) 
        {
            keyAcquiredText.SetActive(true);
            yield return new WaitForSeconds(textDisplayTime);
            keyAcquiredText.SetActive(false);
        }

        Destroy(gameObject);
    }
}