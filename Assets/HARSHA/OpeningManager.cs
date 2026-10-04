using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;

/// <summary>
/// Controls the waking up sequence:
/// 1. Freezes player movement and mouse look during the cutscene.
/// 2. Smoothly fades in from black (eyes opening effect).
/// 3. Re-enables player movement, mouse look, and hands full control to the player when the wake-up finishes.
/// </summary>
public class OpeningManager : MonoBehaviour
{
    [Header("Timeline Director")]
    public PlayableDirector timelineDirector;

    [Header("Player Scripts to Freeze")]
    public MonoBehaviour playerMovementScript; 
    public MonoBehaviour playerLookScript;
    public CinemachineBrain mainCameraBrain;

    [Header("Atmospheric Wake-Up Polish")]
    [Tooltip("Simulates waking up / opening eyes by fading in from black")]
    public bool fadeFromBlack = true;
    public float fadeDuration = 1.0f;

    private Texture2D blackTexture;
    private float startTime;
    private bool controlsUnlocked = false;

    void Awake()
    {
        startTime = Time.time;
        if (fadeFromBlack)
        {
            blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, Color.black);
            blackTexture.Apply();
        }

        AutoFindReferences();
    }

    void OnEnable()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped += UnlockControls;
        }
    }

    void OnDisable()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped -= UnlockControls;
        }
    }

    void Start()
    {
        AutoFindReferences();

        // Ensure director plays from time 0
        if (timelineDirector != null)
        {
            timelineDirector.time = 0;
            timelineDirector.Play();
        }

        // Freeze player movement and mouse look at the start of cutscene
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        if (playerLookScript != null)
        {
            playerLookScript.enabled = false;
        }
    }

    void Update()
    {
        // Safety check: if timeline finished or reached end, unlock controls
        if (!controlsUnlocked && timelineDirector != null)
        {
            if (timelineDirector.time >= timelineDirector.duration - 0.05 && timelineDirector.time > 1.0)
            {
                UnlockControls(timelineDirector);
            }
        }
    }

    private void AutoFindReferences()
    {
        if (timelineDirector == null)
        {
            timelineDirector = GetComponent<PlayableDirector>();
        }

        if (playerMovementScript == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerMovementScript = player.GetComponent("jump") as MonoBehaviour;
            }
        }

        if (playerLookScript == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                playerLookScript = cam.GetComponent("mouselook") as MonoBehaviour;
            }
        }

        if (mainCameraBrain == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                mainCameraBrain = cam.GetComponent<CinemachineBrain>();
            }
        }
    }

    void UnlockControls(PlayableDirector director)
    {
        if (controlsUnlocked) return;
        controlsUnlocked = true;

        // 1. Give player their movement and look control back
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        if (playerLookScript != null)
        {
            playerLookScript.enabled = true;
        }
        
        // 2. Shut down Cinemachine Brain so player's camera takes full direct control
        if (mainCameraBrain != null)
        {
            mainCameraBrain.enabled = false;
        }

        Debug.Log("[OpeningManager] Wake-up cutscene complete. Player controls unlocked!");
    }

    void OnGUI()
    {
        if (!fadeFromBlack || blackTexture == null) return;

        float elapsed = Time.time - startTime;
        if (elapsed < fadeDuration)
        {
            float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            Color oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
            GUI.color = oldColor;
        }
    }

    void OnDestroy()
    {
        if (blackTexture != null)
        {
            Destroy(blackTexture);
        }
    }
}