using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

// Scene-local ending: uses the same movement/look components as finalmapdone.
public class ExitGateSequence : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Collider endingTrigger;
    [SerializeField] private VideoClip openingVideo;
    [SerializeField] private VideoClip endingVideo;
    [SerializeField] private Behaviour[] playerControls;
    [SerializeField] private Canvas playbackCanvas;
    [SerializeField, Min(0f)] private float fadeDuration = 1f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float endTextDisplayDuration = 3f;

    private VideoPlayer videoPlayer;
    private bool[] controlStates;
    private bool started;
    private bool finished;
    private bool missingVideoReported;
    private CursorLockMode previousLock;
    private bool previousCursorVisible;
    private float prepareStarted;
    private GameObject playbackRoot;
    private RawImage videoImage;
    private Image fadeImage;
    private Text endText;
    private bool receivedFirstFrame;
    private bool playingOpening;

    void Awake()
    {
        Time.timeScale = 1f;
        if (playbackCanvas) CreatePlaybackUI();
        if (openingVideo && playbackCanvas && player && playerCamera)
            StartPlayback(openingVideo, true);
    }

    private RectTransform CreateLayer(string layerName, Transform parent)
    {
        GameObject layer = new GameObject(layerName, typeof(RectTransform));
        layer.layer = playbackCanvas.gameObject.layer;
        RectTransform rect = (RectTransform)layer.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    private void CreatePlaybackUI()
    {
        RectTransform root = CreateLayer("EndingPlayback", playbackCanvas.transform);
        playbackRoot = root.gameObject;
        videoImage = CreateLayer("FullscreenVideo", root).gameObject.AddComponent<RawImage>();
        videoImage.raycastTarget = false;
        videoImage.gameObject.SetActive(false);
        fadeImage = CreateLayer("FadeToBlack", root).gameObject.AddComponent<Image>();
        fadeImage.raycastTarget = false;
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        endText = CreateLayer("EndingTitle", root).gameObject.AddComponent<Text>();
        endText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        endText.fontSize = 48;
        endText.alignment = TextAnchor.MiddleCenter;
        endText.color = Color.white;
        endText.text = "He Shouldn't Have Gone There";
        endText.raycastTarget = false;
        endText.gameObject.SetActive(false);
        playbackRoot.SetActive(false);
    }

    void Update()
    {
        if (started)
        {
            if (!finished && videoPlayer && !receivedFirstFrame && Time.unscaledTime - prepareStarted > 20f)
                PlaybackFailed(videoPlayer, "Video preparation or first frame timed out.");
            return;
        }
    }

    void OnTriggerEnter(Collider other) => TryStartEnding(other);
    void OnTriggerStay(Collider other) => TryStartEnding(other);

    private void TryStartEnding(Collider other)
    {
        if (started || !isActiveAndEnabled || !player || !playerCamera || Time.timeScale <= 0f) return;
        if (!endingTrigger || other != endingTrigger) return;

        if (!endingVideo || !playbackCanvas)
        {
            if (!missingVideoReported)
            {
                Debug.LogWarning("Exit trigger reached. Assign Ending Video and Playback Canvas on ExitGateSequence.", this);
                missingVideoReported = true;
            }
            return;
        }

        StartEnding();
    }

    private void StartEnding()
    {
        StartPlayback(endingVideo, false);
    }

    private void StartPlayback(VideoClip clip, bool opening)
    {
        if (!playbackRoot) CreatePlaybackUI();
        started = true;
        finished = false;
        receivedFirstFrame = false;
        playingOpening = opening;
        endText.gameObject.SetActive(false);
        videoImage.gameObject.SetActive(false);
        videoImage.texture = null;
        if (opening) fadeImage.color = Color.black;
        controlStates = new bool[playerControls.Length];
        for (int i = 0; i < playerControls.Length; i++)
        {
            if (!playerControls[i]) continue;
            controlStates[i] = playerControls[i].enabled;
            playerControls[i].enabled = false;
        }
        previousLock = opening ? CursorLockMode.Locked : Cursor.lockState;
        previousCursorVisible = opening ? false : Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        playbackRoot.transform.SetAsLastSibling();
        playbackRoot.SetActive(true);
        StartCoroutine(FadeThenPrepare(clip));
    }

    private IEnumerator FadeThenPrepare(VideoClip clip)
    {
        yield return FadeTo(1f);

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.prepareCompleted += Prepared;
        videoPlayer.loopPointReached += Completed;
        videoPlayer.errorReceived += PlaybackFailed;
        videoPlayer.frameReady += FirstFrameReady;
        prepareStarted = Time.unscaledTime;
        videoPlayer.Prepare();
    }

    private void Prepared(VideoPlayer source) => source.Play();
    private void FirstFrameReady(VideoPlayer source, long frame)
    {
        if (receivedFirstFrame) return;
        receivedFirstFrame = true;
        source.sendFrameReadyEvents = false;
        videoImage.texture = source.texture;
        UpdateVideoCrop();
        videoImage.gameObject.SetActive(true);
        StartCoroutine(FadeTo(0f));
    }

    private void Completed(VideoPlayer source)
    {
        finished = true;
        StopAllCoroutines();
        StartCoroutine(FinishEnding());
    }

    private IEnumerator FinishEnding()
    {
        yield return FadeTo(1f);
        videoImage.gameObject.SetActive(false);
        if (playingOpening)
        {
            ReleaseVideo();
            videoImage.texture = null;
            yield return FadeTo(0f);
            playbackRoot.SetActive(false);
            RestoreControls();
            playingOpening = false;
            finished = false;
            started = false;
        }
        else
        {
            endText.gameObject.SetActive(true);

            if (endTextDisplayDuration > 0f)
                yield return new WaitForSeconds(endTextDisplayDuration);

            // Unlock and reveal mouse cursor for the Main Menu buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;

            // Load the Main Menu scene
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private IEnumerator FadeTo(float alpha)
    {
        float initial = fadeImage.color.a;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(initial, alpha, Mathf.Clamp01(elapsed / fadeDuration)));
            yield return null;
        }
        fadeImage.color = new Color(0f, 0f, 0f, alpha);
    }

    void LateUpdate()
    {
        if (videoImage && videoImage.gameObject.activeInHierarchy) UpdateVideoCrop();
    }

    private void UpdateVideoCrop()
    {
        if (!videoImage.texture || videoImage.texture.height == 0) return;
        Rect rect = videoImage.rectTransform.rect;
        if (rect.height <= 0f || rect.width <= 0f) return;
        float sourceAspect = (float)videoImage.texture.width / videoImage.texture.height;
        float screenAspect = rect.width / rect.height;
        // Fill every screen size without stretching; crop excess edges if necessary.
        float width = Mathf.Min(1f, screenAspect / sourceAspect);
        float height = Mathf.Min(1f, sourceAspect / screenAspect);
        videoImage.uvRect = new Rect((1f - width) * 0.5f, (1f - height) * 0.5f, width, height);
    }

    private void PlaybackFailed(VideoPlayer source, string message)
    {
        Debug.LogError($"Exit video failed: {message}", this);
        StopAllCoroutines();
        ReleaseVideo();
        if (playbackRoot) playbackRoot.SetActive(false);
        RestoreControls();
        if (playingOpening)
        {
            playingOpening = false;
            started = false;
            finished = false;
            return;
        }
        // Avoid retrying a broken video every frame while standing at the gate.
        enabled = false;
    }

    private void RestoreControls()
    {
        if (controlStates == null) return;
        for (int i = 0; i < playerControls.Length; i++)
            if (playerControls[i]) playerControls[i].enabled = controlStates[i];
        Cursor.lockState = previousLock;
        Cursor.visible = previousCursorVisible;
        controlStates = null;
    }

    private void ReleaseVideo()
    {
        if (!videoPlayer) return;
        videoPlayer.prepareCompleted -= Prepared;
        videoPlayer.loopPointReached -= Completed;
        videoPlayer.errorReceived -= PlaybackFailed;
        videoPlayer.frameReady -= FirstFrameReady;
        videoPlayer.Stop();
        Destroy(videoPlayer);
        videoPlayer = null;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        ReleaseVideo();
        if (playbackRoot) playbackRoot.SetActive(false);
        RestoreControls();
    }

    void OnDestroy()
    {
        if (playbackRoot) Destroy(playbackRoot);
    }
}