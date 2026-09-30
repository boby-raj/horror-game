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
    private Animator animator;
    private CharacterController playerController;
    private MonoBehaviour playerMovementScript;

    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private float searchTimer = 0f;
    private Vector3 lastKnownPlayerPosition;
    private bool hasCaughtPlayer = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        FindPlayerReferences();
    }

    void OnEnable()
    {
        hasCaughtPlayer = false;
        currentState = AIState.Patrol;

        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = patrolSpeed;

            // Snap onto nearest NavMesh floor on spawn
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
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

        if (agent == null || !agent.isOnNavMesh || hasCaughtPlayer) return;

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
                    agent.SetDestination(targetCharacter.position);
                }
                else
                {
                    // Lost visual contact -> Transition to Investigating Last Known Position
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

    /// <summary>
    /// Checks distance, field of view angle, and raycast obstacle occlusion.
    /// </summary>
    private bool CheckLineOfSight(float distanceToPlayer)
    {
        float effectiveRadius = detectionRadius;

        // If player has flashlight active, boost vision range
        if (detectFlashlight && IsPlayerFlashlightOn())
        {
            effectiveRadius += flashlightBonusDistance;
        }

        // 1. Distance check
        if (distanceToPlayer > effectiveRadius) return false;

        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 playerTargetPos = targetCharacter.position + Vector3.up * playerTargetHeight;
        Vector3 directionToPlayer = (playerTargetPos - eyePos).normalized;

        // 2. Field of View (FOV) Angle check
        // Allow close-range detection even if slightly behind (e.g. within 2.5m)
        if (distanceToPlayer > 2.5f)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle > fieldOfViewAngle * 0.5f)
            {
                return false; // Player is behind the enemy's vision cone
            }
        }

        // 3. Raycast line of sight check against obstacles (walls, doors)
        if (Physics.Raycast(eyePos, directionToPlayer, out RaycastHit hit, effectiveRadius, obstacleMask))
        {
            // If the raycast hit the player or a child of the player, line of sight is clear!
            if (hit.transform == targetCharacter || hit.transform.IsChildOf(targetCharacter))
            {
                return true;
            }

            // Raycast hit a wall/door instead -> blocked!
            return false;
        }

        return false;
    }

    private bool IsPlayerFlashlightOn()
    {
        // Searches for active light on player
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

        // Check if reached current waypoint
        if (!agent.pathPending && agent.remainingDistance <= 0.6f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waypointWaitTime)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                SetNextWaypointDestination();
                waitTimer = 0f;
            }
        }
    }

    private void SetNextWaypointDestination()
    {
        if (waypoints != null && waypoints.Length > 0 && waypoints[currentWaypointIndex] != null)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void StartChasing()
    {
        currentState = AIState.Chase;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        lastKnownPlayerPosition = targetCharacter.position;
        agent.SetDestination(targetCharacter.position);

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
        agent.speed = patrolSpeed * 1.3f;
        agent.isStopped = false;

        // Move to the exact spot where the player was last seen
        agent.SetDestination(lastKnownPlayerPosition);

        if (animator != null)
        {
            animator.SetBool(isChasingParam, false);
            if (!string.IsNullOrEmpty(isSearchingParam)) animator.SetBool(isSearchingParam, true);
        }
    }

    private void InvestigateLogic()
    {
        // While moving to the last known spot
        if (agent.remainingDistance > 0.8f)
        {
            return;
        }

        // Arrived at last known spot -> look around
        searchTimer += Time.deltaTime;

        // Slowly pivot left and right looking for player
        transform.Rotate(Vector3.up * Mathf.Sin(Time.time * 2f) * searchTurnSpeed * Time.deltaTime);

        if (searchTimer >= searchDuration)
        {
            // Give up search and resume normal patrol
            StopChasingAndResumePatrol();
        }
    }

    private void StopChasingAndResumePatrol()
    {
        currentState = AIState.Patrol;
        agent.speed = patrolSpeed;
        agent.isStopped = false;

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

    private void CatchPlayer()
    {
        if (hasCaughtPlayer) return;
        hasCaughtPlayer = true;
        currentState = AIState.Caught;

        StartCoroutine(ExecuteJumpscareSequence());
    }

    private IEnumerator ExecuteJumpscareSequence()
    {
        // 1. Freeze enemy movement
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        // 2. Freeze Player Controls
        if (playerController != null) playerController.enabled = false;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        // 3. Audio Transition
        if (chaseMusicSource != null && chaseMusicSource.isPlaying) chaseMusicSource.Stop();
        if (jumpscareSource != null) jumpscareSource.Play();

        // 4. Snap Monster right in front of player's face
        if (playerCamera != null)
        {
            Vector3 facePosition = playerCamera.position + (playerCamera.forward * faceDistance);
            facePosition.y = playerCamera.position.y - 0.4f;
            transform.position = facePosition;

            // Rotate monster to face camera
            Vector3 lookDir = (playerCamera.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        // 5. Camera Lock & Violent Screen Vibration
        float timer = 0f;
        Vector3 originalCamPos = playerCamera != null ? playerCamera.localPosition : Vector3.zero;

        while (timer < vibrationDuration)
        {
            timer += Time.deltaTime;

            if (playerCamera != null)
            {
                // Lock camera gaze directly onto the monster's eyes
                Vector3 monsterHeadPos = transform.position + Vector3.up * eyeHeight;
                Vector3 lookAtMonster = (monsterHeadPos - playerCamera.position).normalized;
                if (lookAtMonster != Vector3.zero)
                {
                    playerCamera.rotation = Quaternion.LookRotation(lookAtMonster);
                }

                // Intense vibration shake
                playerCamera.localPosition = originalCamPos + Random.insideUnitSphere * vibrationIntensity;
            }

            yield return null;
        }

        if (playerCamera != null) playerCamera.localPosition = originalCamPos;

        yield return new WaitForSeconds(delayBeforeRespawn);

        // 6. Respawn or Reload
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

        // Draw Detection Radius
        Gizmos.color = (currentState == AIState.Chase) ? Color.red :
                       (currentState == AIState.Investigating) ? Color.yellow : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw Catch Distance
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        // Draw Vision Cone FOV Lines
        Vector3 leftRayDirection = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * transform.forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePos, leftRayDirection * detectionRadius);
        Gizmos.DrawRay(eyePos, rightRayDirection * detectionRadius);

        // Draw line to last known position if investigating
        if (currentState == AIState.Investigating)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(eyePos, lastKnownPlayerPosition);
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
        }
    }
}