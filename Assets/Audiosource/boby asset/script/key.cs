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
    [Tooltip("Shows 'Press E to Pick Up' when player looks at the key")]
    public GameObject hoverText;
    [Tooltip("Inventory icon shown when key is held")]
    public GameObject inventoryIcon;

    private bool isPickedUp = false;

    void Start()
    {
        if (keyAcquiredText != null) keyAcquiredText.SetActive(false);
        if (hoverText != null) hoverText.SetActive(false);
        if (inventoryIcon != null) inventoryIcon.SetActive(false);
    }

    // Show "Press E" prompt when player looks at the key
    public void OnHoverEnter() 
    { 
        if (isPickedUp) return;
        if (hoverText != null) hoverText.SetActive(true);
    }

    // Hide "Press E" prompt when player looks away
    public void OnHoverExit() 
    { 
        if (hoverText != null) hoverText.SetActive(false);
    }

    // Triggered when player presses E while looking at the key
    public void Interact() 
    {
        if (isPickedUp) return;
        if (hoverText != null) hoverText.SetActive(false);
        StartCoroutine(PickupSequence());
    }

    private IEnumerator PickupSequence()
    {
        isPickedUp = true;
        
        if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        
        // Tell the door/box that the player now has the key
        if (doorToUnlock != null)
        {
            doorToUnlock.have_key = true;
            doorToUnlock.keyIconToDisable = inventoryIcon;
            Debug.Log("SUCCESS! Key picked up and unlock signal sent to door/box.");
        }
        else
        {
            Debug.LogError("FAIL! The doorToUnlock slot is EMPTY in the inspector!");
        }

        // Instantly hide the key model
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        // Show inventory icon in top-right
        if (inventoryIcon != null) inventoryIcon.SetActive(true);

        // Show "Key Acquired" text briefly
        if (keyAcquiredText != null) 
        {
            keyAcquiredText.SetActive(true);
            yield return new WaitForSeconds(textDisplayTime);
            keyAcquiredText.SetActive(false);
        }

        Destroy(gameObject);
    }
}