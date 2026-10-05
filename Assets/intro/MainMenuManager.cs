using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button continueButton;

    [Header("Scene Names")]
    public string storyIntroSceneName = "StoryIntro";
    public string mainGameplaySceneName = "finalmapdone";

    private float menuLoadTime = 0f;

    void Start()
    {
        menuLoadTime = Time.unscaledTime;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

        if (Time.unscaledTime - menuLoadTime < 0.4f) return;

        PlayerPrefs.SetInt("LoadFromSave", 0);

        SceneManager.LoadScene(storyIntroSceneName);
    }

    public void ContinueGame()
    {

        if (Time.unscaledTime - menuLoadTime < 0.4f) return;

        PlayerPrefs.SetInt("LoadFromSave", 1);

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