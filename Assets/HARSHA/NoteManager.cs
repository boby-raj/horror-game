using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central UI Manager for reading notes and papers in the game.
/// Handles opening/closing animations, pausing the game, unlocking/relocking the mouse,
/// and disabling player camera and movement controls while reading.
/// </summary>
public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    [Header("UI Canvas & Panels")]
    [Tooltip("The root Canvas or panel for note reading. Auto-toggled on/off.")]
    public GameObject noteCanvas;
    [Tooltip("CanvasGroup for smooth fade-in / fade-out animations.")]
    public CanvasGroup canvasGroup;
    [Tooltip("The central paper transform for scale-up pop effect.")]
    public RectTransform paperRect;

    [Header("Text & Display Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public Image paperImage;
    [Tooltip("Prompt showing 'Press E or Esc to close'")]
    public GameObject closePromptText;
    [Tooltip("Optional clickable Close button on the paper")]
    public Button closeButton;
    [Tooltip("Optional background dimmer button to close when clicking outside the paper")]
    public Button backgroundDimmerButton;

    [Header("Reading Settings")]
    public bool pauseGameWhileReading = true;
    public bool useOpenAnimation = true;
    public float animationDuration = 0.22f;

    [Header("Default Audio")]
    public AudioSource audioSource;
    public AudioClip defaultOpenSound;
    public AudioClip defaultCloseSound;

    [Header("Player Controller References (Auto-Found if Empty)")]
    public MonoBehaviour playerMovement;
    public MonoBehaviour cameraLook;
    public MonoBehaviour playerInteraction;

    public bool IsReading { get; private set; } = false;
    public bool JustClosedThisFrame => Time.frameCount == justClosedFrame;
    private int justClosedFrame = -1;

    private Coroutine typewriterCoroutine;
    private Coroutine animationCoroutine;
    private Sprite defaultPaperSprite;
    private Color defaultPaperColor = Color.white;
    private TMP_FontAsset defaultFont;
    private float defaultFontSize = 28f;
    private System.Action pendingOnCloseCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (paperImage != null)
        {
            defaultPaperSprite = paperImage.sprite;
            defaultPaperColor = paperImage.color;
        }

        if (bodyText != null)
        {
            defaultFont = bodyText.font;
            defaultFontSize = bodyText.fontSize;
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseNote);
        }

        if (backgroundDimmerButton == null && noteCanvas != null)
        {
            Transform dimT = noteCanvas.transform.Find("BackgroundDimmer");
            if (dimT != null)
            {
                backgroundDimmerButton = dimT.GetComponent<Button>();
                if (backgroundDimmerButton == null)
                {
                    backgroundDimmerButton = dimT.gameObject.AddComponent<Button>();
                    backgroundDimmerButton.transition = Selectable.Transition.None;
                }
            }
        }

        if (backgroundDimmerButton != null)
        {
            backgroundDimmerButton.onClick.RemoveAllListeners();
            backgroundDimmerButton.onClick.AddListener(CloseNote);
        }
    }

    private void Start()
    {
        FindPlayerReferences();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (paperRect != null) paperRect.gameObject.SetActive(false);
        if (closePromptText != null) closePromptText.SetActive(false);

        if (noteCanvas != null && noteCanvas != gameObject)
        {
            noteCanvas.SetActive(false);
        }
    }

    private void FindPlayerReferences()
    {
        if (playerMovement == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent("FirstPersonMovement") as MonoBehaviour;
                if (playerMovement == null) playerMovement = player.GetComponent("jump") as MonoBehaviour;
                if (playerInteraction == null) playerInteraction = player.GetComponentInChildren<PlayerInteraction>();
            }
        }

        if (cameraLook == null)
        {
            mouselook ml = FindFirstObjectByType<mouselook>();
            if (ml != null) cameraLook = ml;
            else if (Camera.main != null)
            {
                cameraLook = Camera.main.GetComponent("mouselook") as MonoBehaviour;
            }
        }

        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }
    }

    private void Update()
    {
        if (!IsReading) return;

        // Close note on E, Escape, Space, or Return
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            CloseNote();
        }
    }

    /// <summary>
    /// Opens the note UI with custom content or from a NoteData asset.
    /// </summary>
    public void ShowNote(string content, string title = "", NoteData data = null, System.Action onCloseCallback = null)
    {
        if (IsReading) return;

        IsReading = true;
        pendingOnCloseCallback = onCloseCallback;
        FindPlayerReferences();

        // 1. Lock player movement and unlock cursor
        LockPlayerControls(true);

        // 2. Setup text and visual styling
        string finalTitle = title;
        string finalBody = content;
        Sprite finalPaper = defaultPaperSprite;
        Color finalColor = defaultPaperColor;
        TMP_FontAsset finalFont = defaultFont;
        float finalFontSize = defaultFontSize;
        AudioClip openSfx = defaultOpenSound;
        bool typewriter = false;
        float twSpeed = 35f;

        if (data != null)
        {
            if (!string.IsNullOrEmpty(data.noteTitle)) finalTitle = data.noteTitle;
            if (!string.IsNullOrEmpty(data.noteContent)) finalBody = data.noteContent;
            if (data.paperSprite != null) finalPaper = data.paperSprite;
            if (data.customFont != null) finalFont = data.customFont;
            if (data.fontSize > 0) finalFontSize = data.fontSize;
            if (data.openSound != null) openSfx = data.openSound;
            finalColor = data.paperColor;
            typewriter = data.useTypewriterEffect;
            twSpeed = data.typewriterSpeed;
        }

        if (titleText != null)
        {
            titleText.text = finalTitle;
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(finalTitle));
        }

        if (bodyText != null)
        {
            bodyText.font = finalFont;
            bodyText.fontSize = finalFontSize;
            if (typewriter)
            {
                bodyText.text = "";
                if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = StartCoroutine(TypewriterReveal(finalBody, twSpeed));
            }
            else
            {
                bodyText.text = finalBody;
            }
        }

        if (paperImage != null)
        {
            paperImage.sprite = finalPaper;
            paperImage.color = finalColor;
        }

        if (closePromptText != null) closePromptText.SetActive(true);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseNote);
            closeButton.gameObject.SetActive(true);
            closeButton.interactable = true;
        }

        if (backgroundDimmerButton != null)
        {
            backgroundDimmerButton.onClick.RemoveAllListeners();
            backgroundDimmerButton.onClick.AddListener(CloseNote);
            backgroundDimmerButton.interactable = true;
        }

        // 3. Play audio
        if (audioSource != null && openSfx != null)
        {
            audioSource.PlayOneShot(openSfx);
        }

        // 4. Show Canvas and animate
        if (noteCanvas != null) noteCanvas.SetActive(true);
        if (paperRect != null) paperRect.gameObject.SetActive(true);

        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        if (useOpenAnimation)
        {
            animationCoroutine = StartCoroutine(AnimateOpen());
        }
        else
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            if (paperRect != null) paperRect.localScale = Vector3.one;
        }

        // 5. Optionally pause time
        if (pauseGameWhileReading)
        {
            Time.timeScale = 0f;
        }
    }

    public void CloseNote()
    {
        if (!IsReading) return;

        IsReading = false;
        justClosedFrame = Time.frameCount;

        // Restore game time first
        if (pauseGameWhileReading)
        {
            Time.timeScale = 1f;
        }

        // Play close sound
        if (audioSource != null && defaultCloseSound != null)
        {
            audioSource.PlayOneShot(defaultCloseSound);
        }

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        if (useOpenAnimation)
        {
            animationCoroutine = StartCoroutine(AnimateClose());
        }
        else
        {
            FinishClosing();
        }
    }

    private void FinishClosing()
    {
        // 1. Immediately restore player controls and lock cursor FIRST
        LockPlayerControls(false);

        // 2. Clear canvas group interactability
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 3. Trigger callback if any (e.g. jump scare or quest update)
        pendingOnCloseCallback?.Invoke();
        pendingOnCloseCallback = null;

        // 4. Disable visual panel
        if (paperRect != null) paperRect.gameObject.SetActive(false);
        if (closePromptText != null) closePromptText.SetActive(false);

        // Only deactivate noteCanvas if it is NOT this NoteManager itself
        if (noteCanvas != null && noteCanvas != gameObject)
        {
            noteCanvas.SetActive(false);
        }
    }

    private void LockPlayerControls(bool locked)
    {
        FindPlayerReferences();

        if (locked)
        {
            if (playerMovement != null) playerMovement.enabled = false;
            if (cameraLook != null) cameraLook.enabled = false;
            if (playerInteraction != null) playerInteraction.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // IMMEDIATELY re-enable all controls and relock cursor
            if (playerMovement != null) playerMovement.enabled = true;
            if (cameraLook != null) cameraLook.enabled = true;
            if (playerInteraction != null) playerInteraction.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private IEnumerator AnimateOpen()
    {
        float timer = 0f;
        Vector3 startScale = new Vector3(0.85f, 0.85f, 1f);
        Vector3 targetScale = Vector3.one;

        if (paperRect != null) paperRect.localScale = startScale;
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / animationDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (canvasGroup != null) canvasGroup.alpha = smoothT;
            if (paperRect != null) paperRect.localScale = Vector3.Lerp(startScale, targetScale, smoothT);

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        if (paperRect != null) paperRect.localScale = targetScale;
    }

    private IEnumerator AnimateClose()
    {
        float timer = 0f;
        Vector3 startScale = paperRect != null ? paperRect.localScale : Vector3.one;
        Vector3 targetScale = new Vector3(0.9f, 0.9f, 1f);

        while (timer < animationDuration * 0.75f)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / (animationDuration * 0.75f));

            if (canvasGroup != null) canvasGroup.alpha = 1f - t;
            if (paperRect != null) paperRect.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        FinishClosing();
    }

    private IEnumerator TypewriterReveal(string fullText, float speed)
    {
        float delay = 1f / Mathf.Max(speed, 5f);
        for (int i = 0; i <= fullText.Length; i++)
        {
            if (bodyText != null) bodyText.text = fullText.Substring(0, i);
            yield return new WaitForSecondsRealtime(delay);
        }
        typewriterCoroutine = null;
    }
}
