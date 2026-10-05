using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

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

        if (playerInArea && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

        if (!isRunning) return;

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

    public void StartGenerator()
    {
        if (currentFuelTime <= 0) return;

        isRunning = true;
        isWarningFlickering = false;

        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {

        if (engineStartSound != null && engineAudioSource != null)
        {
            engineAudioSource.loop = false;
            engineAudioSource.clip = engineStartSound;
            engineAudioSource.Play();
            yield return new WaitForSeconds(engineStartSound.length * 0.85f);
        }

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

    public void StopGenerator()
    {
        isRunning = false;
        isWarningFlickering = false;

        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

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

    private LightmapData[] originalLightmaps;
    private FlickeringLight[] cachedFlickers;

    private bool IsRedLight(Light l)
    {
        if (l == null) return false;
        Color c = l.color;
        if (c.r > 0.4f && c.r > c.g * 1.8f && c.r > c.b * 1.8f)
        {
            return true;
        }
        if (l.gameObject.name.IndexOf("red", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }
        return false;
    }

    private void ApplyPowerState(bool powerOn)
    {
        if (originalLightmaps == null || originalLightmaps.Length == 0)
        {
            originalLightmaps = LightmapSettings.lightmaps;
        }

        if (poweredLights == null || poweredLights.Length == 0)
        {
            Light[] allLights = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            System.Collections.Generic.List<Light> list = new System.Collections.Generic.List<Light>();
            foreach (Light l in allLights)
            {
                if (l == null || l.type == LightType.Directional) continue;
                if (IsRedLight(l))
                {
                    l.enabled = true;
                    continue;
                }
                Transform curr = l.transform;
                bool skip = false;
                while (curr != null)
                {
                    if (curr.CompareTag("Player") || curr.CompareTag("MainCamera") ||
                        curr.GetComponent<Camera>() != null ||
                        curr.GetComponent<deadcollide>() != null ||
                        curr.GetComponent<deadcollide2>() != null ||
                        curr.name.IndexOf("Player", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        curr.name.IndexOf("Camera", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        curr.name.IndexOf("Flashlight", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        curr.name.IndexOf("FlippedTV", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        curr.name.IndexOf("horrorlady", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        skip = true;
                        break;
                    }
                    curr = curr.parent;
                }
                if (!skip) list.Add(l);
            }
            poweredLights = list.ToArray();
        }

        if (cachedFlickers == null)
        {
            cachedFlickers = FindObjectsByType<FlickeringLight>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        if (cachedFlickers != null)
        {
            foreach (FlickeringLight fl in cachedFlickers)
            {
                if (fl == null) continue;
                Light flLight = fl.myLight != null ? fl.myLight : fl.GetComponent<Light>();
                if (flLight != null && IsRedLight(flLight))
                {
                    fl.enabled = true;
                    continue;
                }
                fl.enabled = powerOn;
            }
        }

        if (poweredLights != null)
        {
            foreach (Light l in poweredLights)
            {
                if (l == null) continue;
                if (IsRedLight(l))
                {
                    l.enabled = true;
                    continue;
                }
                l.enabled = powerOn;
            }
        }

        if (poweredObjects != null)
        {
            foreach (GameObject obj in poweredObjects)
            {
                if (obj != null) obj.SetActive(powerOn);
            }
        }

        if (powerOn)
        {
            if (originalLightmaps != null && originalLightmaps.Length > 0)
            {
                LightmapSettings.lightmaps = originalLightmaps;
            }
        }
        else
        {
            LightmapSettings.lightmaps = new LightmapData[0];
        }
    }

    private IEnumerator LowFuelFlickerRoutine()
    {
        isWarningFlickering = true;

        while (isRunning && currentFuelTime <= lowFuelWarningThreshold && currentFuelTime > 0)
        {

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
