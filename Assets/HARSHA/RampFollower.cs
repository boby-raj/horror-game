using UnityEngine;

/// <summary>
/// Backward-compatible wrapper for RampFollower referenced in SampleScene on Walking.
/// Inherits full AI, vision cone, jumpscare, and hybrid movement from PatrolAndChaseAI.
/// </summary>
public class RampFollower : PatrolAndChaseAI
{
    [Header("RampFollower Bindings (Auto-maps to PatrolAndChaseAI)")]
    public Transform player;
    public float speed = 4.8f;
    public float rotationSpeed = 12f;
    public float stickToGroundForce = -15f;
    public LayerMask groundLayer = ~0;
    public AudioSource chaseAudioSource;
    public AudioSource jumpscareAudioSource;

    protected override void Awake()
    {
        if (player != null && targetCharacter == null)
            targetCharacter = player;

        if (speed > 0f)
            chaseSpeed = speed;

        if (chaseAudioSource != null && chaseMusicSource == null)
            chaseMusicSource = chaseAudioSource;

        if (jumpscareAudioSource != null && jumpscareSource == null)
            jumpscareSource = jumpscareAudioSource;

        base.Awake();
    }
}
