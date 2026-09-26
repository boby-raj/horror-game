using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button continueButton;

    [Header("Scene Names")]
    public string storyIntroSceneName = "StoryIntro";       // Sends New Game here
    public string mainGameplaySceneName = "MainLevel";      // Sends Continue here

    void Start()
    {
        // Check if the player has a save file
        if (PlayerPrefs.HasKey("HasSavedGame"))
        {
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false; 
        }
    }

    public void StartNewGame()
    {
        // Flag 0: Fresh start
        PlayerPrefs.SetInt("LoadFromSave", 0); 
        
        // Load the text story scene instead of the game
        SceneManager.LoadScene(storyIntroSceneName); 
    }

    public void ContinueGame()
    {
        // Flag 1: Load saved coordinates
        PlayerPrefs.SetInt("LoadFromSave", 1); 
        
        // Load the actual gameplay scene directly, skipping the story
        SceneManager.LoadScene(mainGameplaySceneName); 
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