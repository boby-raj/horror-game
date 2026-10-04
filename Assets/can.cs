using UnityEngine;

/// <summary>
/// Fuel Can script.
/// Shows [E] prompt when player approaches or looks at the can.
/// Pressing [E] collects ONLY this can, plays sound, hides prompt, and increments fuel count.
/// </summary>
public class lcan : MonoBehaviour, IInteractable
{
    // Global counter tracking how many fuel cans the player is currently holding
    public static int playerFuelCount = 0;

    // Frame throttle to prevent picking up multiple cans on the exact same frame
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

    // Legacy serialized field preserved for Unity scene compatibility
    [HideInInspector] public GameObject thep;

    private void Start()
    {
        // Auto-find [E] UI if not assigned or broken
        if (escreen == null)
        {
            GameObject eObj = GameObject.Find("[E]");
            if (eObj != null) escreen = eObj;
        }

        if (escreen != null) escreen.SetActive(false);

        // Auto-detect AudioSource if not assigned
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

        // If player is inside the trigger zone or hovering
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

        // Prevent picking up multiple cans on the exact same frame
        if (Time.frameCount == lastPickupFrame) return;
        lastPickupFrame = Time.frameCount;

        isUsed = true;
        inarea = false;

        // Increment player's fuel inventory
        playerFuelCount++;
        Debug.Log($"[FuelCan] Picked up fuel can! Player is now holding: {playerFuelCount} can(s).");

        // 1. Hide the [E] prompt
        if (escreen != null) escreen.SetActive(false);

        // 2. Play pickup sound
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

        // 3. Enable held/inventory visual if assigned
        if (tru != null && tru != gameObject)
        {
            tru.SetActive(true);
        }

        // 4. Also notify GeneratorSystem if used in the project
        if (GeneratorSystem.Instance != null)
        {
            GeneratorSystem.Instance.isPlayerHoldingFuel = true;
            GeneratorSystem.Instance.fuelPerCan = 60f;
        }

        // 5. Deactivate ONLY THIS specific can GameObject!
        // We DO NOT deactivate 'thep' so other cans remain in the scene!
        gameObject.SetActive(false);
    }

    // --- Trigger Area Detection (Walking up to the can) ---
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

    // --- IInteractable Implementation (Looking directly at the can with crosshair) ---
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