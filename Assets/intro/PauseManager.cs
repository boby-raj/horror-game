using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Player Reference")]
    public Transform playerTransform;

    private bool isPaused = false;
    private Button quitButton;
    private Button resumeButton;
    private TextMeshProUGUI quitButtonText;
    private TextMeshProUGUI resumeButtonText;
    private Vector3 quitOrigScale = Vector3.one;
    private Vector3 resumeOrigScale = Vector3.one;
    private Color quitOrigColor = Color.white;
    private Color resumeOrigColor = Color.white;

    void Awake()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        ConfigurePauseMenuButtons();
    }

    void Start()
    {
        // Ensure game runs normally and cursor is locked when playing
        Time.timeScale = 1f;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        LockCursor();
    }

    void Update()
    {
        // Trigger pause/resume exclusively with the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
            return;
        }

        if (!isPaused) return;

        // Keep cursor unlocked while paused in case any other script tries to lock it
        if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
        {
            UnlockCursor();
        }

        // Direct unscaled pointer hover + click detection so Pause Menu buttons
        // ALWAYS respond even when Time.timeScale == 0 or another Canvas (like FadeScreen) exists.
        HandleDirectPauseMenuInput();
    }

    private void HandleDirectPauseMenuInput()
    {
        if (pauseMenuPanel == null || !pauseMenuPanel.activeInHierarchy) return;

        Vector2 mousePos = Input.mousePosition;

        bool overQuit = IsMouseOverButton(quitButton, quitButtonText, mousePos);
        bool overResume = !overQuit && IsMouseOverButton(resumeButton, resumeButtonText, mousePos);

        // Apply visual hover feedback (works even at Time.timeScale = 0)
        if (quitButton != null)
        {
            quitButton.transform.localScale = overQuit ? quitOrigScale * 1.1f : quitOrigScale;
            if (quitButtonText != null)
            {
                quitButtonText.color = overQuit ? new Color(0.6f, 0f, 0f, 1f) : quitOrigColor;
            }
        }

        if (resumeButton != null)
        {
            resumeButton.transform.localScale = overResume ? resumeOrigScale * 1.1f : resumeOrigScale;
            if (resumeButtonText != null)
            {
                resumeButtonText.color = overResume ? new Color(0.6f, 0f, 0f, 1f) : resumeOrigColor;
            }
        }

        // Handle Left Mouse Click on button release (MouseUp) so the click does not carry over into MainMenu buttons
        if (Input.GetMouseButtonUp(0))
        {
            if (overQuit)
            {
                Debug.Log("[PauseManager] Quit button clicked -> QuitToMenu()");
                QuitToMenu();
            }
            else if (overResume)
            {
                Debug.Log("[PauseManager] Resume button clicked -> ResumeGame()");
                ResumeGame();
            }
        }
    }

    private bool IsMouseOverButton(Button btn, TextMeshProUGUI txt, Vector2 screenPoint)
    {
        if (btn == null || !btn.gameObject.activeInHierarchy) return false;

        Canvas canvas = btn.GetComponentInParent<Canvas>();
        Camera uiCam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        RectTransform btnRect = btn.GetComponent<RectTransform>();
        if (btnRect != null && RectTransformUtility.RectangleContainsScreenPoint(btnRect, screenPoint, uiCam))
        {
            return true;
        }

        if (txt != null)
        {
            RectTransform txtRect = txt.GetComponent<RectTransform>();
            if (txtRect != null && RectTransformUtility.RectangleContainsScreenPoint(txtRect, screenPoint, uiCam))
            {
                return true;
            }
        }

        return false;
    }

    private void ConfigurePauseMenuButtons()
    {
        if (pauseMenuPanel == null) return;

        // 1. Put the PauseMenuPanel's Canvas on a high sortingOrder so secondary Canvases (like HARSHA/Canvas/FadeScreen) cannot block it
        Canvas parentCanvas = pauseMenuPanel.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            parentCanvas.sortingOrder = 999;
            if (parentCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        // 2. Disable raycastTarget on any full-screen 'FadeScreen' image that blocks mouse clicks across the screen
        Image[] allImages = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Image img in allImages)
        {
            if (img.gameObject.name.Equals("FadeScreen", System.StringComparison.OrdinalIgnoreCase))
            {
                img.raycastTarget = false;
            }
        }

        // 3. Configure Resume and Quit buttons
        Button[] buttons = pauseMenuPanel.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null && !string.IsNullOrEmpty(tmp.text))
            {
                tmp.text = tmp.text.Trim();
                tmp.raycastTarget = true;
            }

            Image img = btn.GetComponent<Image>();
            if (img == null)
            {
                img = btn.gameObject.AddComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0.01f);
            }
            img.raycastTarget = true;

            if (btn.targetGraphic == null)
            {
                btn.targetGraphic = img;
            }

            string btnName = btn.gameObject.name.ToLowerInvariant();
            string txtContent = (tmp != null && tmp.text != null) ? tmp.text.ToLowerInvariant() : "";

            if (btnName.Contains("quit") || txtContent.Contains("quit"))
            {
                quitButton = btn;
                quitButtonText = tmp;
                quitOrigScale = btn.transform.localScale;
                if (tmp != null) quitOrigColor = tmp.color;

                btn.onClick.RemoveListener(QuitToMenu);
                btn.onClick.AddListener(QuitToMenu);
            }
            else if (btnName.Contains("resume") || txtContent.Contains("resume"))
            {
                resumeButton = btn;
                resumeButtonText = tmp;
                resumeOrigScale = btn.transform.localScale;
                if (tmp != null) resumeOrigColor = tmp.color;

                btn.onClick.RemoveListener(ResumeGame);
                btn.onClick.AddListener(ResumeGame);
            }
        }
    }

    public void PauseGame()
    {
        if (pauseMenuPanel != null)
        {
            ConfigurePauseMenuButtons();
            pauseMenuPanel.transform.SetAsLastSibling();
            pauseMenuPanel.SetActive(true);
        }

        Time.timeScale = 0f; // Freeze time
        isPaused = true;

        UnlockCursor(); // Show the mouse so they can click the menu
    }

    public void ResumeGame()
    {
        if (!isPaused && (pauseMenuPanel == null || !pauseMenuPanel.activeSelf)) return;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        Time.timeScale = 1f; // Unfreeze time
        isPaused = false;

        LockCursor(); // Hide and lock the mouse again for gameplay
    }

    public void QuitToMenu()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        // 1. Save the player's exact X, Y, and Z positions if playerTransform is available
        if (playerTransform != null)
        {
            PlayerPrefs.SetFloat("PlayerX", playerTransform.position.x);
            PlayerPrefs.SetFloat("PlayerY", playerTransform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", playerTransform.position.z);

            // 2. Tell the main menu that a save file now exists
            PlayerPrefs.SetInt("HasSavedGame", 1);

            // 3. Force Unity to write this to the hard drive immediately
            PlayerPrefs.Save();
        }

        // 4. Unfreeze time, keep cursor unlocked for the menu, and load the menu
        isPaused = false;
        Time.timeScale = 1f;
        UnlockCursor();

        if (!string.IsNullOrEmpty(mainMenuSceneName) && Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else if (Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    // --- Cursor Management Methods ---
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}