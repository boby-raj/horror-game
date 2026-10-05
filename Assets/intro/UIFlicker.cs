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

        if (darknessOverlay == null)
        {
            darknessOverlay = GetComponent<Image>();
        }
    }

    void Update()
    {

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);

        float currentAlpha = noise * maxDarkness;

        if (Random.value < blackoutChance)
        {
            currentAlpha = 1f;
        }

        Color newColor = darknessOverlay.color;
        newColor.a = currentAlpha;
        darknessOverlay.color = newColor;
    }
}