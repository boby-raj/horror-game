using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
//  DoorTrigger  —  Press E near the door to open/close it
//
//  SETUP
//  ─────
//  1. Attach this script to the Door GameObject
//  2. Make sure the Door has a Collider — this will act as the proximity zone.
//     Check "Is Trigger" on the Collider so the player can walk through the
//     zone without physically colliding with it.
//     (If your door also needs a SEPARATE solid collider for collision when
//      closed, add a second Box Collider without Is Trigger checked.)
//  3. Make sure your Player GameObject has the tag "Player" set in Inspector
//  4. Adjust openAngle / openSpeed to match your door size
// ──────────────────────────────────────────────────────────────────────────────

public class DoorTrigger : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Door Rotation")]
    [Tooltip("How many degrees the door swings open")]
    [SerializeField] private float openAngle = 90f;

    [Tooltip("How fast the door opens/closes")]
    [SerializeField] private float openSpeed = 2f;

    [Header("Sound (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   openSound;
    [SerializeField] private AudioClip   closeSound;

    [Header("UI Prompt (optional)")]
    [Tooltip("Drag a UI Text/Image object here to show 'Press E to open'")]
    [SerializeField] private GameObject interactPrompt;

    // ─── Private state ────────────────────────────────────────────────────────
    private bool _isOpen        = false;
    private bool _playerInRange = false;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    void Start()
    {
        _closedRotation = transform.rotation;
        _openRotation   = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y + openAngle,
            transform.eulerAngles.z);

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Update()
    {
        // Smoothly rotate door toward target state every frame
        Quaternion target = _isOpen ? _openRotation : _closedRotation;
        transform.rotation = Quaternion.Slerp(
            transform.rotation, target, Time.deltaTime * openSpeed);

        // Only listen for key press while player is in range
        if (_playerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleDoor();
        }
    }

    private void ToggleDoor()
    {
        _isOpen = !_isOpen;

        if (audioSource != null)
            audioSource.PlayOneShot(_isOpen ? openSound : closeSound);
    }

    // ── Proximity detection ──────────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            if (interactPrompt != null)
                interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }
    }
}