using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryTypewriter : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI storyText;

    [Header("Story Settings")]
    [TextArea(3, 10)]
    public string fullStory = "You were just going in for a routine extraction...\n\nBut the waiting room was empty.\n\nAnd the screaming hasn't stopped.";
    
    public float typingSpeed = 0.05f;
    public float delayAfterStory = 4.0f;
    
    [Header("Next Scene")]
    public string gameplaySceneName = "MainLevel";

    void Start()
    {
        // Clear the text completely at the start
        storyText.text = "";
        StartCoroutine(TypeStory());
    }

    private IEnumerator TypeStory()
    {
        // Loop through each character in the string
        foreach (char letter in fullStory.ToCharArray())
        {
            storyText.text += letter;
            
            // Add a tiny bit of random delay to make it feel human/unsettling
            float randomSpeed = typingSpeed + Random.Range(-0.02f, 0.02f);
            yield return new WaitForSeconds(randomSpeed);
        }

        // Wait for the player to finish reading the horror
        yield return new WaitForSeconds(delayAfterStory);

        // Load the actual gameplay scene
        SceneManager.LoadScene(gameplaySceneName);
    }
}