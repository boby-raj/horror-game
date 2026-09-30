using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class PatrolAndChaseAI : MonoBehaviour
{
    public enum AIState
    {
        Patrol,
        Chase,
        Investigating,
        Caught
    }

    [Header("Current State (Debug)")]
    public AIState currentState = AIState.Patrol;

    [Header("Target & Detection")]
    [Tooltip("Drag your Player / Capsule here. Auto-finds by 'Player' tag if left empty.")]
    public Transform targetCharacter;
    public float detectionRadius = 12.0f;
    public float catchDistance = 1.6f;

    [Header("Vision & Line of Sight")]
    [Tooltip("Field of View angle in degrees (e.g. 110-120 allows sneaking behind)")]
    [Range(30f, 360f)]
    public float fieldOfViewAngle = 110.0f;
    [Tooltip("LayerMask for walls, doors, and furniture that block line of sight")]
    public LayerMask obstacleMask = ~0;
    [Tooltip("Height of enemy eyes above pivot for raycasting")]
    public float eyeHeight = 1.6f;
    [Tooltip("Height of player center/eyes above pivot")]
    public float playerTargetHeight = 1.2f;

    [Header("Flashlight Detection")]
    public bool detectFlashlight = true;
    public float flashlightBonusDistance = 6.0f;

    [Header("Patrol Waypoints")]
    [Tooltip("Create empty GameObjects in your scene and drag them here")]
    public Transform[] waypoints;
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 4.8f;
    public float waypointWaitTime = 1.5f;

    [Header("Investigate / Search State")]
    [Tooltip("How long enemy searches the last known player position before giving up")]
    public float searchDuration = 4.0f;
    public float searchTurnSpeed = 90.0f;

    [Header("Animation")]
    public string isChasingParam = "IsChasing";
    public string isSearchingParam = "IsSearching";

    [Header("Audio")]
    public AudioSource chaseMusicSource;
    public AudioSource jumpscareSource;

    [Header("Jumpscare & Camera FX")]
    [Tooltip("Player main camera for face-snapping and screen shake. Auto-finds if empty.")]
    public Transform playerCamera;
    [Tooltip("How close to the player's face the monster snaps during jumpscare")]
    public float faceDistance = 1.0f;
    public float vibrationIntensity = 0.25f;
    public float vibrationDuration = 1.2f;
    public float delayBeforeRespawn = 1.0f;

    private NavMeshAgent agent;
    private CharacterController characterController;
    private Animator animator;
    private CharacterController playerController;
    private MonoBehaviour playerMovementScript;

    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private float searchTimer = 0f;
    private Vector3 lastKnownPlayerPosition;
    private bool hasCaughtPlayer = false;
    private Vector3 currentDestination;
    private float verticalVelocity = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = GetComponentInChildren<NavMeshAgent>();

        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        AudioSource[] audios = GetComponents<AudioSource>();
        if (audios.Length > 0 && chaseMusicSource == null) chaseMusicSource = audios[0];
        if (audios.Length > 1 && jumpscareSource == null) jumpscareSource = audios[1];
    }

    void Start()
    {
        FindPlayerReferences();

        // If NavMeshAgent is present, try snapping to nearest NavMesh surface
        if (agent != null)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }
    }

    void OnEnable()
    {
        hasCaughtPlayer = false;
        currentState = AIState.Patrol;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.enabled = true;
            agent.speed = patrolSpeed;
        }

        SetNextWaypointDestination();
    }

    private void FindPlayerReferences()
    {
        if (targetCharacter == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                targetCharacter = playerObj.transform;
            }
        }

        if (targetCharacter != null)
        {
            playerController = targetCharacter.GetComponent<CharacterController>();
            playerMovementScript = targetCharacter.GetComponent("FirstPersonMovement") as MonoBehaviour;
        }

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        if (targetCharacter == null)
        {
            FindPlayerReferences();
            if (targetCharacter == null) return;
        }

        if (hasCaughtPlayer) return;

        float distanceToPlayer = Vector3.Distance(transform.position, targetCharacter.position);

        // 1. CATCH CHECK
        if (distanceToPlayer <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        // 2. CHECK LINE OF SIGHT & VISION
        bool canSeePlayer = CheckLineOfSight(distanceToPlayer);

        switch (currentState)
        {
            case AIState.Patrol:
                if (canSeePlayer)
                {
                    StartChasing();
                }
                else
                {
                    PatrolLogic();
                }
                break;

            case AIState.Chase:
                if (canSeePlayer)
                {
                    lastKnownPlayerPosition = targetCharacter.position;
                    MoveTo(targetCharacter.position, chaseSpeed);
                }
                else
                {
                    StartInvestigating();
                }
                break;

            case AIState.Investigating:
                if (canSeePlayer)
                {
                    StartChasing();
                }
                else
                {
                    InvestigateLogic();
                }
                break;
        }
    }

    private bool CheckLineOfSight(float distanceToPlayer)
    {
        float effectiveRadius = detectionRadius;

        if (detectFlashlight && IsPlayerFlashlightOn())
        {
            effectiveRadius += flashlightBonusDistance;
        }

        if (distanceToPlayer > effectiveRadius) return false;

        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 playerTargetPos = targetCharacter.position + Vector3.up * playerTargetHeight;
        Vector3 directionToPlayer = (playerTargetPos - eyePos).normalized;

        if (distanceToPlayer > 2.5f)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle > fieldOfViewAngle * 0.5f)
            {
                return false;
            }
        }

        if (Physics.Raycast(eyePos, directionToPlayer, out RaycastHit hit, effectiveRadius, obstacleMask))
        {
            if (hit.transform == targetCharacter || hit.transform.IsChildOf(targetCharacter))
            {
                return true;
            }
            return false;
        }

        return false;
    }

    private bool IsPlayerFlashlightOn()
    {
        Light[] lights = targetCharacter.GetComponentsInChildren<Light>();
        foreach (Light l in lights)
        {
            if (l.enabled && l.gameObject.activeInHierarchy && l.type == LightType.Spot)
            {
                return true;
            }
        }
        return false;
    }

    private void PatrolLogic()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        float dist = GetDistanceTo(currentDestination);
        if (dist <= 0.8f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waypointWaitTime)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                SetNextWaypointDestination();
                waitTimer = 0f;
            }
        }
        else
        {
            MoveTo(currentDestination, patrolSpeed);
        }
    }

    private void SetNextWaypointDestination()
    {
        if (waypoints != null && waypoints.Length > 0 && waypoints[currentWaypointIndex] != null)
        {
            currentDestination = waypoints[currentWaypointIndex].position;
            MoveTo(currentDestination, patrolSpeed);
        }
    }

    private void StartChasing()
    {
        currentState = AIState.Chase;
        lastKnownPlayerPosition = targetCharacter.position;
        MoveTo(targetCharacter.position, chaseSpeed);

        if (animator != null)
        {
            animator.SetBool(isChasingParam, true);
            if (!string.IsNullOrEmpty(isSearchingParam)) animator.SetBool(isSearchingParam, false);
        }

        if (chaseMusicSource != null && !chaseMusicSource.isPlaying)
        {
            chaseMusicSource.loop = true;
            chaseMusicSource.Play();
        }
    }

    private void StartInvestigating()
    {
        currentState = AIState.Investigating;
        searchTimer = 0f;
        currentDestination = lastKnownPlayerPosition;
        MoveTo(lastKnownPlayerPosition, patrolSpeed * 1.3f);

        if (animator != null)
        {
            animator.SetBool(isChasingParam, false);
            if (!string.IsNullOrEmpty(isSearchingParam)) animator.SetBool(isSearchingParam, true);
        }
    }

    private void InvestigateLogic()
    {
        float dist = GetDistanceTo(lastKnownPlayerPosition);
        if (dist > 0.8f)
        {
            MoveTo(lastKnownPlayerPosition, patrolSpeed * 1.3f);
            return;
        }

        searchTimer += Time.deltaTime;
        transform.Rotate(Vector3.up * Mathf.Sin(Time.time * 2f) * searchTurnSpeed * Time.deltaTime);

        if (searchTimer >= searchDuration)
        {
            StopChasingAndResumePatrol();
        }
    }

    private void StopChasingAndResumePatrol()
    {
        currentState = AIState.Patrol;

        if (animator != null)
        {
            animator.SetBool(isChasingParam, false);
            if (!string.IsNullOrEmpty(isSearchingParam)) animator.SetBool(isSearchingParam, false);
        }

        if (chaseMusicSource != null && chaseMusicSource.isPlaying)
        {
            chaseMusicSource.Stop();
        }

        SetNextWaypointDestination();
    }

    /// <summary>
    /// Smooth universal movement engine: Uses NavMeshAgent if on NavMesh,
    /// otherwise falls back to CharacterController or physics movement so the enemy
    /// works anywhere in the scene regardless of NavMesh baking.
    /// </summary>
    private void MoveTo(Vector3 targetPos, float speed)
    {
        currentDestination = targetPos;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = speed;
            agent.SetDestination(targetPos);
            return;
        }

        // Fallback: CharacterController or Transform movement
        Vector3 flatDir = targetPos - transform.position;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude > 0.04f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);

            Vector3 move = flatDir.normalized * speed;

            if (characterController != null && characterController.enabled)
            {
                if (characterController.isGrounded) verticalVelocity = -5f;
                else verticalVelocity += -20f * Time.deltaTime;

                move.y = verticalVelocity;
                characterController.Move(move * Time.deltaTime);
            }
            else
            {
                transform.position += flatDir.normalized * (speed * Time.deltaTime);
            }
        }
    }

    private float GetDistanceTo(Vector3 targetPos)
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh && !agent.pathPending)
        {
            return agent.remainingDistance;
        }

        Vector3 flatSelf = transform.position;
        flatSelf.y = 0;
        Vector3 flatTarget = targetPos;
        flatTarget.y = 0;
        return Vector3.Distance(flatSelf, flatTarget);
    }

    private void CatchPlayer()
    {
        if (hasCaughtPlayer) return;
        hasCaughtPlayer = true;
        currentState = AIState.Caught;

        StartCoroutine(ExecuteJumpscareSequence());
    }

    private IEnumerator ExecuteJumpscareSequence()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        if (playerController != null) playerController.enabled = false;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        if (chaseMusicSource != null && chaseMusicSource.isPlaying) chaseMusicSource.Stop();
        if (jumpscareSource != null) jumpscareSource.Play();

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
                Vector3 monsterHeadPos = transform.position + Vector3.up * eyeHeight;
                Vector3 lookAtMonster = (monsterHeadPos - playerCamera.position).normalized;
                if (lookAtMonster != Vector3.zero)
                {
                    playerCamera.rotation = Quaternion.LookRotation(lookAtMonster);
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = (currentState == AIState.Chase) ? Color.red :
                       (currentState == AIState.Investigating) ? Color.yellow : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        Vector3 leftRay = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * transform.forward;
        Vector3 rightRay = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePos, leftRay * detectionRadius);
        Gizmos.DrawRay(eyePos, rightRay * detectionRadius);

        if (currentState == AIState.Investigating)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(eyePos, lastKnownPlayerPosition);
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
        }
    }
}