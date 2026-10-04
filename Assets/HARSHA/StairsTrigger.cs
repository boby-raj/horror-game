using System.Collections;
using System.Collections.Generic; // Added for List compatibility
using UnityEngine;
using TMPro;

/// <summary>
/// Attach to the StairsTrigger GameObject (must have a BoxCollider set to Is Trigger).
/// Sequence on player contact:
///   1. Stops the generator (lever_script.StopGenerator()).
///   2. Turns off all lights in the scene (including pointlightwork).
///   3. Triggers the GhostHUD / staircase panel (same as the first door trigger).
/// </summary>
public class StairsTrigger : MonoBehaviour
{
    [Header("GhostHUD / Staircase Panel Reference")]
    [Tooltip("Drag the GhostHUD component from your panel (or drag GhostPanel GameObject).")]
    public GhostHUD ghostHUD;

    [Tooltip("Alternative panel GameObject if not using GhostHUD.")]
    public GameObject panelObject;

    [Header("Generator Reference (Optional - Auto-Found if Empty)")]
    [Tooltip("The lever_script running the generator. If empty, automatically found.")]
    public lever_script generatorLever;

    [Header("Lights Control")]
    [Tooltip("Specific lights or parent GameObject to disable (e.g. pointlightwork).")]
    public GameObject allLightsParent;

    [Tooltip("Leave empty to automatically turn off every Light component in the scene.")]
    public Light[] specificLightsToTurnOff;

    [Tooltip("Delay in seconds before lights and generator cut out after entering trigger.")]
    public float shutoffDelay = 0.2f;

    [Header("Custom Message Override (Optional)")]
    [Tooltip("If set, changes the text on the GhostHUD popup for the stairs event.")]
    public string overrideHeading = "";
    [TextArea(2, 5)]
    public string overrideBody = "";

    [Header("Audio (Optional)")]
    public AudioSource triggerAudio;
    public AudioClip powerCutSound;

    private bool hasTriggered = false;

    private void Awake()
    {
        // 1. Auto-find GhostHUD if not manually linked
        if (ghostHUD == null && panelObject != null)
        {
            ghostHUD = panelObject.GetComponentInChildren<GhostHUD>(true);
        }

        if (ghostHUD == null)
        {
            ghostHUD = FindFirstObjectByType<GhostHUD>(FindObjectsInactive.Include);
        }

        // 2. Auto-find Generator if not manually linked
        if (generatorLever == null)
        {
            generatorLever = FindFirstObjectByType<lever_script>(FindObjectsInactive.Include);
        }

        // 3. FIX: We cannot assign a List<GameObject> directly to a single GameObject variable.
        // We leave allLightsParent untouched here. If you want a specific parent disabled, link it in the Unity Inspector.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        Debug.Log("[StairsTrigger] Player reached stairs trigger zone!");

        StartCoroutine(ExecuteTriggerSequence());

        // Disable collider so trigger never fires twice
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    private IEnumerator ExecuteTriggerSequence()
    {
        // 1. Optional delay before the power cuts
        if (shutoffDelay > 0f)
        {
            yield return new WaitForSeconds(shutoffDelay);
        }

        // 2. Sound effect
        if (triggerAudio != null && powerCutSound != null)
        {
            triggerAudio.PlayOneShot(powerCutSound);
        }

        // 3. SHUT DOWN GENERATOR (This automatically turns off lever_script's alllights)
        if (generatorLever != null)
        {
            generatorLever.StopGenerator();
            Debug.Log("[StairsTrigger] Generator stopped via lever_script.StopGenerator().");
        }

        // 4. SHUT OFF ALL LIGHTS
        if (allLightsParent != null)
        {
            allLightsParent.SetActive(false);
            Debug.Log($"[StairsTrigger] Disabled lights parent: {allLightsParent.name}");
        }

        // Turn off all scene lights or specific lights
        Light[] lights = (specificLightsToTurnOff != null && specificLightsToTurnOff.Length > 0)
            ? specificLightsToTurnOff
            : FindObjectsByType<Light>(FindObjectsSortMode.None);

        int lightsCut = 0;
        foreach (Light l in lights)
        {
            if (l != null && l.enabled)
            {
                // Preserve player flashlight if present
                if (l.CompareTag("MainCamera") || l.transform.IsChildOf(Camera.main != null ? Camera.main.transform : l.transform.root))
                {
                    continue;
                }

                l.enabled = false;
                lightsCut++;
            }
        }
        Debug.Log($"[StairsTrigger] Extinguished {lightsCut} scene light(s).");

        // 5. TRIGGER GHOSTHUD / STAIRCASE PANEL
        if (ghostHUD != null)
        {
            // Ensure its GameObject and parent canvases are active
            GameObject hudGO = ghostHUD.gameObject;
            if (!hudGO.activeSelf) hudGO.SetActive(true);

            Transform curr = hudGO.transform.parent;
            while (curr != null)
            {
                if (!curr.gameObject.activeSelf) curr.gameObject.SetActive(true);
                curr = curr.parent;
            }

            // Optional custom text
            if (!string.IsNullOrEmpty(overrideHeading) && ghostHUD.headingText != null)
            {
                ghostHUD.headingText.text = overrideHeading;
            }
            if (!string.IsNullOrEmpty(overrideBody) && ghostHUD.bodyText != null)
            {
                ghostHUD.bodyText.text = overrideBody;
            }

            // Trigger identical to first door
            ghostHUD.TriggerOpen();
            Debug.Log("[StairsTrigger] GhostHUD.TriggerOpen() successfully called!");
        }
        else if (panelObject != null)
        {
            panelObject.SetActive(true);
            CanvasGroup cg = panelObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            Debug.Log($"[StairsTrigger] Activated panelObject: {panelObject.name}");
        }
        else
        {
            Debug.LogWarning("[StairsTrigger] Neither GhostHUD nor panelObject was found to display!");
        }
    }
}
