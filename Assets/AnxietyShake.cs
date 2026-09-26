using UnityEngine;

public class AnxietyShake : MonoBehaviour
{
    public float shakeSpeed = 20f;
    public float shakeAmount = 1.5f;
    
    private Vector3 originalPosition;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // Creates a microscopic, rapid jitter effect
        float offsetX = Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) * shakeAmount - (shakeAmount / 2f);
        float offsetY = Mathf.PerlinNoise(0f, Time.time * shakeSpeed) * shakeAmount - (shakeAmount / 2f);

        rectTransform.anchoredPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
    }
}