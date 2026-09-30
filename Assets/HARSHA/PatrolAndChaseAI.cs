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

    [Header("Dynamic Hunting (When No Waypoints)")]
    [Tooltip("If no waypoints are assigned, enemy hunts / stalks towards player instead of freezing in place.")]
    public bool huntPlayerIfNoWaypoints = true;

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

    protected NavMeshAgent agent;
    protected CharacterController characterController;
    protected Animator animator;
    protected CharacterController playerController;
    protected MonoBehaviour playerMovementScript;

    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private float searchTimer = 0f;
    private Vector3 lastKnownPlayerPosition;
    private bool hasCaughtPlayer = false;
    private Vector3 currentDestination;
    private float verticalVelocity = 0f;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = GetComponentInChildren<NavMeshAgent>();

        // Ensure NavMeshAgent exists so the enemy can smoothly navigate baked NavMesh
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
            agent.radius = 0.5f;
            agent.height = 2f;
            agent.speed = patrolSpeed;
            agent.acceleration = 12f;
            agent.angularSpeed = 240f;
            agent.stoppingDistance = 0.5f;
            agent.autoBraking = false;
        }

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
        ValidateNavMeshStatus();
    }

    void OnEnable()
    {
        hasCaughtPlayer = false;
        currentState = AIState.Patrol;
        ValidateNavMeshStatus();
        SetNextWaypointDestination();
    }

    private void ValidateNavMeshStatus()
    {
        if (agent == null) return;

        // If already navigating on NavMesh, ensure CharacterController is disabled so they don't fight
        if (agent.isOnNavMesh)
        {
            if (characterController != null && characterController.enabled)
            {
                characterController.enabled = false;
            }
            return;
        }

        bool onMesh = false;
        try
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
            {
                if (!agent.enabled) agent.enabled = true;
                agent.Warp(hit.position);
                onMesh = agent.isOnNavMesh;
            }
        }
        catch
        {
            onMesh = false;
        }

        if (onMesh)
        {
            if (characterController != null && characterController.enabled)
            {
                characterController.enabled = false;
            }
        }
        else
        {
            if (agent.enabled) agent.enabled = false;
            if (characterController != null && !characterController.enabled)
            {
                characterController.enabled = true;
            }
        }
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

        RaycastHit[] hits = Physics.RaycastAll(eyePos, directionToPlayer, effectiveRadius, obstacleMask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            if (hit.collider.isTrigger)
                continue;

            if (hit.transform == targetCharacter || hit.transform.IsChildOf(targetCharacter))
            {
                return true;
            }

            // Solid obstacle hit
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
        if (waypoints == null || waypoints.Length == 0 || waypoints[0] == null)
        {
            if (huntPlayerIfNoWaypoints && targetCharacter != null)
            {
                // Stalk towards player's position
                MoveTo(targetCharacter.position, patrolSpeed);
            }
            return;
        }

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

        SetAnimBool(isChasingParam, true);
        SetAnimBool(isSearchingParam, false);

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

        SetAnimBool(isChasingParam, false);
        SetAnimBool(isSearchingParam, true);
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

        SetAnimBool(isChasingParam, false);
        SetAnimBool(isSearchingParam, false);

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
            if (characterController != null && characterController.enabled)
            {
                characterController.enabled = false;
            }
            agent.isStopped = false;
            agent.speed = speed;
            agent.SetDestination(targetPos);
            return;
        }

        // If outside NavMesh, periodically check if we can re-dock onto a baked NavMesh
        if (agent != null && !agent.isOnNavMesh && Time.frameCount % 60 == 0)
        {
            ValidateNavMeshStatus();
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.speed = speed;
                agent.SetDestination(targetPos);
                return;
            }
        }

        // Fallback: CharacterController or Transform movement
        if (characterController != null && !characterController.enabled)
        {
            characterController.enabled = true;
        }
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
                Vector3 newPos = transform.position + flatDir.normalized * (speed * Time.deltaTime);
                if (Physics.Raycast(newPos + Vector3.up * 1.5f, Vector3.down, out RaycastHit hit, 3.0f, obstacleMask))
                {
                    newPos.y = hit.point.y;
                }
                transform.position = newPos;
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

    private bool HasParameter(string paramName)
    {
        if (animator == null || string.IsNullOrEmpty(paramName)) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == paramName) return true;
        }
        return false;
    }

    private void SetAnimBool(string paramName, bool val)
    {
        if (HasParameter(paramName))
        {
            animator.SetBool(paramName, val);
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