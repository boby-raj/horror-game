using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup studioLogo;
    public CanvasGroup gameLogo;

    [Header("Timing Settings")]
    public float fadeInDuration = 1.5f;
    public float displayDuration = 2.0f;
    public float fadeOutDuration = 1.5f;
    public float timeBetweenLogos = 0.5f;

    [Header("Scene Transition")]
    public string nextSceneName = "MainMenu"; // Change this to your actual next scene

    void Start()
    {
        // Start the sequence as soon as the scene loads
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // 1. Fade In Studio Logo
        yield return StartCoroutine(FadeCanvasGroup(studioLogo, 0f, 1f, fadeInDuration));
        
        // 2. Wait
        yield return new WaitForSeconds(displayDuration);
        
        // 3. Fade Out Studio Logo
        yield return StartCoroutine(FadeCanvasGroup(studioLogo, 1f, 0f, fadeOutDuration));
        
        // 4. Short pause between logos
        yield return new WaitForSeconds(timeBetweenLogos);
        
        // 5. Fade In Game Logo
        yield return StartCoroutine(FadeCanvasGroup(gameLogo, 0f, 1f, fadeInDuration));
        
        // 6. Wait
        yield return new WaitForSeconds(displayDuration);
        
        // 7. Fade Out Game Logo
        yield return StartCoroutine(FadeCanvasGroup(gameLogo, 1f, 0f, fadeOutDuration));

        // 8. Load Next Scene
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        cg.alpha = endAlpha; // Ensure it reaches the exact target alpha
    }
}