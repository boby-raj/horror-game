using UnityEngine;
using TMPro;

/// <summary>
/// Generator and Lever Script.
/// - Shows 'NEED FUEL' when player approaches without fuel.
/// - Shows '[E] to fuel' when player approaches with fuel.
/// - Powers generator for generatorRunDuration (e.g. 60 seconds).
/// - Displays countdown timer (HUD/TextMeshPro & atmospheric on-screen UI).
/// - Automatically shuts off engine and lights when time expires.
/// - Allows refueling again with another fuel can.
/// </summary>
public class lever_script : MonoBehaviour, IInteractable
{
    [Header("Generator & Lever Controls")]
    public Animator lv_animator;
    public GameObject lev_ui;
    public GameObject MANDATORY;
    public GameObject alllights;

    [Header("Audio")]
    public AudioSource grator;
    public AudioClip start_sound;
    public AudioClip stop_sound;

    [Header("Fuel Check UI")]
    public lcan can_script;
    public GameObject open_text;   // 'NEED FUEL'
    public GameObject close_text;  // '[E] to fuel'

    [Header("Timer & Runtime Settings")]
    [Tooltip("How many seconds the generator runs per fuel can")]
    public float generatorRunDuration = 60f;
    [Tooltip("Current remaining runtime in seconds")]
    public float currentRunTime = 0f;
    [Tooltip("Is the generator currently running?")]
    public bool isRunning = false;
    [Tooltip("Allow adding more time if player brings fuel while generator is running")]
    public bool allowRefuelWhileRunning = true;

    [Header("UI Counter Display")]
    [Tooltip("Panel with background image holding the timer UI")]
    public GameObject timerPanel;
    [Tooltip("TextMeshPro text on the panel displaying the time")]
    public TextMeshProUGUI timerText;
    [Tooltip("Text color for the timer (preserved from user's settings)")]
    public Color timerTextColor = Color.white;
    [Tooltip("Show atmospheric on-screen timer while generator is running (fallback if no canvas text is active)")]
    public bool showOnScreenTimer = true;

    private bool inarea = false;

    private void Awake()
    {
        ResolveTimerReferences();
    }

    private void Start()
    {
        ResolveTimerReferences();

        if (close_text != null) close_text.SetActive(false);
        if (open_text != null) open_text.SetActive(false);
        if (lev_ui != null) lev_ui.SetActive(false);
        if (timerPanel != null) timerPanel.SetActive(false);

        // Ensure lights start OFF if generator is not running
        if (!isRunning && alllights != null)
        {
            alllights.SetActive(false);
        }

        if (grator == null)
        {
            grator = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Auto-resolves timerText if unassigned or if linked as GameObject in scene YAML.
    /// Preserves the user's chosen text color.
    /// </summary>
    public void ResolveTimerReferences()
    {
        if (timerText == null && timerPanel != null)
        {
            timerText = timerPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (timerText == null)
        {
            TextMeshProUGUI[] allTMP = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var tmp in allTMP)
            {
                if (tmp.gameObject.name.IndexOf("Timer", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    tmp.gameObject.name.IndexOf("Counter", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    timerText = tmp;
                    if (timerPanel == null && tmp.transform.parent != null)
                    {
                        timerPanel = tmp.transform.parent.gameObject;
                    }
                    break;
                }
            }
        }

        // Keep the exact text colour the user kept on the UI component
        if (timerText != null)
        {
            timerTextColor = timerText.color;
        }
    }

    private void Update()
    {
        // 1. If generator is running, count down remaining time
        if (isRunning)
        {
            currentRunTime -= Time.deltaTime;

            UpdateTimerDisplay();

            if (currentRunTime <= 0f)
            {
                StopGenerator();
            }
        }

        // 2. Interaction logic when player is near the generator
        if (inarea)
        {
            UpdateInteractionPrompts();

            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
    }

    /// <summary>
    /// Checks whether the player is holding a fuel can.
    /// </summary>
    public bool HasFuel()
    {
        if (lcan.playerFuelCount > 0) return true;
        if (can_script != null && can_script.isUsed) return true;
        if (MANDATORY != null && !MANDATORY.activeSelf) return true;
        if (GeneratorSystem.Instance != null && GeneratorSystem.Instance.isPlayerHoldingFuel) return true;
        return false;
    }

    /// <summary>
    /// Starts the generator and powers on lights and audio.
    /// </summary>
    public void FuelAndStartGenerator()
    {
        ConsumeFuel();

        isRunning = true;
        currentRunTime = generatorRunDuration;

        // Hide prompt texts
        if (close_text != null) close_text.SetActive(false);
        if (open_text != null) open_text.SetActive(false);
        if (lev_ui != null) lev_ui.SetActive(false);

        // Lever animation
        if (lv_animator != null)
        {
            lv_animator.enabled = true;
            lv_animator.Play(0);
        }

        // Engine sound loop
        if (grator != null)
        {
            if (start_sound != null)
            {
                grator.clip = start_sound;
                grator.loop = true;
                grator.Play();
            }
            else
            {
                grator.Play();
            }
        }

        // Turn ON all powered lights
        if (alllights != null)
        {
            alllights.SetActive(true);
        }

        // Notify GeneratorSystem if attached
        if (GeneratorSystem.Instance != null)
        {
            GeneratorSystem.Instance.AddFuel(generatorRunDuration);
        }

        // Show timer UI panel if assigned
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
        }
        else if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }

        UpdateTimerDisplay();

        Debug.Log($"[Generator] Started! Running for {generatorRunDuration} seconds.");
    }

    /// <summary>
    /// Adds additional runtime if player refuels while the generator is running.
    /// </summary>
    public void AddFuelWhileRunning()
    {
        ConsumeFuel();
        currentRunTime += generatorRunDuration;

        if (close_text != null) close_text.SetActive(false);
        if (lev_ui != null) lev_ui.SetActive(false);

        if (GeneratorSystem.Instance != null)
        {
            GeneratorSystem.Instance.AddFuel(generatorRunDuration);
        }

        Debug.Log($"[Generator] Refueled while running! Time remaining: {currentRunTime:F1}s.");
    }

    private void ConsumeFuel()
    {
        if (lcan.playerFuelCount > 0)
        {
            lcan.playerFuelCount--;
        }

        if (can_script != null)
        {
            can_script.isUsed = false;
        }

        if (GeneratorSystem.Instance != null)
        {
            GeneratorSystem.Instance.isPlayerHoldingFuel = false;
        }
    }

    /// <summary>
    /// Shuts down the generator when fuel runs out.
    /// </summary>
    public void StopGenerator()
    {
        currentRunTime = 0f;
        isRunning = false;

        // Turn OFF lights
        if (alllights != null)
        {
            alllights.SetActive(false);
        }

        // Stop engine sound
        if (grator != null)
        {
            grator.Stop();
        }

        // Optional power-down sound
        if (stop_sound != null)
        {
            AudioSource.PlayClipAtPoint(stop_sound, transform.position);
        }

        // Reset lever animation
        if (lv_animator != null)
        {
            lv_animator.enabled = false;
        }

        if (timerText != null)
        {
            timerText.text = "00:00";
        }

        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }

        Debug.Log("[Generator] Fuel expired! Generator stopped. Lights turned OFF.");

        // If player is still standing in the generator area, update prompt
        if (inarea)
        {
            if (HasFuel())
            {
                if (close_text != null) close_text.SetActive(true);
            }
            else
            {
                if (open_text != null) open_text.SetActive(true);
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
        {
            ResolveTimerReferences();
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(Mathf.Max(0f, currentRunTime) / 60f);
            int seconds = Mathf.FloorToInt(Mathf.Max(0f, currentRunTime) % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Keep the exact text colour the user kept
            timerText.color = timerTextColor;
        }
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other == null) return false;
        return other.CompareTag("Player") ||
               other.GetComponent<CharacterController>() != null ||
               other.name.IndexOf("Capsule", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               other.name.IndexOf("Player", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayerCollider(other))
        {
            inarea = true;
            UpdateInteractionPrompts();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!inarea && IsPlayerCollider(other))
        {
            inarea = true;
            UpdateInteractionPrompts();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayerCollider(other))
        {
            inarea = false;
            HideAllPrompts();
        }
    }

    private void UpdateInteractionPrompts()
    {
        bool hasFuel = HasFuel();

        if (!isRunning)
        {
            if (hasFuel)
            {
                if (close_text != null) close_text.SetActive(true);
                if (lev_ui != null) lev_ui.SetActive(true);
                if (open_text != null) open_text.SetActive(false);
            }
            else
            {
                if (open_text != null) open_text.SetActive(true);
                if (close_text != null) close_text.SetActive(false);
                if (lev_ui != null) lev_ui.SetActive(false);
            }
        }
        else
        {
            if (hasFuel && allowRefuelWhileRunning)
            {
                if (close_text != null) close_text.SetActive(true);
                if (lev_ui != null) lev_ui.SetActive(true);
                if (open_text != null) open_text.SetActive(false);
            }
            else
            {
                HideAllPrompts();
            }
        }
    }

    private void HideAllPrompts()
    {
        if (open_text != null) open_text.SetActive(false);
        if (close_text != null) close_text.SetActive(false);
        if (lev_ui != null) lev_ui.SetActive(false);
    }

    // --- IInteractable Implementation (Looking directly at generator/lever with crosshair) ---
    public void OnHoverEnter()
    {
        inarea = true;
        UpdateInteractionPrompts();
    }

    public void OnHoverExit()
    {
        inarea = false;
        HideAllPrompts();
    }

    public void Interact()
    {
        if (HasFuel())
        {
            if (!isRunning)
            {
                FuelAndStartGenerator();
            }
            else if (allowRefuelWhileRunning)
            {
                AddFuelWhileRunning();
            }
        }
        else if (!isRunning)
        {
            // Flash NEED FUEL prompt
            if (open_text != null) open_text.SetActive(true);
            if (close_text != null) close_text.SetActive(false);
        }
    }

    // OnGUI fallback removed in favor of TextMeshProUGUI on Canvas.
}