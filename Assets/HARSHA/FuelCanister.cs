using UnityEngine;

/// <summary>
/// Attach to Jerry Can / Fuel Canister.
/// Can be picked up with [E] using PlayerInteraction or by walking into its trigger.
/// </summary>
public class FuelCanister : MonoBehaviour, IInteractable
{
    [Header("Fuel Settings")]
    [Tooltip("Seconds of fuel this can provides")]
    public float fuelAmount = 60f;
    [Tooltip("If true, directly fuels the generator upon pickup. If false, player carries it until interacting with the generator.")]
    public bool fuelGeneratorDirectlyOnPickup = false;

    [Header("Audio & Effects")]
    public AudioClip pickupSound;
    public GameObject hoverPrompt;

    private bool isCollected = false;

    private void Start()
    {
        if (hoverPrompt != null) hoverPrompt.SetActive(false);
    }

    public void Interact()
    {
        Collect();
    }

    public void OnHoverEnter()
    {
        if (!isCollected && hoverPrompt != null)
        {
            hoverPrompt.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        if (hoverPrompt != null)
        {
            hoverPrompt.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            // If hover prompt is active or auto pickup enabled
            if (hoverPrompt != null && hoverPrompt.activeSelf)
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        if (hoverPrompt != null) hoverPrompt.SetActive(false);

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        if (GeneratorSystem.Instance != null)
        {
            if (fuelGeneratorDirectlyOnPickup)
            {
                GeneratorSystem.Instance.AddFuel(fuelAmount);
            }
            else
            {
                GeneratorSystem.Instance.isPlayerHoldingFuel = true;
                GeneratorSystem.Instance.fuelPerCan = fuelAmount;
            }
        }

        gameObject.SetActive(false);
    }
}
