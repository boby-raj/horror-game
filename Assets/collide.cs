using UnityEngine;

public class ChaseTrigger : MonoBehaviour
{
    [Tooltip("Drag agatha_RIG here")]
    public GameObject enemyToActivate;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {

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