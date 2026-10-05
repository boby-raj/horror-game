using UnityEngine;

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

        Quaternion target = _isOpen ? _openRotation : _closedRotation;
        transform.rotation = Quaternion.Slerp(
            transform.rotation, target, Time.deltaTime * openSpeed);

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