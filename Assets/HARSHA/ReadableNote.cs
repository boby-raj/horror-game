using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach to any 3D paper mesh, quad, or book in the scene.
/// Implements IInteractable so PlayerInteraction raycast automatically detects it.
/// Supports both inline text or external NoteData ScriptableObject assets.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ReadableNote : MonoBehaviour, IInteractable
{
    [Header("Note Data (Optional ScriptableObject)")]
    [Tooltip("Optional NoteData asset. If assigned, overrides inline text.")]
    public NoteData noteData;

    [Header("Inline Content (If No Asset Used)")]
    [Tooltip("Title shown at the top of the note")]
    public string noteTitle = "Scrap of Paper";
    [Tooltip("The text written on this specific paper in the scene")]
    [TextArea(5, 15)]
    public string messageOnPaper = "They won't stop screaming behind the walls...";

    [Header("Interaction & UI Prompt")]
    [Tooltip("Hover UI prompt (e.g. '[E]'). Auto-finds universal '[E]' if empty.")]
    public GameObject hoverText;

    [Header("Visual Hover Highlight")]
    public bool highlightOnHover = true;
    public Color hoverHighlightColor = new Color(1f, 0.92f, 0.4f);

    [Header("Audio")]
    public AudioClip pickupRustleSound;
    [Tooltip("Max seconds the rustle sound plays before being cut off")]
    public float rustleDuration = 1f;
    [Tooltip("Seconds at the end of the duration used to fade the sound out")]
    public float rustleFadeOutTime = 0.2f;
    [Range(0f, 1f)] public float rustleVolume = 1f;

    [Header("Natural Floor Rotation")]
    [Tooltip("Slightly randomizes Y rotation on start so papers look naturally placed")]
    public bool randomizeRotationOnAwake = false;

    [Header("Events")]
    [Tooltip("Triggers once when this note is read for the first time (can trigger jumpscares, sounds, or open doors)")]
    public UnityEvent onFirstRead;

    private bool hasBeenRead = false;
    private Renderer[] renderers;
    private Color[] originalColors;

    private void Awake()
    {
        // Robust collision for quads and thin meshes:
        // Quads only have a single-sided polygon that fails raycasts from backfaces or shallow angles.
        // A BoxCollider with small depth guarantees reliable hit detection from any angle.
        MeshCollider meshCol = GetComponent<MeshCollider>();
        if (meshCol != null && (!meshCol.convex || (meshCol.sharedMesh != null && meshCol.sharedMesh.name.ToLower().Contains("quad"))))
        {
            Destroy(meshCol);
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.size = new Vector3(1f, 1f, 0.3f);
        }
        else if (GetComponent<Collider>() == null)
        {
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.size = new Vector3(1f, 1f, 0.3f);
        }

        if (randomizeRotationOnAwake)
        {
            transform.Rotate(Vector3.up, Random.Range(-25f, 25f), Space.Self);
        }

        renderers = GetComponentsInChildren<Renderer>();
        if (renderers != null && renderers.Length > 0)
        {
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material != null)
                {
                    if (renderers[i].material.HasProperty("_BaseColor"))
                        originalColors[i] = renderers[i].material.GetColor("_BaseColor");
                    else if (renderers[i].material.HasProperty("_Color"))
                        originalColors[i] = renderers[i].material.color;
                }
            }
        }

        // Find or auto-create prompt if not assigned in Inspector
        if (hoverText == null)
        {
            hoverText = FindUniversalPrompt();
            if (hoverText == null) hoverText = CreateFallbackPrompt();
        }
    }

    private void Start()
    {
        // Ensure prompt starts hidden
        if (hoverText != null)
        {
            hoverText.SetActive(false);
        }
    }

    public void OnHoverEnter()
    {
        if (NoteManager.Instance != null && NoteManager.Instance.IsReading) return;

        Debug.Log($"[ReadableNote] Looking at note: '{noteTitle}'");

        if (hoverText == null)
        {
            hoverText = FindUniversalPrompt();
            if (hoverText == null) hoverText = CreateFallbackPrompt();
        }

        if (hoverText != null)
        {
            hoverText.SetActive(true);
        }

        if (highlightOnHover && renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null && r.material != null)
                {
                    if (r.material.HasProperty("_BaseColor"))
                        r.material.SetColor("_BaseColor", hoverHighlightColor);
                    else if (r.material.HasProperty("_Color"))
                        r.material.color = hoverHighlightColor;
                }
            }
        }
    }

    private static GameObject cachedFallbackPrompt;

    private static GameObject FindUniversalPrompt()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas c in canvases)
        {
            if (c.name.Contains("NoteReadingCanvas")) continue;

            Transform[] transforms = c.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                string n = t.name.Trim().ToLower();
                if (n == "[e]" || n == "'[e]'" || n == "press e to open" || n == "open_text" || n == "interact" || n == "escreen")
                {
                    return t.gameObject;
                }
            }
        }
        return null;
    }

    private static GameObject CreateFallbackPrompt()
    {
        if (cachedFallbackPrompt != null) return cachedFallbackPrompt;

        // Find HUD canvas
        Canvas hudCanvas = null;
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas c in canvases)
        {
            if (!c.name.Contains("NoteReadingCanvas") && c.renderMode != RenderMode.WorldSpace)
            {
                hudCanvas = c;
                break;
            }
        }

        if (hudCanvas == null) return null;

        GameObject promptObj = new GameObject("[E]");
        promptObj.transform.SetParent(hudCanvas.transform, false);

        RectTransform rt = promptObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -45f);
        rt.sizeDelta = new Vector2(200f, 40f);

        TMPro.TextMeshProUGUI tmp = promptObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "[E] Read";
        tmp.fontSize = 22;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 1f, 1f, 0.95f);
        tmp.raycastTarget = false;

        promptObj.SetActive(false);
        cachedFallbackPrompt = promptObj;
        return promptObj;
    }

    public void OnHoverExit()
    {
        if (hoverText != null) hoverText.SetActive(false);

        if (highlightOnHover && renderers != null && originalColors != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    if (renderers[i].material.HasProperty("_BaseColor"))
                        renderers[i].material.SetColor("_BaseColor", originalColors[i]);
                    else if (renderers[i].material.HasProperty("_Color"))
                        renderers[i].material.color = originalColors[i];
                }
            }
        }
    }

    public void Interact()
    {
        Debug.Log($"[ReadableNote] Interacted with note: '{noteTitle}'");
        if (hoverText != null) hoverText.SetActive(false);

        // Turn off highlight while reading
        OnHoverExit();

        if (pickupRustleSound != null)
        {
            PlayRustle();
        }

        if (NoteManager.Instance != null)
        {
            NoteManager.Instance.ShowNote(
                messageOnPaper,
                noteTitle,
                noteData,
                onCloseCallback: () =>
                {
                    if (!hasBeenRead)
                    {
                        hasBeenRead = true;
                        onFirstRead?.Invoke();
                    }
                }
            );
        }
        else
        {
            Debug.LogError("[ReadableNote] NoteManager.Instance is missing in the scene! Please add a NoteManager to a Canvas in your scene.");
        }
    }

    /// <summary>
    /// Plays the rustle sound from a temporary AudioSource that is cut off
    /// after rustleDuration seconds, with a short fade-out at the end.
    /// </summary>
    private void PlayRustle()
    {
        GameObject temp = new GameObject("RustleSound_Temp");
        temp.transform.position = transform.position;

        AudioSource src = temp.AddComponent<AudioSource>();
        src.clip = pickupRustleSound;
        src.volume = rustleVolume;
        src.spatialBlend = 1f; // 3D, like PlayClipAtPoint
        src.Play();

        // Never play longer than the clip itself
        float playTime = Mathf.Min(rustleDuration, pickupRustleSound.length);

        RustleFader fader = temp.AddComponent<RustleFader>();
        fader.Init(src, playTime, rustleFadeOutTime);
    }

    // Lives on the temp object, so it keeps working even if the note is disabled/destroyed
    private class RustleFader : MonoBehaviour
    {
        private AudioSource src;
        private float duration, fadeTime, timer, startVolume;

        public void Init(AudioSource source, float playDuration, float fadeOut)
        {
            src = source;
            duration = playDuration;
            fadeTime = Mathf.Clamp(fadeOut, 0f, playDuration);
            startVolume = source.volume;
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;

            float fadeStart = duration - fadeTime;
            if (fadeTime > 0f && timer > fadeStart)
                src.volume = Mathf.Lerp(startVolume, 0f, (timer - fadeStart) / fadeTime);

            if (timer >= duration)
            {
                src.Stop();
                Destroy(gameObject);
            }
        }
    }
}