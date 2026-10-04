using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attach to the StairsTrigger GameObject (must have a BoxCollider set to Is Trigger).
/// When the player steps into the zone for the first time:
///   1. Activates & shows the staircase panel (works with GameObject, CanvasGroup, or GhostHUD).
///   2. Turns off all scene lights (or a custom list).
/// </summary>
public class StairsTrigger : MonoBehaviour
{
    [Header("Staircase Panel Reference")]
    [Tooltip("Drag your staircase panel GameObject here (e.g. GhostPanel, StaircasePanel, etc.)")]
    public GameObject panelObject;

    [Tooltip("Optional: If the panel uses a CanvasGroup for fading, drag it here (auto-found if empty).")]
    public CanvasGroup panelCanvasGroup;

    [Tooltip("Optional: If your panel uses GhostHUD, drag it here (auto-found if empty).")]
    public GhostHUD ghostHUD;

    [Header("Display Settings")]
    [Tooltip("If true, does a smooth fade-in. If false, turns it on instantly.")]
    public bool useFadeIn = true;
    public float fadeInDuration = 1.0f;

    [Tooltip("Press this key to dismiss/close the panel (default: E). Set to None to disable.")]
    public KeyCode dismissKey = KeyCode.E;

    [Header("Lights")]
    [Tooltip("Leave empty to automatically turn off every Light in the scene.")]
    public Light[] lightsToTurnOff;
    [Tooltip("Delay in seconds before lights shut off.")]
    public float lightCutDelay = 0.2f;

    [Header("Audio (Optional)")]
    public AudioSource triggerAudio;
    public AudioClip lightOffSound;

    private bool hasTriggered = false;
    private bool isPanelShowing = false;

    private void Awake()
    {
        // Auto-resolve components from panelObject if provided
        if (panelObject != null)
        {
            if (panelCanvasGroup == null)
                panelCanvasGroup = panelObject.GetComponent<CanvasGroup>();
            if (ghostHUD == null)
                ghostHUD = panelObject.GetComponent<GhostHUD>();
        }
        else if (panelCanvasGroup != null)
        {
            panelObject = panelCanvasGroup.gameObject;
        }
        else if (ghostHUD != null)
        {
            panelObject = ghostHUD.gameObject;
        }

        // Initially ensure the panel is hidden if configured to be
        if (ghostHUD == null && panelObject != null && !panelObject.activeSelf)
        {
            // Keep it inactive until triggered
        }
    }

    private void Update()
    {
        if (isPanelShowing && dismissKey != KeyCode.None && Input.GetKeyDown(dismissKey))
        {
            ClosePanel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        Debug.Log("[StairsTrigger] Player entered stairs trigger zone!");

        // 1. Show the panel
        ShowPanel();

        // 2. Shut off the lights
        StartCoroutine(TurnOffLightsCoroutine(lightCutDelay));

        // Disable collider so it doesn't trigger again
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    public void ShowPanel()
    {
        isPanelShowing = true;

        // If it's a GhostHUD popup, use its built-in open sequence
        if (ghostHUD != null)
        {
            Debug.Log("[StairsTrigger] Triggering GhostHUD on panel.");
            if (panelObject != null && !panelObject.activeSelf)
                panelObject.SetActive(true);
            ghostHUD.TriggerOpen();
            return;
        }

        // Standard GameObject / CanvasGroup panel
        if (panelObject != null)
        {
            panelObject.SetActive(true);
            Debug.Log($"[StairsTrigger] SetActive(true) on panel: {panelObject.name}");

            // Ensure parent canvases/hierarchy are active
            Transform curr = panelObject.transform.parent;
            while (curr != null)
            {
                if (!curr.gameObject.activeSelf)
                {
                    curr.gameObject.SetActive(true);
                    Debug.Log($"[StairsTrigger] Activated parent: {curr.gameObject.name}");
                }
                curr = curr.parent;
            }

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.interactable = true;
                panelCanvasGroup.blocksRaycasts = true;

                if (useFadeIn)
                {
                    StartCoroutine(FadeInCanvasGroup(panelCanvasGroup, fadeInDuration));
                }
                else
                {
                    panelCanvasGroup.alpha = 1f;
                }
            }
            else
            {
                // Ensure text and graphics are not hidden with 0 alpha
                TextMeshProUGUI[] texts = panelObject.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    Color c = t.color;
                    if (c.a < 0.05f) { c.a = 1f; t.color = c; }
                }
            }
        }
        else
        {
            Debug.LogWarning("[StairsTrigger] No Panel Object or CanvasGroup assigned in inspector!");
        }
    }

    public void ClosePanel()
    {
        if (!isPanelShowing) return;
        isPanelShowing = false;

        if (panelCanvasGroup != null && useFadeIn)
        {
            StartCoroutine(FadeOutAndDisable(panelCanvasGroup, 0.5f));
        }
        else if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
    }

    private IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
    {
        float elapsed = 0f;
        cg.alpha = 0f;
        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator FadeOutAndDisable(CanvasGroup cg, float duration)
    {
        float elapsed = 0f;
        float startAlpha = cg.alpha;
        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        if (panelObject != null) panelObject.SetActive(false);
    }

    private IEnumerator TurnOffLightsCoroutine(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (triggerAudio != null && lightOffSound != null)
        {
            triggerAudio.PlayOneShot(lightOffSound);
        }

        Light[] targets = (lightsToTurnOff != null && lightsToTurnOff.Length > 0)
            ? lightsToTurnOff
            : FindObjectsByType<Light>(FindObjectsSortMode.None);

        int count = 0;
        foreach (Light l in targets)
        {
            if (l != null && l.enabled)
            {
                // Do not disable flashlight attached to player camera unless explicitly assigned
                if (lightsToTurnOff == null || lightsToTurnOff.Length == 0)
                {
                    if (l.CompareTag("MainCamera") || l.transform.IsChildOf(Camera.main != null ? Camera.main.transform : l.transform.root))
                    {
                        continue;
                    }
                }
                l.enabled = false;
                count++;
            }
        }

        Debug.Log($"[StairsTrigger] Cut off {count} light(s).");
    }
}
