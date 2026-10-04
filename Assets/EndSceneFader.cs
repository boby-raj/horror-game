using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndSceneFader : MonoBehaviour
{
    [Header("UI References")]
    public Image blackScreen;
    public TextMeshProUGUI timeText;
    
    [Header("Audio")]
    public AudioSource endAudio;
    
    [Header("Settings")]
    public float fadeDuration = 2f;
    public float delayBeforeAudio = 0.5f;

    void Start()
    {
        // Ensure starting alpha is exactly 0
        SetAlpha(0f);
        StartCoroutine(PlayEndSceneRoutine());
    }

    IEnumerator PlayEndSceneRoutine()
    {
        float elapsedTime = 0f;

        // Fade in the black screen and text
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        // Ensure alpha is exactly 1 at the end
        SetAlpha(1f);

        // Optional pause before the sound hits
        yield return new WaitForSeconds(delayBeforeAudio);

        // Play the final audio
        if (endAudio != null)
        {
            endAudio.Play();
        }
    }

    private void SetAlpha(float alpha)
    {
        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = alpha;
            blackScreen.color = c;
        }
        
        if (timeText != null)
        {
            Color c = timeText.color;
            c.a = alpha;
            timeText.color = c;
        }
    }
}