using System.Linq;
using UnityEngine;

public class FirstPersonAudio : MonoBehaviour
{
    public FirstPersonMovement character;

    [Header("Step")]
    public AudioSource stepAudio;
    public AudioSource runningAudio;
    public float velocityThreshold = .01f;
    Vector2 lastCharacterPosition;
    Vector2 CurrentCharacterPosition => new Vector2(character.transform.position.x, character.transform.position.z);

    [Header("Landing")]
    public AudioSource landingAudio;
    public AudioClip[] landingSFX;

    [Header("Jump")]
    public AudioSource jumpAudio;
    public AudioClip[] jumpSFX;

    [Header("Crouch")]
    public AudioSource crouchStartAudio, crouchedAudio, crouchEndAudio;
    public AudioClip[] crouchStartSFX, crouchEndSFX;

    AudioSource[] MovingAudios => new AudioSource[] { stepAudio, runningAudio, crouchedAudio };

    void Reset()
    {
        character = GetComponentInParent<FirstPersonMovement>();
        stepAudio = GetOrCreateAudioSource("Step Audio");
        runningAudio = GetOrCreateAudioSource("Running Audio");
        landingAudio = GetOrCreateAudioSource("Landing Audio");
        jumpAudio = GetOrCreateAudioSource("Jump audio");
        crouchStartAudio = GetOrCreateAudioSource("Crouch Start Audio");
        crouchedAudio = GetOrCreateAudioSource("Crouched Audio");
        crouchEndAudio = GetOrCreateAudioSource("Crouch End Audio");
    }

    void OnEnable() => SubscribeToEvents();
    void OnDisable() => UnsubscribeToEvents();

    void FixedUpdate()
    {
        if (!character) return;

        float velocity = Vector3.Distance(CurrentCharacterPosition, lastCharacterPosition);
        if (velocity >= velocityThreshold && character.isGrounded)
        {
            if (character.IsCrouched)
            {
                SetPlayingMovingAudio(crouchedAudio);
            }
            else if (character.IsRunning)
            {
                SetPlayingMovingAudio(runningAudio);
            }
            else
            {
                SetPlayingMovingAudio(stepAudio);
            }
        }
        else
        {
            SetPlayingMovingAudio(null);
        }

        lastCharacterPosition = CurrentCharacterPosition;
    }

    void SetPlayingMovingAudio(AudioSource audioToPlay)
    {
        foreach (var audio in MovingAudios.Where(audio => audio != audioToPlay && audio != null))
        {
            audio.Pause();
        }

        if (audioToPlay && !audioToPlay.isPlaying)
        {
            audioToPlay.Play();
        }
    }

    void PlayLandingAudio() => PlayRandomClip(landingAudio, landingSFX);
    void PlayJumpAudio() => PlayRandomClip(jumpAudio, jumpSFX);
    void PlayCrouchStartAudio() => PlayRandomClip(crouchStartAudio, crouchStartSFX);
    void PlayCrouchEndAudio() => PlayRandomClip(crouchEndAudio, crouchEndSFX);

    void SubscribeToEvents()
    {
        if (character)
        {
            character.Grounded += PlayLandingAudio;
            character.Jumped += PlayJumpAudio;
            character.CrouchStart += PlayCrouchStartAudio;
            character.CrouchEnd += PlayCrouchEndAudio;
        }
    }

    void UnsubscribeToEvents()
    {
        if (character)
        {
            character.Grounded -= PlayLandingAudio;
            character.Jumped -= PlayJumpAudio;
            character.CrouchStart -= PlayCrouchStartAudio;
            character.CrouchEnd -= PlayCrouchEndAudio;
        }
    }

    AudioSource GetOrCreateAudioSource(string name)
    {
        AudioSource result = System.Array.Find(GetComponentsInChildren<AudioSource>(), a => a.name == name);
        if (result) return result;

        result = new GameObject(name).AddComponent<AudioSource>();
        result.spatialBlend = 1;
        result.playOnAwake = false;
        result.transform.SetParent(transform, false);
        return result;
    }

    static void PlayRandomClip(AudioSource audio, AudioClip[] clips)
    {
        if (!audio || clips.Length <= 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clips.Length > 1)
            while (clip == audio.clip)
                clip = clips[Random.Range(0, clips.Length)];

        audio.clip = clip;
        audio.Play();
    }
}
