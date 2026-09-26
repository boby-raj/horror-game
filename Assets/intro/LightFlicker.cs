using System.Collections;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light myLight;
    public float baseIntensity = 3.0f;
    public float maxFlickerIntensity = 0.5f; // How much it can dim
    public float flickerSpeed = 0.05f;

    void Start()
    {
        myLight = GetComponent<Light>();
    }

    void Update()
    {
        // Simple, noisy intensity change
        float currentNoise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0);
        
        // This causes the intensity to wobble around the base value
        float targetIntensity = baseIntensity - (currentNoise * maxFlickerIntensity);
        
        // Optionally, make it sometimes completely black
        if (Random.value < 0.01f) // 1% chance per frame to go black
        {
             targetIntensity = 0f;
        }

        myLight.intensity = targetIntensity;
    }
}