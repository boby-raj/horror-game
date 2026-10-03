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

    [Header("Interaction Settings")]
    [Tooltip("Maximum distance to allow pickup (prevents giant trigger box overlap)")]
    public float maxPickupDistance = 4.5f;

    [Header("State")]
    public bool inarea = false;
    public bool isUsed = false;

    // Legacy serialized field preserved for Unity scene compatibility
    [HideInInspector] public GameObject thep;

    private void Start()
    {
        // Make sure the [E] prompt is hidden at start
        if (escreen != null) escreen.SetActive(false);

        // Auto-detect AudioSource if not assigned
        if (source == null) source = GetComponent<AudioSource>();
        if (source == null) source = GetComponentInParent<AudioSource>();
    }

    private void Update()
    {
        if (isUsed) return;

        // If player is inside the trigger zone around the can
        if (inarea)
        {
            // Verify player is within actual proximity
            if (IsPlayerCloseEnough())
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
            else
            {
                if (escreen != null && escreen.activeSelf)
                {
                    escreen.SetActive(false);
                }
            }
        }
    }

    private bool IsPlayerCloseEnough()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            float dist = Vector3.Distance(transform.position, cam.transform.position);
            return dist <= maxPickupDistance;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            return dist <= maxPickupDistance;
        }

        return true;
    }

    public void CollectFuelCan()
    {
        if (isUsed) return;

        // Prevent picking up multiple cans on the exact same frame!
        if (Time.frameCount == lastPickupFrame)
        {
            return;
        }
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
            AudioSource.PlayClipAtPoint(sound, transform.position);
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

        // 5. Deactivate ONLY THIS specific can GameObject so other cans remain in the map!
        gameObject.SetActive(false);
    }

    // --- Trigger Area Detection (Walking up to the can) ---
    private void OnTriggerEnter(Collider other)
    {
        if (isUsed) return;

        if (other.CompareTag("Player"))
        {
            inarea = true;
            if (IsPlayerCloseEnough() && escreen != null)
            {
                escreen.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isUsed) return;

        if (other.CompareTag("Player"))
        {
            inarea = false;
            if (escreen != null) escreen.SetActive(false);
        }
    }

    // --- IInteractable Implementation (Looking directly at the can with crosshair) ---
    public void OnHoverEnter()
    {
        if (isUsed) return;
        if (IsPlayerCloseEnough() && escreen != null)
        {
            escreen.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        if (isUsed) return;
        if (!inarea && escreen != null)
        {
            escreen.SetActive(false);
        }
    }

    public void Interact()
    {
        CollectFuelCan();
    }
}