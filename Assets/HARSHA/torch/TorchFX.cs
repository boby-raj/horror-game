using UnityEngine;

public class TorchFX : MonoBehaviour
{
    public static TorchFX Instance { get; private set; }

    [Header("Toggle & State")]
    public bool isOn = true;
    public KeyCode toggleKey = KeyCode.F;
    public bool enableInputToggle = true;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip toggleClip;

    [Header("Light & Flicker")]
    public Light torchLight;
    public float baseIntensity = 20f;
    public float flickerAmount = 0.28f;
    public float flickerSpeed = 8f;

    [Header("Sway")]
    public float swayAmount = 0.39f;
    public float maxSway = 0.36f;
    public float swaySmooth = 8.97f;

    Vector3 startPos;
    float seed;
    Camera aimCamera;

    void Awake()
    {
        Instance = this;
        seed = Random.value * 100f;

        if (!torchLight)
        {
            torchLight = GetComponentInChildren<Light>(true);
        }

        if (!audioSource)
        {
            audioSource = GetComponent<AudioSource>();
        }

        AutoAttachToCameraIfNeeded();
        startPos = transform.localPosition;
        CompensateParentScale();
        ApplyLightState(false);
    }

    void Start()
    {
        if (!aimCamera)
        {
            FindAimCamera();
        }

        SyncWithEnemyAI();
    }

    void OnEnable()
    {
        Instance = this;
    }

    /// <summary>
    /// If someone in another branch drops the prefab under Capsule/Player instead of the Camera,
    /// auto-reparent to the player's Camera so it rotates naturally with head look.
    /// </summary>
    void AutoAttachToCameraIfNeeded()
    {
        aimCamera = GetComponentInParent<Camera>();
        if (aimCamera == null)
        {
            Camera cam = Camera.main;
            if (cam == null && transform.root != null)
            {
                cam = transform.root.GetComponentInChildren<Camera>(true);
            }

            if (cam != null && transform.parent != cam.transform)
            {
                transform.SetParent(cam.transform, false);
                transform.localPosition = new Vector3(0.2f, -0.15f, 0.55f);
                transform.localRotation = Quaternion.identity;
                aimCamera = cam;
            }
        }
    }

    void FindAimCamera()
    {
        aimCamera = GetComponentInParent<Camera>();
        if (!aimCamera) aimCamera = Camera.main;
        if (!aimCamera && transform.root != null)
        {
            aimCamera = transform.root.GetComponentInChildren<Camera>(true);
        }
    }

    void CompensateParentScale()
    {
        if (transform.parent)
        {
            Vector3 scale = transform.parent.lossyScale;
            transform.localScale = new Vector3(
                Mathf.Abs(scale.x) > 0.0001f ? 1f / scale.x : 1f,
                Mathf.Abs(scale.y) > 0.0001f ? 1f / scale.y : 1f,
                Mathf.Abs(scale.z) > 0.0001f ? 1f / scale.z : 1f);
        }
    }

    /// <summary>
    /// Registers this torch's light with any monster/enemy AI in the scene.
    /// </summary>
    public void SyncWithEnemyAI()
    {
        if (!torchLight) return;
        PatrolAndChaseAI[] ais = Object.FindObjectsByType<PatrolAndChaseAI>(FindObjectsSortMode.None);
        foreach (var ai in ais)
        {
            if (ai != null && (ai.playerFlashlight == null || !ai.playerFlashlight.gameObject.activeInHierarchy))
            {
                ai.playerFlashlight = torchLight;
            }
        }
    }

    void Update()
    {
        if (enableInputToggle && Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        SetTorchOn(!isOn);
    }

    public void SetTorchOn(bool on)
    {
        isOn = on;
        ApplyLightState(true);
    }

    void ApplyLightState(bool playAudio)
    {
        if (torchLight)
        {
            torchLight.enabled = isOn;
        }

        if (playAudio && audioSource)
        {
            if (toggleClip)
            {
                audioSource.PlayOneShot(toggleClip);
            }
            else
            {
                audioSource.Play();
            }
        }
    }

    /// <summary>
    /// Static helper to shut off torches (e.g. from jumpscare triggers or cutscenes).
    /// </summary>
    public static void TurnOffAll()
    {
        if (Instance != null)
        {
            Instance.SetTorchOn(false);
        }
        else
        {
            TorchFX[] all = Object.FindObjectsByType<TorchFX>(FindObjectsSortMode.None);
            foreach (var t in all) t.SetTorchOn(false);
        }
    }

    /// <summary>
    /// Static helper to turn on torches.
    /// </summary>
    public static void TurnOnAll()
    {
        if (Instance != null)
        {
            Instance.SetTorchOn(true);
        }
        else
        {
            TorchFX[] all = Object.FindObjectsByType<TorchFX>(FindObjectsSortMode.None);
            foreach (var t in all) t.SetTorchOn(true);
        }
    }

    /// <summary>
    /// Backwards compatibility for external scripts that monitor an external power switch.
    /// </summary>
    public void BindPowerSource(GameObject source)
    {
        if (source != null)
        {
            SetTorchOn(source.activeInHierarchy);
        }
    }

    void LateUpdate()
    {
        if (Time.timeScale <= 0f) return;

        if (torchLight && torchLight.isActiveAndEnabled && isOn)
        {
            float n = Mathf.PerlinNoise(seed, Time.time * flickerSpeed);
            torchLight.intensity = Mathf.Max(0f, baseIntensity) * (1f + (n - 0.5f) * 2f * Mathf.Clamp01(flickerAmount));
        }

        bool looking = Cursor.lockState == CursorLockMode.Locked;
        float mx = looking ? Mathf.Clamp(-Input.GetAxis("Mouse X") * swayAmount, -maxSway, maxSway) : 0f;
        float my = looking ? Mathf.Clamp(-Input.GetAxis("Mouse Y") * swayAmount, -maxSway, maxSway) : 0f;
        Vector3 target = startPos + new Vector3(mx, my, 0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, target, 1f - Mathf.Exp(-Mathf.Max(0f, swaySmooth) * Time.deltaTime));

        if (torchLight && aimCamera)
        {
            Ray aim = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            torchLight.transform.SetPositionAndRotation(aim.origin,
                Quaternion.LookRotation(aim.direction, aimCamera.transform.up));
        }
    }
}
