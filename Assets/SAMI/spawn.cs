using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpawnerTrigger : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject objectToSpawn;
    public Transform spawnDestination;

    [Header("Trigger Settings")]
    public bool playerOnly = true;
    public bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnlyOnce && hasTriggered) return;
        if (playerOnly && !other.CompareTag("Player")) return;

        if (objectToSpawn != null && spawnDestination != null)
        {
            hasTriggered = true;
            Instantiate(objectToSpawn, spawnDestination.position, spawnDestination.rotation);
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
