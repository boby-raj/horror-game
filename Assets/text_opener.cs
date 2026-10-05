using UnityEngine;
using TMPro;
using System.Collections;

public class text_opener : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questText;
    public string objectiveMessage = "Find a way out.";

    [Header("Timing Settings")]
    public float flashDuration = 0.05f;
    public float darkPause = 0.5f;
    public float burnInDuration = 3f;
    public float displayDuration = 5f;
    public float fadeOutDuration = 2f;

    private bool isTriggered = false;

    void Start()
    {

        Color c = questText.color;
        c.a = 0f;
        questText.color = c;
        questText.text = objectiveMessage;
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.O) && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(TriggerGhostSequence());
        }
    }

    private IEnumerator TriggerGhostSequence()
    {
        Color textColor = questText.color;

        textColor.a = 1f;
        questText.color = textColor;
        yield return new WaitForSeconds(flashDuration);

        textColor.a = 0f;
        questText.color = textColor;
        yield return new WaitForSeconds(darkPause);

        float timeElapsed = 0f;
        while (timeElapsed < burnInDuration)
        {
            textColor.a = Mathf.Lerp(0f, 0.7f, timeElapsed / burnInDuration);
            questText.color = textColor;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(displayDuration);

        timeElapsed = 0f;
        while (timeElapsed < fadeOutDuration)
        {
            textColor.a = Mathf.Lerp(0.7f, 0f, timeElapsed / fadeOutDuration);
            questText.color = textColor;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}