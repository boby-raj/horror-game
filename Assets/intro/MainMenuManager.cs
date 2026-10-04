using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button continueButton;

    [Header("Scene Names")]
    public string storyIntroSceneName = "StoryIntro";       // Sends New Game here
    public string mainGameplaySceneName = "finalmapdone";   // Sends Continue here

    private float menuLoadTime = 0f;

    void Start()
    {
        menuLoadTime = Time.unscaledTime;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Check if the player has a save file
        if (continueButton != null)
        {
            if (PlayerPrefs.HasKey("HasSavedGame"))
            {
                continueButton.interactable = true;
            }
            else
            {
                continueButton.interactable = false; 
            }
        }
    }

    public void StartNewGame()
    {
        // Ignore stray click carried over from the Pause Menu Quit button
        if (Time.unscaledTime - menuLoadTime < 0.4f) return;

        // Flag 0: Fresh start
        PlayerPrefs.SetInt("LoadFromSave", 0); 
        
        // Load the text story scene instead of the game
        SceneManager.LoadScene(storyIntroSceneName); 
    }

    public void ContinueGame()
    {
        // Ignore stray click carried over from the Pause Menu Quit button
        if (Time.unscaledTime - menuLoadTime < 0.4f) return;

        // Flag 1: Load saved coordinates
        PlayerPrefs.SetInt("LoadFromSave", 1); 
        
        // Load the actual gameplay scene directly, skipping the story
        if (!string.IsNullOrEmpty(mainGameplaySceneName) && Application.CanStreamedLevelBeLoaded(mainGameplaySceneName))
        {
            SceneManager.LoadScene(mainGameplaySceneName);
        }
        else if (Application.CanStreamedLevelBeLoaded("finalmapdone"))
        {
            SceneManager.LoadScene("finalmapdone");
        }
    }

    public void OpenSettings()
    {
        Debug.Log("Settings Panel Opened");
    }

    public void QuitGame()
    {
        Debug.Log("Game Exiting...");
        Application.Quit(); 

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}