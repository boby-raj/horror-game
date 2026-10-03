using UnityEngine;

/// <summary>
/// Fuel Can script.
/// Shows [E] prompt when player approaches or looks at the can.
/// Pressing [E] collects the can, plays sound, hides prompt, and activates generator readiness.
/// </summary>
public class lcan : MonoBehaviour, IInteractable
{
    [Header("UI Prompts")]
    [Tooltip("The [E] UI prompt on the screen (e.g. Press E to pick up)")]
    public GameObject escreen;

    [Header("Objects & References")]
    [Tooltip("The Jerry Can object in the scene to disappear when picked up (MANDATORY in lever_script)")]
    public GameObject thep;
    [Tooltip("Optional held can or inventory object to appear upon pickup")]
    public GameObject tru;

    [Header("Audio")]
    public AudioSource source;
    public AudioClip sound;

    [Header("State")]
    public bool inarea = false;
    public bool isUsed = false;

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
            // Show [E] prompt when near
            if (escreen != null && !escreen.activeSelf)
            {
                escreen.SetActive(true);
            }

            // Press E to pick up
            if (Input.GetKeyDown(KeyCode.E))
            {
                CollectFuelCan();
            }
        }
    }

    public void CollectFuelCan()
    {
        if (isUsed) return;
        isUsed = true;
        inarea = false;

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

        // 3. Enable held/inventory can if assigned
        if (tru != null && tru != gameObject)
        {
            tru.SetActive(true);
        }

        // 4. Disable the can so the generator knows we picked it up
        if (thep != null)
        {
            thep.SetActive(false);
        }

        // Also notify GeneratorSystem if used in the project
        if (GeneratorSystem.Instance != null)
        {
            GeneratorSystem.Instance.isPlayerHoldingFuel = true;
        }

        // Disable this can GameObject if it's not the same object as thep
        if (gameObject != thep)
        {
            gameObject.SetActive(false);
        }
    }

    // --- Trigger Area Detection (Walking up to the can) ---
    private void OnTriggerEnter(Collider other)
    {
        if (isUsed) return;

        if (other.CompareTag("Player"))
        {
            inarea = true;
            if (escreen != null) escreen.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isUsed) return;

        if (other.CompareTag("Player"))
        {
            inarea = false;
            if (escreen != null) escreen.SetActive(false);
            // Notice: We do NOT disable thep here! The can stays visible on the floor!
        }
    }

    // --- IInteractable Implementation (Looking directly at the can with crosshair) ---
    public void OnHoverEnter()
    {
        if (isUsed) return;
        if (escreen != null) escreen.SetActive(true);
    }

    public void OnHoverExit()
    {
        if (isUsed) return;
        if (!inarea && escreen != null) escreen.SetActive(false);
    }

    public void Interact()
    {
        CollectFuelCan();
    }
}