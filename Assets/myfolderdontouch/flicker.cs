using System.Collections;
using UnityEngine;

public class TriggeredLightFlicker : MonoBehaviour
{
    [Header("Target Light")]
    public Light targetLight;
    public float minIntensity = 0.1f;
    public float maxIntensity = 2.5f;
    public float minDelay = 0.02f;
    public float maxDelay = 0.15f;

    [Header("Trigger Setup")]
    public string playerTag = "Player";

    [Header("Audio")]
    [Tooltip("Plays ONCE when player steps in trigger (e.g. switch click or electric spark SFX)")]
    public AudioSource triggerSFX;
    [Tooltip("Continuous electric humming/buzzing audio")]
    public AudioSource buzzAudioLoop;

    [Header("Optional Mesh Emission Glow")]
    public Renderer bulbRenderer;

    private bool isActivated = false;
    private Material bulbMaterial;
    private Color originalEmissionColor;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Start()
    {

        if (targetLight != null)
        {
            targetLight.enabled = false;
        }

        if (bulbRenderer != null)
        {
            bulbMaterial = bulbRenderer.material;
            if (bulbMaterial.HasProperty(EmissionColor))
            {
                originalEmissionColor = bulbMaterial.GetColor(EmissionColor);
                bulbMaterial.SetColor(EmissionColor, Color.black);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated && other.CompareTag(playerTag))
        {
            ActivateLight();
        }
    }

    public void ActivateLight()
    {
        isActivated = true;

        if (targetLight != null)
        {
            targetLight.enabled = true;
        }

        if (triggerSFX != null)
        {
            triggerSFX.Play();
        }

        if (buzzAudioLoop != null)
        {
            buzzAudioLoop.loop = true;
            buzzAudioLoop.Play();
        }

        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (isActivated)
        {
            float randomIntensity = Random.Range(minIntensity, maxIntensity);

            if (targetLight != null)
            {
                targetLight.intensity = randomIntensity;
            }

            if (bulbMaterial != null && bulbMaterial.HasProperty(EmissionColor))
            {
                float brightnessFactor = randomIntensity / maxIntensity;
                bulbMaterial.SetColor(EmissionColor, originalEmissionColor * brightnessFactor);
            }

            if (buzzAudioLoop != null)
            {
                buzzAudioLoop.volume = Mathf.Clamp01(randomIntensity / maxIntensity);
            }

            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        }
    }
}