using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Player Reference")]
    public Transform playerTransform;

    private bool isPaused = false;

    void Start()
    {
        // Ensure game runs normally and cursor is locked when playing
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
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
        }
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Freeze time
        isPaused = true;
        
        UnlockCursor(); // Show the mouse so they can click the menu
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Unfreeze time
        isPaused = false;
        
        LockCursor(); // Hide and lock the mouse again for gameplay
    }

  public void QuitToMenu()
    {
        // 1. Save the player's exact X, Y, and Z positions
        PlayerPrefs.SetFloat("PlayerX", playerTransform.position.x);
        PlayerPrefs.SetFloat("PlayerY", playerTransform.position.y);
        PlayerPrefs.SetFloat("PlayerZ", playerTransform.position.z);
        
        // 2. Tell the main menu that a save file now exists
        PlayerPrefs.SetInt("HasSavedGame", 1);
        
        // 3. Force Unity to write this to the hard drive immediately
        PlayerPrefs.Save(); 

        // 4. Unfreeze time and load the menu
        Time.timeScale = 1f; 
        SceneManager.LoadScene(mainMenuSceneName);
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