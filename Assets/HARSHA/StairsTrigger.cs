using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

        if (ghostHUD == null && panelObject != null)
        {
            ghostHUD = panelObject.GetComponentInChildren<GhostHUD>(true);
        }

        if (ghostHUD == null)
        {
            ghostHUD = FindFirstObjectByType<GhostHUD>(FindObjectsInactive.Include);
        }

        if (generatorLever == null)
        {
            generatorLever = FindFirstObjectByType<lever_script>(FindObjectsInactive.Include);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        Debug.Log("[StairsTrigger] Player reached stairs trigger zone!");

        StartCoroutine(ExecuteTriggerSequence());

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    private IEnumerator ExecuteTriggerSequence()
    {

        if (shutoffDelay > 0f)
        {
            yield return new WaitForSeconds(shutoffDelay);
        }

        if (triggerAudio != null && powerCutSound != null)
        {
            triggerAudio.PlayOneShot(powerCutSound);
        }

        if (generatorLever != null)
        {
            generatorLever.StopGenerator();
            Debug.Log("[StairsTrigger] Generator stopped via lever_script.StopGenerator().");
        }

        if (allLightsParent != null)
        {
            allLightsParent.SetActive(false);
            Debug.Log($"[StairsTrigger] Disabled lights parent: {allLightsParent.name}");
        }

        Light[] lights = (specificLightsToTurnOff != null && specificLightsToTurnOff.Length > 0)
            ? specificLightsToTurnOff
            : FindObjectsByType<Light>(FindObjectsSortMode.None);

        int lightsCut = 0;
        foreach (Light l in lights)
        {
            if (l != null && l.enabled)
            {

                if (l.CompareTag("MainCamera") || l.transform.IsChildOf(Camera.main != null ? Camera.main.transform : l.transform.root))
                {
                    continue;
                }

                l.enabled = false;
                lightsCut++;
            }
        }
        Debug.Log($"[StairsTrigger] Extinguished {lightsCut} scene light(s).");

        if (ghostHUD != null)
        {

            GameObject hudGO = ghostHUD.gameObject;
            if (!hudGO.activeSelf) hudGO.SetActive(true);

            Transform curr = hudGO.transform.parent;
            while (curr != null)
            {
                if (!curr.gameObject.activeSelf) curr.gameObject.SetActive(true);
                curr = curr.parent;
            }

            if (!string.IsNullOrEmpty(overrideHeading) && ghostHUD.headingText != null)
            {
                ghostHUD.headingText.text = overrideHeading;
            }
            if (!string.IsNullOrEmpty(overrideBody) && ghostHUD.bodyText != null)
            {
                ghostHUD.bodyText.text = overrideBody;
            }

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
