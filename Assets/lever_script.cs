using UnityEngine;
using TMPro;

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
    public GameObject open_text;
    public GameObject close_text;

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
    private GUIStyle timerStyle;
    private readonly System.Collections.Generic.List<Light> sceneLights = new System.Collections.Generic.List<Light>();
    private readonly System.Collections.Generic.List<FlickeringLight> sceneFlickers = new System.Collections.Generic.List<FlickeringLight>();
    private LightmapData[] originalLightmaps;
    private bool lightsResolved = false;

    private void Awake()
    {
        ResolveTimerReferences();
        ResolveLightReferences();
    }

    private void Start()
    {
        ResolveTimerReferences();
        ResolveLightReferences();

        if (close_text != null) close_text.SetActive(false);
        if (open_text != null) open_text.SetActive(false);
        if (lev_ui != null) lev_ui.SetActive(false);
        if (timerPanel != null) timerPanel.SetActive(false);

        if (isRunning)
        {
            if (currentRunTime <= 0f)
            {
                currentRunTime = generatorRunDuration > 0f ? generatorRunDuration : 9999f;
            }
            SetGeneratorLights(true);
            if (timerPanel != null)
            {
                timerPanel.SetActive(true);
            }
            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
            }
            if (grator != null && start_sound != null)
            {
                grator.clip = start_sound;
                grator.loop = true;
                grator.Play();
            }
            if (lv_animator != null)
            {
                lv_animator.enabled = true;
                lv_animator.Play(0);
            }
        }
        else
        {
            SetGeneratorLights(false);
        }

        if (grator == null)
        {
            grator = GetComponent<AudioSource>();
        }
    }

    public void ResolveLightReferences()
    {
        if (originalLightmaps == null || originalLightmaps.Length == 0)
        {
            originalLightmaps = LightmapSettings.lightmaps;
        }

        if (alllights == null)
        {
            pointlight[] plScripts = FindObjectsByType<pointlight>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (plScripts != null && plScripts.Length > 0)
            {
                alllights = plScripts[0].gameObject;
            }
            else
            {
                GameObject found = GameObject.Find("pointlightwork");
                if (found != null) alllights = found;
            }
        }

        if (alllights != null)
        {
            alllights.SetActive(true);
            pointlight pl = alllights.GetComponent<pointlight>();
            if (pl != null)
            {
                pl.enabled = false;
                if (pl.light1 != null)
                {
                    pl.light1.SetActive(true);
                }
            }
        }

        GameObject playerFlashlightObj = null;
        flahlight[] flashlights = FindObjectsByType<flahlight>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (flashlights != null && flashlights.Length > 0 && flashlights[0].lightt != null)
        {
            playerFlashlightObj = flashlights[0].lightt;
        }

        sceneLights.Clear();
        Light[] allSceneLights = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Light l in allSceneLights)
        {
            if (l == null) continue;
            if (l.type == LightType.Directional) continue;
            if (playerFlashlightObj != null && (l.gameObject == playerFlashlightObj || l.transform.IsChildOf(playerFlashlightObj.transform))) continue;

            if (IsRedLight(l))
            {
                l.enabled = true;
                continue;
            }

            if (IsExcludedLight(l.transform)) continue;

            sceneLights.Add(l);
        }

        sceneFlickers.Clear();
        FlickeringLight[] allFlickers = FindObjectsByType<FlickeringLight>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (FlickeringLight fl in allFlickers)
        {
            if (fl == null) continue;
            Light flLight = fl.myLight != null ? fl.myLight : fl.GetComponent<Light>();
            if (flLight != null && IsRedLight(flLight))
            {
                fl.enabled = true;
                continue;
            }
            if (IsExcludedLight(fl.transform)) continue;
            sceneFlickers.Add(fl);
        }

        lightsResolved = true;
    }

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

    private bool IsExcludedLight(Transform t)
    {
        Transform curr = t;
        while (curr != null)
        {
            if (curr.CompareTag("Player") || curr.CompareTag("MainCamera")) return true;
            if (curr.GetComponent<Camera>() != null || curr.GetComponent<CharacterController>() != null) return true;
            if (curr.GetComponent<deadcollide>() != null || curr.GetComponent<deadcollide2>() != null) return true;
            string n = curr.name;
            if (n.IndexOf("Player", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Camera", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Capsule", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("Flashlight", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("FlippedTV", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                n.IndexOf("horrorlady", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            curr = curr.parent;
        }
        return false;
    }

    public void SetGeneratorLights(bool turnOn)
    {
        if (!lightsResolved)
        {
            ResolveLightReferences();
        }

        for (int i = 0; i < sceneFlickers.Count; i++)
        {
            if (sceneFlickers[i] != null)
            {
                sceneFlickers[i].enabled = turnOn;
            }
        }

        for (int i = 0; i < sceneLights.Count; i++)
        {
            if (sceneLights[i] != null)
            {
                sceneLights[i].enabled = turnOn;
            }
        }

        if (turnOn)
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

    public void ResolveTimerReferences()
    {
        if (timerPanel == null)
        {
            GameObject panel = GameObject.Find("GeneratorTimerPanel");
            if (panel != null)
            {
                timerPanel = panel;
            }
        }

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

        if (timerText != null)
        {
            timerTextColor = timerText.color;
        }
    }

    private void Update()
    {

        if (isRunning)
        {
            currentRunTime -= Time.deltaTime;

            UpdateTimerDisplay();

            if (currentRunTime <= 0f)
            {
                StopGenerator();
            }
        }

        if (inarea)
        {
            UpdateInteractionPrompts();

            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
    }

    public bool HasFuel()
    {
        if (lcan.playerFuelCount > 0) return true;
        if (can_script != null && can_script.isUsed) return true;
        if (MANDATORY != null && !MANDATORY.activeSelf) return true;
        if (GeneratorSystem.Instance != null && GeneratorSystem.Instance.isPlayerHoldingFuel) return true;
        return false;
    }

    public void FuelAndStartGenerator()
    {
        ConsumeFuel();

        isRunning = true;
        currentRunTime = generatorRunDuration;

        if (close_text != null) close_text.SetActive(false);
        if (open_text != null) open_text.SetActive(false);
        if (lev_ui != null) lev_ui.SetActive(false);

        if (lv_animator != null)
        {
            lv_animator.enabled = true;
            lv_animator.Play(0);
        }

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

        SetGeneratorLights(true);

        if (GeneratorSystem.Instance != null)
        {
            GeneratorSystem.Instance.AddFuel(generatorRunDuration);
        }

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

    public void StopGenerator()
    {
        currentRunTime = 0f;
        isRunning = false;

        SetGeneratorLights(false);

        if (grator != null)
        {
            grator.Stop();
        }

        if (stop_sound != null)
        {
            AudioSource.PlayClipAtPoint(stop_sound, transform.position);
        }

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
        if (timerText == null || timerPanel == null)
        {
            ResolveTimerReferences();
        }

        if (isRunning)
        {
            if (timerPanel != null && !timerPanel.activeSelf)
            {
                timerPanel.SetActive(true);
            }
            if (timerText != null && !timerText.gameObject.activeSelf)
            {
                timerText.gameObject.SetActive(true);
            }
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(Mathf.Max(0f, currentRunTime) / 60f);
            int seconds = Mathf.FloorToInt(Mathf.Max(0f, currentRunTime) % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

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

            if (open_text != null) open_text.SetActive(true);
            if (close_text != null) close_text.SetActive(false);
        }
    }

    private void OnGUI()
    {

        if (timerText != null || timerPanel != null) return;
        if (!isRunning || !showOnScreenTimer) return;

        if (timerStyle == null)
        {
            timerStyle = new GUIStyle(GUI.skin.box);
            timerStyle.fontSize = 18;
            timerStyle.fontStyle = FontStyle.Bold;
            timerStyle.alignment = TextAnchor.MiddleCenter;
        }

        int minutes = Mathf.FloorToInt(Mathf.Max(0f, currentRunTime) / 60f);
        int seconds = Mathf.FloorToInt(Mathf.Max(0f, currentRunTime) % 60f);

        if (currentRunTime <= 15f)
        {
            bool flash = Mathf.PingPong(Time.time * 3f, 1f) > 0.5f;
            timerStyle.normal.textColor = flash ? Color.red : Color.yellow;
        }
        else
        {
            timerStyle.normal.textColor = new Color(0.2f, 1f, 0.4f);
        }

        string text = string.Format("⚡ POWER: {0:00}:{1:00}", minutes, seconds);

        float width = 230f;
        float height = 40f;
        float x = (Screen.width - width) / 2f;
        float y = 20f;

        GUI.Box(new Rect(x, y, width, height), text, timerStyle);
    }
}