using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RampFollower : MonoBehaviour
{
    [Header("Player References")]
    public Transform player;
    public CharacterController playerController;
    public MonoBehaviour playerMovementScript;
    public Transform playerCamera;

    [Header("Movement Tuning")]
    public float speed = 6.0f;
    public float rotationSpeed = 12.0f;
    public float stickToGroundForce = -15.0f;
    public LayerMask groundLayer = ~0;

    [Header("Catch & Vibration Settings")]
    public float catchDistance = 2.5f;
    [Tooltip("How close to the player's face the ghost snaps (1.0 = right in your face)")]
    public float faceDistance = 1.0f;
    [Tooltip("How violent the camera shake vibration is")]
    public float vibrationIntensity = 0.25f;
    public float vibrationDuration = 1.2f;
    public float delayBeforeRespawn = 1.0f;

    [Header("Audio")]
    public AudioSource chaseAudioSource;
    public AudioSource jumpscareAudioSource;

    private CharacterController controller;
    private float verticalVelocity = 0f;
    private bool isCaught = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {

        if (chaseAudioSource != null && !chaseAudioSource.isPlaying)
        {
            chaseAudioSource.loop = true;
            chaseAudioSource.Play();
        }
    }

    void Update()
    {
        if (player == null || controller == null || isCaught) return;

        Vector3 flatDirection = (player.position - transform.position);
        flatDirection.y = 0f;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= catchDistance)
        {
            StartCoroutine(ExecuteCatchSequence());
            return;
        }

        if (flatDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);

        Vector3 moveDirection = flatDirection.normalized;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 1.5f, groundLayer))
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal).normalized;
        }

        Vector3 finalVelocity = moveDirection * speed;

        if (controller.isGrounded)
        {
            verticalVelocity = stickToGroundForce;
        }
        else
        {
            verticalVelocity += -20.0f * Time.deltaTime;
        }

        finalVelocity.y += verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private IEnumerator ExecuteCatchSequence()
    {
        isCaught = true;

        if (controller != null) controller.enabled = false;

        if (playerController != null) playerController.enabled = false;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        if (chaseAudioSource != null) chaseAudioSource.Stop();
        if (jumpscareAudioSource != null) jumpscareAudioSource.Play();

        if (playerCamera != null)
        {

            Vector3 facePosition = playerCamera.position + (playerCamera.forward * faceDistance);
            facePosition.y = playerCamera.position.y - 0.4f;
            transform.position = facePosition;

            Vector3 lookDir = (playerCamera.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        float timer = 0f;
        Vector3 originalCamPos = playerCamera != null ? playerCamera.localPosition : Vector3.zero;

        while (timer < vibrationDuration)
        {
            timer += Time.deltaTime;

            if (playerCamera != null)
            {

                Vector3 ghostFacePos = transform.position + Vector3.up * 1.5f;
                Vector3 lookAtGhost = (ghostFacePos - playerCamera.position).normalized;
                if (lookAtGhost != Vector3.zero)
                {
                    playerCamera.rotation = Quaternion.LookRotation(lookAtGhost);
                }

                playerCamera.localPosition = originalCamPos + Random.insideUnitSphere * vibrationIntensity;
            }

            yield return null;
        }

        if (playerCamera != null) playerCamera.localPosition = originalCamPos;

        yield return new WaitForSeconds(delayBeforeRespawn);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RespawnPlayer();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }
}