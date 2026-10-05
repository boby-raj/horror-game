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
    public string nextSceneName = "MainMenu";

    void Start()
    {

        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {

        yield return StartCoroutine(FadeCanvasGroup(studioLogo, 0f, 1f, fadeInDuration));

        yield return new WaitForSeconds(displayDuration);

        yield return StartCoroutine(FadeCanvasGroup(studioLogo, 1f, 0f, fadeOutDuration));

        yield return new WaitForSeconds(timeBetweenLogos);

        yield return StartCoroutine(FadeCanvasGroup(gameLogo, 0f, 1f, fadeInDuration));

        yield return new WaitForSeconds(displayDuration);

        yield return StartCoroutine(FadeCanvasGroup(gameLogo, 1f, 0f, fadeOutDuration));

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
        cg.alpha = endAlpha;
    }
}