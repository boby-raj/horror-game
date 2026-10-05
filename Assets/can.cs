using UnityEngine;

public class lcan : MonoBehaviour, IInteractable
{

    public static int playerFuelCount = 0;

    private static int lastPickupFrame = -1;

    [Header("UI Prompts")]
    [Tooltip("The [E] UI prompt on the screen (e.g. Press E to pick up)")]
    public GameObject escreen;

    [Header("Optional Inventory Visuals")]
    [Tooltip("Optional held can or inventory object to appear upon pickup")]
    public GameObject tru;

    [Header("Audio")]
    public AudioSource source;
    public AudioClip sound;

    [Header("State")]
    public bool inarea = false;
    public bool isUsed = false;

    [HideInInspector] public GameObject thep;

    private void Start()
    {

        if (escreen == null)
        {
            GameObject eObj = GameObject.Find("[E]");
            if (eObj != null) escreen = eObj;
        }

        if (escreen != null) escreen.SetActive(false);

        if (source == null) source = GetComponent<AudioSource>();
        if (source == null) source = GetComponentInParent<AudioSource>();
    }

    private void OnDisable()
    {
        inarea = false;
        if (escreen != null && escreen.activeSelf) escreen.SetActive(false);
    }

    private void Update()
    {
        if (isUsed) return;

        if (inarea)
        {
            if (escreen != null && !escreen.activeSelf)
            {
                escreen.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                CollectFuelCan();
            }
        }
    }

    public void CollectFuelCan()
    {
        if (isUsed) return;

        if (Time.frameCount == lastPickupFrame) return;
        lastPickupFrame = Time.frameCount;

        isUsed = true;
        inarea = false;

        playerFuelCount++;
        Debug.Log($"[FuelCan] Picked up fuel can! Player is now holding: {playerFuelCount} can(s).");

        if (escreen != null) escreen.SetActive(false);

        if (sound != null)
        {
            if (source != null)
            {
                source.PlayOneShot(sound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(sound, transform.position);
            }
        }

        if (tru != null && tru != gameObject)
        {
            tru.SetActive(true);
        }

        if (GeneratorSystem.Instance != null)
        {
            GeneratorSystem.Instance.isPlayerHoldingFuel = true;
            GeneratorSystem.Instance.fuelPerCan = 60f;
        }

        gameObject.SetActive(false);
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        return other.CompareTag("Player") ||
               other.GetComponent<CharacterController>() != null ||
               other.name.IndexOf("Capsule", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               other.name.IndexOf("Player", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isUsed) return;

        if (IsPlayer(other))
        {
            inarea = true;
            if (escreen != null && !escreen.activeSelf) escreen.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isUsed) return;

        if (!inarea && IsPlayer(other))
        {
            inarea = true;
            if (escreen != null && !escreen.activeSelf) escreen.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isUsed) return;

        if (IsPlayer(other))
        {
            inarea = false;
            if (escreen != null && escreen.activeSelf) escreen.SetActive(false);
        }
    }

    public void OnHoverEnter()
    {
        if (isUsed) return;
        inarea = true;
        if (escreen != null) escreen.SetActive(true);
    }

    public void OnHoverExit()
    {
        if (isUsed) return;
        inarea = false;
        if (escreen != null) escreen.SetActive(false);
    }

    public void Interact()
    {
        CollectFuelCan();
    }
}