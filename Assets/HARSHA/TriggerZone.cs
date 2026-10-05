using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [Header("Reference to your HUD")]
    public GhostHUD ghostHUD;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            if (ghostHUD != null)
            {
                ghostHUD.TriggerOpen();
            }

            Destroy(gameObject);
        }
    }
}