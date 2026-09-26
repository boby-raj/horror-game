using UnityEngine;
public class keychain : MonoBehaviour
{
    public SimpleHingeDoor targetDoor;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetDoor.UnlockDoor(); // Unlocks the assigned door
            Destroy(gameObject);      // Removes the key from the world
        }
    }
}