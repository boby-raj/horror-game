using UnityEngine;
using TMPro;
using System.Collections;

public class GhostHUD : MonoBehaviour 
{
    [Header("UI Elements")]
    public CanvasGroup panelCanvasGroup;
    public TextMeshProUGUI headingText;
    public TextMeshProUGUI bodyText;
    public GameObject okButton; 
    public AudioSource fractureSound;
    
    [Header("Atmosphere Settings")]
    public float slowFadeDuration = 3.5f;
    public Color fractureColor = new Color(0.6f, 0f, 0f); 
    
    private bool isReadyToClose = false;
    private bool isOpen = false;

    void Start()
    {
        panelCanvasGroup.alpha = 0f;
        
        // Ensure starting colors are white before setting alpha
        headingText.color = Color.white;
        bodyText.color = Color.white;
        SetAlpha(headingText, 0f);
        SetAlpha(bodyText, 0f);
        
        if (okButton != null) okButton.SetActive(false);
    }

    void Update()
    {
        // Press Enter to close when the sequence is ready
        if (isReadyToClose && Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(CloseSequenceCoroutine());
        }
    }

    public void TriggerOpen()
    {
        if (!isOpen)
        {
            StartCoroutine(OpenSequenceCoroutine());
        }
    }

    private IEnumerator OpenSequenceCoroutine()
    {
        isOpen = true;
        isReadyToClose = false;
        
        panelCanvasGroup.alpha = 1f;
        if (okButton != null) okButton.SetActive(false);

        headingText.rectTransform.localScale = Vector3.one;
        bodyText.rectTransform.localScale = Vector3.one;

        float time = 0;
        while(time < slowFadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 0.4f, time / slowFadeDuration);
            SetAlpha(headingText, alpha);
            SetAlpha(bodyText, alpha);
            time += Time.deltaTime;
            yield return null;
        }

        if (fractureSound != null) fractureSound.Play();
        
        headingText.color = fractureColor;
        bodyText.color = fractureColor;
        
        Vector3 fractureScale = new Vector3(1.04f, 1.04f, 1f);
        headingText.rectTransform.localScale = fractureScale;
        bodyText.rectTransform.localScale = fractureScale;
        
        yield return new WaitForSeconds(0.1f);

        SetAlpha(headingText, 0.85f);
        SetAlpha(bodyText, 0.85f);
        headingText.rectTransform.localScale = Vector3.one;
        bodyText.rectTransform.localScale = Vector3.one;

        yield return new WaitForSeconds(0.5f);
        
        if (okButton != null) okButton.SetActive(true);

        isReadyToClose = true;
    }

    private IEnumerator CloseSequenceCoroutine()
    {
        isReadyToClose = false;
        float time = 0;
        float fadeOutTime = 1.0f;
        
        while(time < fadeOutTime)
        {
            panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeOutTime);
            time += Time.deltaTime;
            yield return null;
        }

        panelCanvasGroup.alpha = 0f;
        
        // Reset back to white for the next time you open a note
        headingText.color = Color.white;
        bodyText.color = Color.white;
        SetAlpha(headingText, 0f);
        SetAlpha(bodyText, 0f);
        
        if (okButton != null) okButton.SetActive(false);
        
        isOpen = false; 
    }

    private void SetAlpha(TextMeshProUGUI txt, float alpha)
    {
        // FIX: Grab the current color (whether white or dark red) instead of forcing white
        Color c = txt.color; 
        c.a = alpha;
        txt.color = c;
    }
}