using System.Collections.Generic;
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
public class lever_script : MonoBehaviour
{
    [Header("Generator & Lever Controls")]
    public Animator lv_animator;
    public GameObject lev_ui;
    public GameObject MANDATORY;
    public GameObject alllights;
    public List<GameObject> lightsList;

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
    [Tooltip("Optional TextMeshPro on Canvas to show generator timer")]
    public TextMeshProUGUI timerText;
    [Tooltip("Show atmospheric on-screen timer while generator is running")]
    public bool showOnScreenTimer = true;

    private bool inarea = false;

    private void Start()
    {
        if (close_text != null) close_text.SetActive(false);
        if (open_text != null) open_text.SetActive(false);
        if (lev_ui != null) lev_ui.SetActive(false);

        // Ensure lights start OFF if generator is not running
        if (!isRunning) SetAllLights(false);

        if (grator == null)
        {
            grator = GetComponent<AudioSource>();
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
            bool hasFuel = HasFuel();

            if (!isRunning)
            {
                // Generator is OFFLINE
                if (hasFuel)
                {
                    if (close_text != null && !close_text.activeSelf) close_text.SetActive(true);
                    if (open_text != null && open_text.activeSelf) open_text.SetActive(false);
                    if (lev_ui != null && !lev_ui.activeSelf) lev_ui.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        FuelAndStartGenerator();
                    }
                }
                else
                {
                    if (open_text != null && !open_text.activeSelf) open_text.SetActive(true);
                    if (close_text != null && close_text.activeSelf) close_text.SetActive(false);
                    if (lev_ui != null && lev_ui.activeSelf) lev_ui.SetActive(false);
                }
            }
            else
            {
                // Generator is RUNNING
                if (hasFuel && allowRefuelWhileRunning)
                {
                    if (close_text != null && !close_text.activeSelf) close_text.SetActive(true);
                    if (open_text != null && open_text.activeSelf) open_text.SetActive(false);
                    if (lev_ui != null && !lev_ui.activeSelf) lev_ui.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        AddFuelWhileRunning();
                    }
                }
                else
                {
                    // While running and no fuel held, don't show prompts
                    if (close_text != null && close_text.activeSelf) close_text.SetActive(false);
                    if (open_text != null && open_text.activeSelf) open_text.SetActive(false);
                    if (lev_ui != null && lev_ui.activeSelf) lev_ui.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Turns on/off the single alllights object and every light in lightsList.
    /// </summary>
    private void SetAllLights(bool on)
    {
        if (alllights != null) alllights.SetActive(on);

        if (lightsList != null)
        {
            foreach (var l in lightsList)
            {
                if (l != null) l.SetActive(on);
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
        SetAllLights(true);

        // Notify GeneratorSystem if attached
        if (GeneratorSystem.Instance != null)
        {
            GeneratorSystem.Instance.AddFuel(generatorRunDuration);
        }

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
        SetAllLights(false);

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
            timerText.text = "GENERATOR: OFFLINE";
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
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(Mathf.Max(0f, currentRunTime) / 60f);
            int seconds = Mathf.FloorToInt(Mathf.Max(0f, currentRunTime) % 60f);
            timerText.text = string.Format("GENERATOR: {0:00}:{1:00}", minutes, seconds);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inarea = true;

            if (!isRunning)
            {
                if (HasFuel())
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
                if (HasFuel() && allowRefuelWhileRunning)
                {
                    if (close_text != null) close_text.SetActive(true);
                    if (lev_ui != null) lev_ui.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inarea = false;
            if (open_text != null) open_text.SetActive(false);
            if (close_text != null) close_text.SetActive(false);
            if (lev_ui != null) lev_ui.SetActive(false);
        }
    }

    // OnGUI removed — use the timerText (TextMeshProUGUI) in the Inspector instead.
    // It is updated every frame via UpdateTimerDisplay() which is much more performant.
}