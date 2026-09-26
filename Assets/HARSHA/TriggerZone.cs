using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [Header("Reference to your HUD")]
    public GhostHUD ghostHUD; // Drag your GhostPanel here

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the zone (make sure your player has the "Player" tag)
        if (other.CompareTag("Player"))
        {
            if (ghostHUD != null)
            {
                ghostHUD.TriggerOpen();
            }

            // Destroy this trigger so it only happens once
            Destroy(gameObject);
        }
    }
}