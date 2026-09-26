using UnityEngine;

public class ChaseTrigger : MonoBehaviour
{
    [Tooltip("Drag agatha_RIG here")]
    public GameObject enemyToActivate;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Keeps colliders active and only runs the spawn logic once
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (enemyToActivate != null)
            {
                enemyToActivate.SetActive(true);
            }
        }
    }
}