using UnityEngine;

public class TriggerDeactivateTarget : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Drag the GameObject you want to turn off here")]
    public GameObject targetToDeactivate;

    [Tooltip("Optional tag filter. Leave empty to trigger on anything.")]
    public string requiredTag = "Player";

    [Header("Audio Settings")]
    [Tooltip("Drag your sound effect clip here")]
    public AudioClip triggerSound;

    [Tooltip("Volume of the sound (0.0 to 1.0)")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    private void OnTriggerEnter(Collider other)
    {

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            return;
        }

        if (triggerSound != null)
        {
            AudioSource.PlayClipAtPoint(triggerSound, transform.position, soundVolume);
        }

        if (targetToDeactivate != null)
        {
            targetToDeactivate.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No Target GameObject assigned to deactivate!", this);
        }
    }
}