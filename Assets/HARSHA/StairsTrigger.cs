using UnityEngine;
using System.Collections;

/// <summary>
/// Attach to the StairsTrigger GameObject (must have a BoxCollider set to Is Trigger).
/// When the player steps into the zone for the first time:
///   1. Shows the staircase UI panel (via CanvasGroup fade-in).
///   2. Turns off all lights in the scene.
/// Press Enter / [E] to dismiss the panel.
/// </summary>
public class StairsTrigger : MonoBehaviour
{
    // ─── Inspector Fields ──────────────────────────────────────────────────────

    [Header("Staircase Panel")]
    [Tooltip("Drag the CanvasGroup on your staircase panel UI here.")]
    public CanvasGroup staircasePanel;

    [Tooltip("How long the panel fades in (seconds).")]
    public float panelFadeInDuration = 1.5f;

    [Tooltip("Key the player presses to dismiss the panel. Default: E")]
    public KeyCode dismissKey = KeyCode.E;

    [Header("Lights")]
    [Tooltip("Leave empty to automatically find every Light in the scene. Or drag specific ones here.")]
    public Light[] lightsToTurnOff;

    [Tooltip("Delay (seconds) before the lights cut out after the trigger fires.")]
    public float lightCutDelay = 0.5f;

    // ─── Private State ─────────────────────────────────────────────────────────

    private bool hasTriggered = false;
    private bool panelOpen    = false;

    // ─── Unity Messages ────────────────────────────────────────────────────────

    private void Start()
    {
        // Hide panel at start
        if (staircasePanel != null)
        {
            staircasePanel.alpha          = 0f;
            staircasePanel.interactable   = false;
            staircasePanel.blocksRaycasts = false;
            staircasePanel.gameObject.SetActive(true); // keep active so coroutines work
        }
    }

    private void Update()
    {
        // Let player dismiss the panel with the chosen key
        if (panelOpen && Input.GetKeyDown(dismissKey))
        {
            StartCoroutine(FadeOutPanel());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;

        // 1. Show staircase panel
        if (staircasePanel != null)
            StartCoroutine(FadeInPanel());

        // 2. Turn off lights (with optional delay)
        StartCoroutine(TurnOffLightsAfterDelay(lightCutDelay));

        // Disable this collider so OnTriggerEnter never fires again
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    // ─── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator FadeInPanel()
    {
        panelOpen = true;
        staircasePanel.interactable   = true;
        staircasePanel.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < panelFadeInDuration)
        {
            staircasePanel.alpha = Mathf.Lerp(0f, 1f, elapsed / panelFadeInDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        staircasePanel.alpha = 1f;
    }

    private IEnumerator FadeOutPanel()
    {
        panelOpen = false;

        float elapsed = 0f;
        float fadeOut = 0.8f;
        float startAlpha = staircasePanel.alpha;

        while (elapsed < fadeOut)
        {
            staircasePanel.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOut);
            elapsed += Time.deltaTime;
            yield return null;
        }

        staircasePanel.alpha          = 0f;
        staircasePanel.interactable   = false;
        staircasePanel.blocksRaycasts = false;
    }

    private IEnumerator TurnOffLightsAfterDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // Use the manually-assigned list if provided, otherwise grab every Light
        Light[] targets = (lightsToTurnOff != null && lightsToTurnOff.Length > 0)
            ? lightsToTurnOff
            : FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (Light l in targets)
        {
            if (l != null)
                l.enabled = false;
        }

        Debug.Log($"[StairsTrigger] Turned off {targets.Length} light(s).");
    }
}
