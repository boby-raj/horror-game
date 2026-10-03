using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Horror Generator System with Fuel Timer, Audio, Lighting Control, and UI Counter.
/// Handles fueling, counting down remaining time, warning flickers, and automatic shutdown.
/// </summary>
public class GeneratorSystem : MonoBehaviour, IInteractable
{
    public static GeneratorSystem Instance { get; private set; }

    [Header("Fuel & Runtime Settings")]
    [Tooltip("Maximum runtime in seconds the generator can store")]
    public float maxFuelTime = 120f;
    [Tooltip("Seconds added per fuel can")]
    public float fuelPerCan = 60f;
    [Tooltip("Current seconds remaining on generator")]
    public float currentFuelTime = 0f;
    [Tooltip("Should the generator start running automatically on game start?")]
    public bool startRunningOnAwake = false;

    [Header("Generator State")]
    public bool isRunning = false;
    public bool isPlayerHoldingFuel = false;
    [Tooltip("If true, player must pick up a Jerry Can first. If false, pressing [E] on generator fuels it directly.")]
    public bool requireFuelCanToStart = false;

    private bool playerInArea = false;

    [Header("UI Counter & Display")]
    [Tooltip("TextMeshPro text on your HUD showing the remaining time (e.g. 02:30)")]
    public TextMeshProUGUI timerText;
    [Tooltip("Optional UI root/panel to show/hide with the generator")]
    public GameObject uiPanel;
    [Tooltip("Prefix before timer (e.g. 'POWER: ' or 'GENERATOR: ')")]
    public string timerPrefix = "GENERATOR: ";
    [Tooltip("Text shown when generator is offline")]
    public string offlineMessage = "GENERATOR: OFFLINE";
    [Tooltip("Seconds remaining when timer starts flashing red warning")]
    public float lowFuelWarningThreshold = 15f;
    public Color normalColor = Color.white;
    public Color lowFuelColor = new Color(1f, 0.25f, 0.25f);

    [Header("Lights & Powered Objects")]
    [Tooltip("Lights that stay ON while generator runs, and turn OFF when it stops")]
    public Light[] poweredLights;
    [Tooltip("Additional GameObjects enabled only while generator runs (e.g. electric gates)")]
    public GameObject[] poweredObjects;
    [Tooltip("Flicker lights when low on fuel for horror tension")]
    public bool flickerOnLowFuel = true;

    [Header("Audio")]
    public AudioSource engineAudioSource;
    public AudioClip engineStartSound;
    public AudioClip engineLoopSound;
    public AudioClip engineStopSound;

    [Header("Interaction / Hover")]
    public GameObject hoverPrompt;
    [Tooltip("Sound played when fueling the generator")]
    public AudioClip refuelSound;

    [Header("Events")]
    public UnityEvent onGeneratorStarted;
    public UnityEvent onGeneratorStopped;
    public UnityEvent onGeneratorRefueled;

    private Coroutine flickerCoroutine;
    private bool isWarningFlickering = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);

        if (engineAudioSource == null)
        {
            engineAudioSource = GetComponent<AudioSource>();
            if (engineAudioSource == null)
            {
                engineAudioSource = gameObject.AddComponent<AudioSource>();
                engineAudioSource.spatialBlend = 1f;
                engineAudioSource.loop = true;
                engineAudioSource.playOnAwake = false;
            }
        }
    }

    private void Start()
    {
        if (startRunningOnAwake && currentFuelTime > 0)
        {
            StartGenerator();
        }
        else if (!isRunning)
        {
            ApplyPowerState(false);
            UpdateUI();
        }

        if (hoverPrompt != null) hoverPrompt.SetActive(false);
    }

    private void Update()
    {
        // Support pressing E when standing inside trigger area
        if (playerInArea && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

        if (!isRunning) return;

        // Count down fuel time
        if (currentFuelTime > 0)
        {
            currentFuelTime -= Time.deltaTime;

            if (currentFuelTime <= lowFuelWarningThreshold && !isWarningFlickering && flickerOnLowFuel)
            {
                flickerCoroutine = StartCoroutine(LowFuelFlickerRoutine());
            }

            if (currentFuelTime <= 0)
            {
                currentFuelTime = 0;
                StopGenerator();
            }
        }

        UpdateUI();
    }

    /// <summary>
    /// Adds fuel to the generator and starts it if not already running.
    /// </summary>
    public void AddFuel(float seconds)
    {
        currentFuelTime = Mathf.Clamp(currentFuelTime + seconds, 0f, maxFuelTime);
        onGeneratorRefueled?.Invoke();

        if (refuelSound != null)
        {
            AudioSource.PlayClipAtPoint(refuelSound, transform.position);
        }

        if (!isRunning && currentFuelTime > 0)
        {
            StartGenerator();
        }
        else
        {
            UpdateUI();
        }
    }

    /// <summary>
    /// Starts the generator and turns on connected lights/power.
    /// </summary>
    public void StartGenerator()
    {
        if (currentFuelTime <= 0) return;

        isRunning = true;
        isWarningFlickering = false;

        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        // Play engine starter crank
        if (engineStartSound != null && engineAudioSource != null)
        {
            engineAudioSource.loop = false;
            engineAudioSource.clip = engineStartSound;
            engineAudioSource.Play();
            yield return new WaitForSeconds(engineStartSound.length * 0.85f);
        }

        // Loop continuous engine hum
        if (engineLoopSound != null && engineAudioSource != null && isRunning)
        {
            engineAudioSource.loop = true;
            engineAudioSource.clip = engineLoopSound;
            engineAudioSource.Play();
        }

        ApplyPowerState(true);
        onGeneratorStarted?.Invoke();
        UpdateUI();
    }

    /// <summary>
    /// Stops the generator and turns off connected lights/power.
    /// </summary>
    public void StopGenerator()
    {
        isRunning = false;
        isWarningFlickering = false;

        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

        // Play shutoff sound
        if (engineAudioSource != null)
        {
            engineAudioSource.Stop();
            engineAudioSource.loop = false;
            if (engineStopSound != null)
            {
                engineAudioSource.PlayOneShot(engineStopSound);
            }
        }

        ApplyPowerState(false);
        onGeneratorStopped?.Invoke();
        UpdateUI();
    }

    private void ApplyPowerState(bool powerOn)
    {
        if (poweredLights != null)
        {
            foreach (Light l in poweredLights)
            {
                if (l != null) l.enabled = powerOn;
            }
        }

        if (poweredObjects != null)
        {
            foreach (GameObject obj in poweredObjects)
            {
                if (obj != null) obj.SetActive(powerOn);
            }
        }
    }

    private IEnumerator LowFuelFlickerRoutine()
    {
        isWarningFlickering = true;

        while (isRunning && currentFuelTime <= lowFuelWarningThreshold && currentFuelTime > 0)
        {
            // Flickering lights for horror tension
            if (poweredLights != null && poweredLights.Length > 0)
            {
                foreach (Light l in poweredLights)
                {
                    if (l != null) l.enabled = (Random.value > 0.4f);
                }
            }

            yield return new WaitForSeconds(Random.Range(0.08f, 0.25f));
        }

        isWarningFlickering = false;
    }

    private void UpdateUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
        }

        if (timerText == null) return;

        if (isRunning && currentFuelTime > 0)
        {
            int minutes = Mathf.FloorToInt(currentFuelTime / 60f);
            int seconds = Mathf.FloorToInt(currentFuelTime % 60f);
            timerText.text = $"{timerPrefix}{minutes:00}:{seconds:00}";

            if (currentFuelTime <= lowFuelWarningThreshold)
            {
                // Flashing red effect
                bool flash = Mathf.PingPong(Time.time * 4f, 1f) > 0.5f;
                timerText.color = flash ? lowFuelColor : normalColor;
            }
            else
            {
                timerText.color = normalColor;
            }
        }
        else
        {
            timerText.text = offlineMessage;
            timerText.color = lowFuelColor;
        }
    }

    // --- IInteractable Implementation (Player looking at generator and pressing E) ---

    public void Interact()
    {
        if (isPlayerHoldingFuel)
        {
            isPlayerHoldingFuel = false;
            AddFuel(fuelPerCan);
            if (hoverPrompt != null) hoverPrompt.SetActive(false);
        }
        else if (!requireFuelCanToStart)
        {
            AddFuel(fuelPerCan);
            if (hoverPrompt != null) hoverPrompt.SetActive(false);
        }
        else if (!isRunning && currentFuelTime > 0)
        {
            StartGenerator();
            if (hoverPrompt != null) hoverPrompt.SetActive(false);
        }
    }

    public void OnHoverEnter()
    {
        if (hoverPrompt != null)
        {
            hoverPrompt.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        if (hoverPrompt != null)
        {
            hoverPrompt.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInArea = true;
            if (isPlayerHoldingFuel)
            {
                isPlayerHoldingFuel = false;
                AddFuel(fuelPerCan);
            }
            else if (hoverPrompt != null)
            {
                hoverPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInArea = false;
            if (hoverPrompt != null)
            {
                hoverPrompt.SetActive(false);
            }
        }
    }
}
