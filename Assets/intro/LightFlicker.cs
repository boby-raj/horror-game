using System.Collections;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light myLight;
    public float baseIntensity = 3.0f;
    public float maxFlickerIntensity = 0.5f;
    public float flickerSpeed = 0.05f;

    void Start()
    {
        myLight = GetComponent<Light>();
    }

    void Update()
    {

        float currentNoise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0);

        float targetIntensity = baseIntensity - (currentNoise * maxFlickerIntensity);

        if (Random.value < 0.01f)
        {
             targetIntensity = 0f;
        }

        myLight.intensity = targetIntensity;
    }
}