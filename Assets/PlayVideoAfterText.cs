using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class PlayVideoAfterText : MonoBehaviour
{
    [Header("Components")]
    public VideoPlayer myVideoPlayer;
    public GameObject rawImageObject; // Used to hide the video screen before it plays
    public VideoClip videoClip;

    [Header("Transition Settings")]
    public string nextSceneName = "finalmapdone";
    public bool allowSkip = false;

    private bool isTransitioning = false;
    private bool isVideoStarted = false;

    void Awake()
    {
        if (myVideoPlayer == null)
        {
            myVideoPlayer = GetComponent<VideoPlayer>() ?? FindFirstObjectByType<VideoPlayer>();
        }

        if (rawImageObject == null)
        {
            RawImage raw = FindFirstObjectByType<RawImage>();
            if (raw != null)
            {
                rawImageObject = raw.gameObject;
            }
        }

        // Hide screen initially so it doesn't block text
        if (rawImageObject != null)
        {
            rawImageObject.SetActive(false);
        }

        EnsureVideoClipAssigned();
    }

    void Start()
    {
        if (rawImageObject != null)
        {
            rawImageObject.SetActive(false);
        }
    }

    void Update()
    {
        // Video must play completely to the end even if the player presses Space or any other key.
    }

    private void EnsureVideoClipAssigned()
    {
        if (myVideoPlayer != null && myVideoPlayer.clip == null && string.IsNullOrEmpty(myVideoPlayer.url))
        {
            if (videoClip != null)
            {
                myVideoPlayer.source = VideoSource.VideoClip;
                myVideoPlayer.clip = videoClip;
            }
            else
            {
#if UNITY_EDITOR
                string assetPath = "Assets/HARSHA/lv_0_20261004123532.mp4";
                VideoClip loadedClip = UnityEditor.AssetDatabase.LoadAssetAtPath<VideoClip>(assetPath);
                if (loadedClip != null)
                {
                    myVideoPlayer.source = VideoSource.VideoClip;
                    myVideoPlayer.clip = loadedClip;
                }
#endif
            }
        }
    }

    // Call this method exactly when your text finishes appearing
    public void StartVideo()
    {
        if (isVideoStarted) return;
        isVideoStarted = true;

        if (myVideoPlayer == null)
        {
            myVideoPlayer = GetComponent<VideoPlayer>() ?? FindFirstObjectByType<VideoPlayer>();
        }

        if (myVideoPlayer == null)
        {
            Debug.LogWarning("[PlayVideoAfterText] No VideoPlayer found! Transitioning directly to next scene.");
            EndVideoAndLoadNextScene();
            return;
        }

        StartCoroutine(PrepareAndPlayVideo());
    }

    private IEnumerator PrepareAndPlayVideo()
    {
        EnsureVideoClipAssigned();

        myVideoPlayer.isLooping = false;
        myVideoPlayer.playOnAwake = false;
        myVideoPlayer.waitForFirstFrame = true;

        // Auto audio configuration
        if (myVideoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
        {
            AudioSource source = myVideoPlayer.GetTargetAudioSource(0);
            if (source == null)
            {
                source = myVideoPlayer.GetComponent<AudioSource>() ?? myVideoPlayer.gameObject.AddComponent<AudioSource>();
                myVideoPlayer.SetTargetAudioSource(0, source);
            }
        }
        else if (myVideoPlayer.audioOutputMode == VideoAudioOutputMode.Direct)
        {
            myVideoPlayer.EnableAudioTrack(0, true);
            myVideoPlayer.SetDirectAudioVolume(0, 1.0f);
        }

        // Video player aspect ratio
        myVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        // Auto 1920x1080 RenderTexture configuration
        if (myVideoPlayer.renderMode == VideoRenderMode.RenderTexture)
        {
            if (myVideoPlayer.targetTexture != null)
            {
                if (myVideoPlayer.targetTexture.width != 1920 || myVideoPlayer.targetTexture.height != 1080)
                {
                    myVideoPlayer.targetTexture.Release();
                    myVideoPlayer.targetTexture.width = 1920;
                    myVideoPlayer.targetTexture.height = 1080;
                    myVideoPlayer.targetTexture.Create();
                }
            }
            else
            {
                RenderTexture rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
                rt.name = "Dynamic1080pVideoRT";
                rt.Create();
                myVideoPlayer.targetTexture = rt;
            }
        }

        // Configure RawImage to fill canvas with exact 16:9 ratio
        if (rawImageObject != null)
        {
            RawImage rawImage = rawImageObject.GetComponent<RawImage>();
            if (rawImage != null)
            {
                RectTransform rect = rawImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                // Ensure AspectRatioFitter locks the display to exact 1920:1080 (16:9)
                AspectRatioFitter fitter = rawImageObject.GetComponent<AspectRatioFitter>();
                if (fitter == null)
                {
                    fitter = rawImageObject.AddComponent<AspectRatioFitter>();
                }
                fitter.aspectRatio = 1920f / 1080f;
                fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

                if (myVideoPlayer.targetTexture != null)
                {
                    rawImage.texture = myVideoPlayer.targetTexture;
                }
            }
        }

        // Setup loop point reached callback
        myVideoPlayer.loopPointReached += OnVideoFinished;

        // Prepare video asynchronously
        myVideoPlayer.Prepare();

        float timeout = 8f;
        float elapsed = 0f;
        while (!myVideoPlayer.isPrepared && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reveal video display without any white/blank flicker
        if (rawImageObject != null)
        {
            RawImage rawImage = rawImageObject.GetComponent<RawImage>();
            if (rawImage != null && rawImage.texture == null && myVideoPlayer.texture != null)
            {
                rawImage.texture = myVideoPlayer.texture;
            }
            rawImageObject.SetActive(true);
        }

        myVideoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoFinished;
        EndVideoAndLoadNextScene();
    }

    public void SkipVideo()
    {
        EndVideoAndLoadNextScene();
    }

    public void EndVideoAndLoadNextScene()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (myVideoPlayer != null && myVideoPlayer.isPlaying)
        {
            myVideoPlayer.Stop();
        }

        if (!string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
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