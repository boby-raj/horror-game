using System.Collections;
using UnityEngine;

public class OpenDrawer : MonoBehaviour, IInteractable
{
    [Header("DRAWER ANIMATION & MOVEMENT")]
    [Tooltip("Animator with 'open' and 'close' bools. Auto-found if left empty.")]
    public Animator ANI;
    [Tooltip("If no Animator is assigned, drawer will slide physically by this offset")]
    public Vector3 slideDirection = new Vector3(0, 0, -0.45f);
    public float slideSpeed = 3.0f;

    [Header("AUDIO")]
    public AudioSource opensound;
    public AudioSource closesound;
    public AudioSource commmonAudio;

    [Header("UI PANELS / PROMPTS (OPTIONAL)")]
    public GameObject opentext;
    public GameObject closetext;

    [Header("OUTLINE / HIGHLIGHT EFFECT")]
    public bool highlightOnHover = false;
    public Color hoverColor = Color.yellow;

    private bool open = false;
    private Renderer[] renderers;
    private Color[] originalColors;
    private Vector3 closedLocalPosition;
    private Vector3 openLocalPosition;
    private Coroutine slideCoroutine;

    private static GameObject cachedInteractUI;

    void Awake()
    {
        // 1. Auto-resolve Animator: prefer Animator on self, then child/parent
        if (ANI == null || ANI.gameObject != gameObject)
        {
            Animator selfAnim = GetComponent<Animator>();
            if (selfAnim != null) ANI = selfAnim;
            else if (ANI == null)
            {
                ANI = GetComponentInChildren<Animator>();
                if (ANI == null) ANI = GetComponentInParent<Animator>();
            }
        }

        // 2. Auto-resolve AudioSource on self if missing
        AudioSource selfAudio = GetComponent<AudioSource>();
        if (opensound == null && selfAudio != null) opensound = selfAudio;
        if (closesound == null && selfAudio != null) closesound = selfAudio;
        if (commmonAudio == null && selfAudio != null) commmonAudio = selfAudio;

        // 3. Cache open and closed positions for fallback slide
        closedLocalPosition = transform.localPosition;
        openLocalPosition = closedLocalPosition + slideDirection;

        // 4. Cache UI prompt if assigned
        if (opentext != null && cachedInteractUI == null)
        {
            cachedInteractUI = opentext;
        }
    }

    void Start()
    {
        // Auto-find Drawer_interact UI prompt if unassigned
        if (opentext == null)
        {
            if (cachedInteractUI != null)
            {
                opentext = cachedInteractUI;
            }
            else
            {
                GameObject found = GameObject.Find("Drawer_interact");
                if (found != null)
                {
                    cachedInteractUI = found;
                    opentext = found;
                }
                else
                {
                    foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    {
                        Transform t = canvas.transform.Find("Drawer_interact");
                        if (t != null)
                        {
                            opentext = t.gameObject;
                            cachedInteractUI = opentext;
                            break;
                        }
                    }
                }
            }
        }

        if (closetext == null)
        {
            closetext = opentext;
        }

        // Hide UI prompts at startup
        if (opentext != null) opentext.SetActive(false);
        if (closetext != null) closetext.SetActive(false);

        // Reset Animator parameters
        if (ANI != null && ANI.runtimeAnimatorController != null)
        {
            ANI.SetBool("open", false);
            ANI.SetBool("close", false);
        }

        // Cache renderers and base colors if hover highlight is used
        if (highlightOnHover)
        {
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
        }
    }

    public void OnHoverEnter()
    {
        if (highlightOnHover && renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null && r.material != null)
                {
                    if (r.material.HasProperty("_BaseColor"))
                        r.material.SetColor("_BaseColor", hoverColor);
                    else if (r.material.HasProperty("_Color"))
                        r.material.color = hoverColor;
                }
            }
        }

        if (!open && opentext != null) opentext.SetActive(true);
        else if (open && closetext != null) closetext.SetActive(true);
    }

    public void OnHoverExit()
    {
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

        if (opentext != null) opentext.SetActive(false);
        if (closetext != null) closetext.SetActive(false);
    }

    public void Interact()
    {
        open = !open;

        PlayAudio();

        if (open)
        {
            // OPEN
            if (ANI != null && ANI.runtimeAnimatorController != null)
            {
                ANI.SetBool("open", true);
                ANI.SetBool("close", false);
            }
            else
            {
                if (slideCoroutine != null) StopCoroutine(slideCoroutine);
                slideCoroutine = StartCoroutine(SlideToPosition(openLocalPosition));
            }

            if (opentext != null) opentext.SetActive(false);
            if (closetext != null) closetext.SetActive(true);
        }
        else
        {
            // CLOSE
            if (ANI != null && ANI.runtimeAnimatorController != null)
            {
                ANI.SetBool("open", false);
                ANI.SetBool("close", true);
            }
            else
            {
                if (slideCoroutine != null) StopCoroutine(slideCoroutine);
                slideCoroutine = StartCoroutine(SlideToPosition(closedLocalPosition));
            }

            if (closetext != null) closetext.SetActive(false);
            if (opentext != null) opentext.SetActive(true);
        }
    }

    private void PlayAudio()
    {
        if (opensound != null) opensound.Play();
        else if (closesound != null) closesound.Play();
        else if (commmonAudio != null) commmonAudio.Play();
    }

    private IEnumerator SlideToPosition(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.localPosition, targetPos) > 0.005f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * slideSpeed);
            yield return null;
        }
        transform.localPosition = targetPos;
    }
}