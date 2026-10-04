using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class PitResetTrigger : MonoBehaviour
{
    private bool hasTriggered = false;
 public AudioSource audioSource; // Reference to the AudioSource component
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
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            audioSource.Play(); // Play the audio clip when the player enters the trigger
        }
    }
}
