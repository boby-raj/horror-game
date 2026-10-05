using UnityEngine;

public class SimpleHingeDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float smoothSpeed = 2f;

    [Header("Key Settings")]
    public bool requiresKey = false;
    [SerializeField] private bool hasKey = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;

    [Header("UI Prompt")]
    public GameObject interactPrompt;

    private bool isOpen = false;
    private bool isPlayerNearby = false;
    private Quaternion defaultRotation;
    private Quaternion targetRotation;

    void Start()
    {
        defaultRotation = transform.localRotation;
        targetRotation = defaultRotation;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryToggleDoor();
        }
    }

    public void UnlockDoor()
    {
        hasKey = true;
        Debug.Log("Door unlocked!");
    }

    void TryToggleDoor()
    {
        if (requiresKey && !hasKey)
        {
            PlaySound(lockedSound);
            Debug.Log("The door is locked! You need a key.");
            return;
        }

        isOpen = !isOpen;

        if (isOpen)
        {
            targetRotation = defaultRotation * Quaternion.Euler(0, openAngle, 0);
            PlaySound(openSound);
        }
        else
        {
            targetRotation = defaultRotation;
            PlaySound(closeSound);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }
}