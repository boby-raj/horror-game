using UnityEngine;
using UnityEngine.UI;

public class UIFlicker : MonoBehaviour
{
    [Header("UI References")]
    public Image darknessOverlay;

    [Header("Flicker Settings")]
    [Tooltip("How dark the room gets. 1 is pitch black, 0 is fully visible.")]
    public float maxDarkness = 0.85f; 
    [Tooltip("How fast the light sputters.")]
    public float flickerSpeed = 15f; 
    [Tooltip("Chance per frame for the bulb to completely short out for a split second.")]
    [Range(0f, 1f)]
    public float blackoutChance = 0.02f;

    void Start()
    {
        // Automatically grab the Image component if you forget to assign it
        if (darknessOverlay == null)
        {
            darknessOverlay = GetComponent<Image>();
        }
    }

    void Update()
    {
        // 1. Generate chaotic noise to simulate a failing bulb
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        
        // 2. Calculate the new alpha (transparency). 
        // When the noise is high, the alpha is high (the room gets dark).
        float currentAlpha = noise * maxDarkness;
        
        // 3. Occasionally plunge the room into total darkness to scare the player
        if (Random.value < blackoutChance) 
        {
            currentAlpha = 1f; // 1 = fully opaque black
        }

        // 4. Apply the new transparency to the image color
        Color newColor = darknessOverlay.color;
        newColor.a = currentAlpha;
        darknessOverlay.color = newColor;
    }
}