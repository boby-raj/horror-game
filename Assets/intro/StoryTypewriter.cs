using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryTypewriter : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI storyText;

    [Header("Story Settings")]
    [TextArea(3, 10)]
    public string fullStory = "You were just going in for a routine extraction...\n\nBut the waiting room was empty.\n\nAnd the screaming hasn't stopped.";

    public float typingSpeed = 0.05f;
    public float delayAfterStory = 4.0f;

    [Header("Audio Settings")]
    public AudioSource typewriterAudio;

    [Header("Video Cutscene Settings")]
    [Tooltip("Reference to PlayVideoAfterText if present in the scene (optional, auto-detected)")]
    public PlayVideoAfterText videoController;
    [Tooltip("Reference to VideoPlayer component (optional, auto-detected)")]
    public VideoPlayer videoPlayer;
    [Tooltip("The GameObject displaying the video (RawImage or Screen) - activated when video starts (optional, auto-detected)")]
    public GameObject videoScreenObject;
    [Tooltip("Fallback video clip to play if the VideoPlayer has no clip assigned")]
    public VideoClip introVideoClip;
    [Tooltip("Allow the player to skip typing or video with Space/Enter/Escape/Click")]
    public bool allowSkip = true;

    [Header("Next Scene")]
    public string gameplaySceneName = "finalmapdone";

    private bool isTyping = false;
    private bool skipRequested = false;
    private bool isTransitioning = false;
    private Coroutine typewriterCoroutine;

    void Awake()
    {

        if (typewriterAudio == null)
        {
            typewriterAudio = GetComponent<AudioSource>() ?? FindFirstObjectByType<AudioSource>();
        }

        if (videoController == null)
        {
            videoController = FindFirstObjectByType<PlayVideoAfterText>();
        }

        if (videoPlayer == null)
        {
            videoPlayer = FindFirstObjectByType<VideoPlayer>();
        }

        if (videoScreenObject == null)
        {
            RawImage rawImg = FindFirstObjectByType<RawImage>();
            if (rawImg != null)
            {
                videoScreenObject = rawImg.gameObject;
            }
        }

        if (videoScreenObject != null)
        {
            videoScreenObject.SetActive(false);
        }

        EnsureVideoClipConfigured();
    }

    void Start()
    {
        if (storyText != null)
        {
            storyText.text = "";
        }
        typewriterCoroutine = StartCoroutine(TypeStory());
    }

    void Update()
    {
        if (!allowSkip) return;

        if (isTyping && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0)))
        {
            skipRequested = true;
        }
    }

    private void EnsureVideoClipConfigured()
    {
        if (videoPlayer != null && videoPlayer.clip == null && string.IsNullOrEmpty(videoPlayer.url))
        {
            if (introVideoClip != null)
            {
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = introVideoClip;
            }
            else
            {
#if UNITY_EDITOR
                string assetPath = "Assets/HARSHA/lv_0_20261004123532.mp4";
                VideoClip loadedClip = UnityEditor.AssetDatabase.LoadAssetAtPath<VideoClip>(assetPath);
                if (loadedClip != null)
                {
                    videoPlayer.source = VideoSource.VideoClip;
                    videoPlayer.clip = loadedClip;
                }
#endif
            }
        }
    }

    private IEnumerator TypeStory()
    {
        isTyping = true;
        skipRequested = false;

        if (typewriterAudio != null && !typewriterAudio.isPlaying)
        {
            typewriterAudio.Play();
        }

        for (int i = 0; i < fullStory.Length; i++)
        {
            if (skipRequested)
            {
                if (storyText != null)
                {
                    storyText.text = fullStory;
                }
                break;
            }

            if (storyText != null)
            {
                storyText.text += fullStory[i];
            }

            float randomSpeed = typingSpeed + Random.Range(-0.015f, 0.015f);
            yield return new WaitForSeconds(Mathf.Max(0.01f, randomSpeed));
        }

        isTyping = false;

        if (typewriterAudio != null && typewriterAudio.isPlaying)
        {
            typewriterAudio.Stop();
        }

        float elapsed = 0f;
        skipRequested = false;
        while (elapsed < delayAfterStory)
        {
            if (skipRequested)
            {
                break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (storyText != null)
        {
            storyText.gameObject.SetActive(false);
        }

        StartVideoSequence();
    }

    private void StartVideoSequence()
    {
        if (videoController == null)
        {
            videoController = FindFirstObjectByType<PlayVideoAfterText>();
        }

        if (videoController != null)
        {
            videoController.nextSceneName = gameplaySceneName;
            videoController.StartVideo();
            return;
        }

        if (videoPlayer == null)
        {
            videoPlayer = FindFirstObjectByType<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            StartCoroutine(PlayVideoCoroutine());
        }
        else
        {
            Debug.LogWarning("[StoryTypewriter] No VideoPlayer found. Transitioning directly to next scene.");
            TransitionToNextScene();
        }
    }

    private IEnumerator PlayVideoCoroutine()
    {
        EnsureVideoClipConfigured();

        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;

        if (videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
        {
            AudioSource source = videoPlayer.GetTargetAudioSource(0);
            if (source == null)
            {
                source = videoPlayer.GetComponent<AudioSource>() ?? videoPlayer.gameObject.AddComponent<AudioSource>();
                videoPlayer.SetTargetAudioSource(0, source);
            }
        }
        else if (videoPlayer.audioOutputMode == VideoAudioOutputMode.Direct)
        {
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetDirectAudioVolume(0, 1.0f);
        }

        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        if (videoPlayer.renderMode == VideoRenderMode.RenderTexture)
        {
            if (videoPlayer.targetTexture != null)
            {
                if (videoPlayer.targetTexture.width != 1920 || videoPlayer.targetTexture.height != 1080)
                {
                    videoPlayer.targetTexture.Release();
                    videoPlayer.targetTexture.width = 1920;
                    videoPlayer.targetTexture.height = 1080;
                    videoPlayer.targetTexture.Create();
                }
            }
            else
            {
                RenderTexture rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
                rt.name = "Dynamic1080pVideoRT";
                rt.Create();
                videoPlayer.targetTexture = rt;
            }
        }

        if (videoScreenObject != null)
        {
            RawImage rawImage = videoScreenObject.GetComponent<RawImage>();
            if (rawImage != null)
            {
                RectTransform rect = rawImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                AspectRatioFitter fitter = videoScreenObject.GetComponent<AspectRatioFitter>();
                if (fitter == null)
                {
                    fitter = videoScreenObject.AddComponent<AspectRatioFitter>();
                }
                fitter.aspectRatio = 1920f / 1080f;
                fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

                if (videoPlayer.targetTexture != null)
                {
                    rawImage.texture = videoPlayer.targetTexture;
                }
            }
        }

        videoPlayer.loopPointReached += OnVideoEndReached;
        videoPlayer.Prepare();

        float timeout = 8f;
        float timer = 0f;
        while (!videoPlayer.isPrepared && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (videoScreenObject != null)
        {
            RawImage rawImage = videoScreenObject.GetComponent<RawImage>();
            if (rawImage != null && rawImage.texture == null && videoPlayer.texture != null)
            {
                rawImage.texture = videoPlayer.texture;
            }
            videoScreenObject.SetActive(true);
        }

        videoPlayer.Play();
    }

    private void OnVideoEndReached(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoEndReached;
        TransitionToNextScene();
    }

    public void TransitionToNextScene()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        if (!string.IsNullOrEmpty(gameplaySceneName) && Application.CanStreamedLevelBeLoaded(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else if (Application.CanStreamedLevelBeLoaded("finalmapdone"))
        {
            SceneManager.LoadScene("finalmapdone");
        }
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
        }
    }
}