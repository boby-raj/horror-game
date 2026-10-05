using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class PatrolAndChaseAi: MonoBehaviour
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
    [Tooltip("Drag your Player here. Auto-finds by 'Player' tag if left empty.")]
    public Transform targetCharacter;
    [Tooltip("How far the enemy can see the player while patrolling / investigating")]
    public float detectionRadius = 12.0f;
    [Tooltip("Horizontal distance at which the player is caught")]
    public float catchDistance = 2.5f;
    [Tooltip("Max height difference allowed for a catch (stops catches through floors/ceilings)")]
    public float maxVerticalCatchDifference = 3.5f;

    [Header("Vision & Line of Sight")]
    [Range(30f, 360f)]
    [Tooltip("Field of view in degrees. Only used when NOT chasing.")]
    public float fieldOfViewAngle = 110.0f;
    [Tooltip("Inside this distance the enemy notices you from any angle (still needs a clear line)")]
    public float closeSenseRange = 3.0f;
    [Tooltip("Layers that block sight: walls, doors, furniture. Include the player layer or leave as Everything.")]
    public LayerMask obstacleMask = ~0;
    [Tooltip("Eye height above pivot (scaled by the enemy's Y scale)")]
    public float eyeHeight = 1.6f;
    [Tooltip("Height of the player's body point the enemy looks at")]
    public float playerTargetHeight = 1.2f;
    [Tooltip("How often (seconds) vision is evaluated. 0.1 is plenty and cheap.")]
    public float senseInterval = 0.1f;

    [Header("Flashlight Detection")]
    public bool detectFlashlight = true;
    public float flashlightBonusDistance = 6.0f;
    [Tooltip("Optional: assign the player's flashlight Light. Auto-finds a Spot Light under the player if empty.")]
    public Light playerFlashlight;

    [Header("Patrol Waypoints")]
    public Transform[] waypoints;
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 4.8f;
    public float waypointWaitTime = 1.5f;

    [Header("Chase & Escape Tuning")]
    [Tooltip("While chasing, the enemy can see the player up to this distance, from any angle")]
    public float loseDistance = 18.0f;
    [Tooltip("Seconds the enemy keeps chasing after losing sight before it starts investigating")]
    public float loseSightGrace = 2.0f;

    [Header("Hearing")]
    [Tooltip("Enemy reacts to HearNoise() calls while patrolling or investigating")]
    public bool canHear = true;

    [Header("Random Roaming (When No Waypoints)")]
    public bool randomRoamIfNoWaypoints = true;
    public float roamRadius = 15.0f;
    public float roamWaitTime = 2.0f;
    [Tooltip("Give up on a roam point after this many seconds")]
    public float roamTimeout = 12.0f;

    [Header("Investigate / Search State")]
    public float searchDuration = 4.0f;
    public float searchTurnSpeed = 90.0f;
    [Tooltip("Hard limit for the whole Investigating state, so the enemy can never get stuck")]
    public float investigateTimeout = 15.0f;

    [Header("Movement")]
    [Tooltip("How close to the destination counts as 'arrived' (added to agent stopping distance)")]
    public float arriveTolerance = 0.4f;
    [Tooltip("Minimum seconds between SetDestination calls")]
    public float destinationUpdateInterval = 0.2f;

    [Header("Animation")]
    public string isChasingParam = "IsChasing";
    public string isSearchingParam = "IsSearching";
    [Tooltip("Optional float parameter fed with the agent's speed. Leave empty or missing to skip.")]
    public string speedParam = "Speed";
    [Tooltip("Optional trigger played when the player is caught. Leave empty or missing to skip.")]
    public string caughtTriggerParam = "Caught";
    public float chaseAnimatorSpeed = 1.6f;

    [Header("Audio")]
    [Tooltip("Looping chase music. Assign this explicitly in the Inspector.")]
    public AudioSource chaseMusicSource;
    public float chaseMusicFadeSpeed = 2.0f;
    public AudioSource jumpscareSource;
    public AudioClip jumpscareClip;

    [Header("Jumpscare & Camera FX")]
    [Tooltip("Player camera. Auto-finds Camera.main if empty.")]
    public Transform playerCamera;
    public float faceDistance = 1.2f;
    [Tooltip("Violent position shake intensity")]
    public float vibrationIntensity = 0.45f;
    [Tooltip("Angular rotational shake intensity (degrees) for violent head trauma impact")]
    public float angularShakeIntensity = 7.0f;
    public float vibrationDuration = 1.6f;
    public float delayBeforeRespawn = 1.0f;
    [Tooltip("Show red bloody vignette attack flash on screen during jumpscare")]
    public bool showBloodAttackEffect = true;
    [Tooltip("Secondary attack impact / hit audio clip")]
    public AudioClip attackHitClip;
    [Tooltip("Extra scripts to disable while the jumpscare plays (movement, look, flashlight...). Restored after respawn.")]
    public MonoBehaviour[] disableOnCatch;

    [Header("Events")]
    public UnityEvent onChaseStarted;
    public UnityEvent onChaseEnded;
    public UnityEvent onPlayerCaught;

    private NavMeshAgent agent;
    private Animator animator;

    private CharacterController playerController;
    private Behaviour playerMoveScript;
    private Behaviour cameraLookScript;

    private readonly List<Behaviour> disabledByCatch = new List<Behaviour>();
    private bool playerControllerWasEnabled;

    private readonly RaycastHit[] hitBuffer = new RaycastHit[16];

    private int chasingHash, searchingHash, speedHash, caughtHash;
    private bool hasChasingParam, hasSearchingParam, hasSpeedParam, hasCaughtParam;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool initialized;
    private bool hasCaughtPlayer;

    private bool canSeePlayer;
    private float senseTimer;
    private float lostSightTimer;
    private Vector3 lastKnownPlayerPosition;

    private Vector3 lastRequestedDestination;
    private bool hasRequestedDestination;
    private float lastDestinationTime = -10f;

    private int currentWaypointIndex;
    private bool waitingAtPoint;
    private float waitTimer;
    private Vector3 roamDestination;
    private bool hasRoamDestination;
    private float roamTimer;

    private float searchTimer;
    private float investigateTimer;
    private bool searching;

    private float chaseMusicMaxVolume = 1f;

    private float ScaleY => transform.lossyScale.y > 0.01f ? transform.lossyScale.y : 1f;
    private Vector3 EyePosition => transform.position + Vector3.up * (eyeHeight * ScaleY);

    private void Awake()
    {
        CharacterController enemyCC = GetComponent<CharacterController>();
        if (enemyCC != null) enemyCC.enabled = false;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.baseOffset = 0f;

        animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;

        startPosition = transform.position;
        startRotation = transform.rotation;

        CacheAnimatorParameters();

        if (jumpscareSource == null)
        {
            jumpscareSource = gameObject.AddComponent<AudioSource>();
            jumpscareSource.playOnAwake = false;
            jumpscareSource.spatialBlend = 0f;
        }

        if (chaseMusicSource != null)
        {
            chaseMusicMaxVolume = chaseMusicSource.volume;
            chaseMusicSource.loop = true;
            chaseMusicSource.volume = 0f;
        }
    }

    private void Start()
    {
        FindPlayerReferences();
        EnsureOnNavMesh();
        ResetBrain();
        initialized = true;
    }

    private void OnEnable()
    {

        if (initialized)
        {
            EnsureOnNavMesh();
            ResetBrain();
        }
    }

    private void Update()
    {
        if (hasCaughtPlayer) return;

        if (targetCharacter == null)
        {
            FindPlayerReferences();
            if (targetCharacter == null) return;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
        {

            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit groundHit, 50f, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                if (transform.position.y > groundHit.point.y + 0.05f)
                {
                    Vector3 targetFloor = new Vector3(transform.position.x, groundHit.point.y, transform.position.z);
                    transform.position = Vector3.MoveTowards(transform.position, targetFloor, 12f * Time.deltaTime);
                }
            }

            if (Time.frameCount % 30 == 0)
            {
                EnsureOnNavMesh();
            }
            return;
        }

        if (IsPlayerInCatchRange())
        {
            CatchPlayer();
            return;
        }

        senseTimer -= Time.deltaTime;
        if (senseTimer <= 0f)
        {
            senseTimer = senseInterval;
            canSeePlayer = EvaluateSight();
        }

        switch (currentState)
        {
            case AIState.Patrol: UpdatePatrol(); break;
            case AIState.Chase: UpdateChase(); break;
            case AIState.Investigating: UpdateInvestigate(); break;
        }

        UpdateAnimator();
        UpdateChaseMusic();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCaughtPlayer) return;
        if (IsPlayerTransform(other.transform)) CatchPlayer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCaughtPlayer) return;
        if (IsPlayerTransform(collision.transform)) CatchPlayer();
    }

    private void CacheAnimatorParameters()
    {
        chasingHash = Animator.StringToHash(isChasingParam);
        searchingHash = Animator.StringToHash(isSearchingParam);
        speedHash = Animator.StringToHash(speedParam);
        caughtHash = Animator.StringToHash(caughtTriggerParam);

        if (animator == null) return;

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == isChasingParam && p.type == AnimatorControllerParameterType.Bool) hasChasingParam = true;
            else if (p.name == isSearchingParam && p.type == AnimatorControllerParameterType.Bool) hasSearchingParam = true;
            else if (p.name == speedParam && p.type == AnimatorControllerParameterType.Float) hasSpeedParam = true;
            else if (p.name == caughtTriggerParam && p.type == AnimatorControllerParameterType.Trigger) hasCaughtParam = true;
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
            else
            {
                jump jumpScript = Object.FindAnyObjectByType<jump>();
                if (jumpScript != null) targetCharacter = jumpScript.transform;
            }
        }

        if (targetCharacter != null)
        {
            playerController = targetCharacter.GetComponent<CharacterController>();
            if (playerMoveScript == null) playerMoveScript = targetCharacter.GetComponent<jump>();

            if (playerFlashlight == null)
            {
                foreach (Light l in targetCharacter.GetComponentsInChildren<Light>(true))
                {
                    if (l.type == LightType.Spot)
                    {
                        playerFlashlight = l;
                        break;
                    }
                }
            }
        }

        if (playerCamera == null)
        {
            if (Camera.main != null) playerCamera = Camera.main.transform;
            else if (targetCharacter != null)
            {
                Camera cam = targetCharacter.GetComponentInChildren<Camera>();
                if (cam != null) playerCamera = cam.transform;
            }
        }

        if (playerCamera != null && cameraLookScript == null)
        {
            cameraLookScript = playerCamera.GetComponent<mouselook>();
        }
    }

    private void EnsureOnNavMesh()
    {
        CharacterController enemyCC = GetComponent<CharacterController>();
        if (enemyCC != null && enemyCC.enabled) enemyCC.enabled = false;

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.baseOffset = 0f;
        if (!agent.enabled) agent.enabled = true;

        if (agent.isOnNavMesh) return;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return;
        }

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit groundHit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (NavMesh.SamplePosition(groundHit.point, out NavMeshHit groundNavHit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(groundNavHit.position);
                return;
            }
            transform.position = groundHit.point;
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit fallbackHit, 50f, NavMesh.AllAreas))
        {
            agent.Warp(fallbackHit.position);
        }
        else
        {
            Debug.LogError($"[PatrolAndChaseAI] '{name}' is not on a NavMesh. Bake the NavMesh and place the enemy on it.", this);
        }
    }

    private void ResetBrain()
    {
        hasCaughtPlayer = false;
        currentState = AIState.Patrol;
        canSeePlayer = false;
        lostSightTimer = 0f;
        waitingAtPoint = false;
        waitTimer = 0f;
        hasRoamDestination = false;
        searching = false;
        ClearDestinationCache();

        SetAnimBool(chasingHash, hasChasingParam, false);
        SetAnimBool(searchingHash, hasSearchingParam, false);
        if (animator != null) animator.speed = 1f;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }
    }

    public void HearNoise(Vector3 position, float radius)
    {
        if (!canHear || hasCaughtPlayer || currentState == AIState.Chase || currentState == AIState.Caught) return;
        if (Vector3.Distance(transform.position, position) > radius) return;

        lastKnownPlayerPosition = SnapToNavMesh(position);
        StartInvestigating();
    }

    public void ResetEnemy()
    {
        StopAllCoroutines();
        CharacterController enemyCC = GetComponent<CharacterController>();
        if (enemyCC != null && enemyCC.enabled) enemyCC.enabled = false;

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.baseOffset = 0f;
            if (!agent.enabled) agent.enabled = true;
        }

        if (agent != null && agent.isOnNavMesh) agent.Warp(startPosition);
        else transform.position = startPosition;
        transform.rotation = startRotation;

        EnsureOnNavMesh();
        ResetBrain();
    }

    private bool EvaluateSight()
    {
        if (targetCharacter == null) return false;

        bool chasing = currentState == AIState.Chase;
        float distance = Vector3.Distance(transform.position, targetCharacter.position);

        float range = chasing ? loseDistance : detectionRadius;
        if (!chasing && detectFlashlight && IsFlashlightOn()) range += flashlightBonusDistance;
        if (distance > range) return false;

        Vector3 eye = EyePosition;
        Vector3 targetPoint = targetCharacter.position + Vector3.up * playerTargetHeight;

        if (!chasing && distance > closeSenseRange)
        {
            Vector3 dir = (targetPoint - eye).normalized;
            if (Vector3.Angle(transform.forward, dir) > fieldOfViewAngle * 0.5f) return false;
        }

        return HasClearLine(eye, targetPoint);
    }

    private bool HasClearLine(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.01f) return true;
        dir /= dist;

        int count = Physics.RaycastNonAlloc(from, dir, hitBuffer, dist, obstacleMask, QueryTriggerInteraction.Ignore);

        float nearest = float.MaxValue;
        Transform nearestTransform = null;

        for (int i = 0; i < count; i++)
        {
            Transform t = hitBuffer[i].transform;
            if (t == transform || t.IsChildOf(transform)) continue;

            if (hitBuffer[i].distance < nearest)
            {
                nearest = hitBuffer[i].distance;
                nearestTransform = t;
            }
        }

        if (nearestTransform == null) return true;
        if (IsPlayerTransform(nearestTransform)) return true;

        return nearest >= dist - 0.4f;
    }

    private bool IsFlashlightOn()
    {
        return playerFlashlight != null && playerFlashlight.enabled && playerFlashlight.gameObject.activeInHierarchy;
    }

    private bool IsPlayerTransform(Transform t)
    {
        if (t == null) return false;
        if (targetCharacter != null && (t == targetCharacter || t.IsChildOf(targetCharacter))) return true;
        return t.CompareTag("Player") || (t.parent != null && t.parent.CompareTag("Player"));
    }

    private bool IsPlayerInCatchRange()
    {
        if (targetCharacter == null) return false;
        Vector3 delta = targetCharacter.position - transform.position;

        if (Mathf.Abs(delta.y) > maxVerticalCatchDifference) return false;

        delta.y = 0f;
        float flat = delta.magnitude;
        if (flat > catchDistance) return false;

        if (flat <= 2.2f) return true;
        return HasClearLine(EyePosition, targetCharacter.position + Vector3.up * playerTargetHeight);
    }

    private void UpdatePatrol()
    {
        if (canSeePlayer)
        {
            StartChasing();
            return;
        }

        agent.speed = patrolSpeed;

        if (HasValidWaypoints()) PatrolWaypoints();
        else if (randomRoamIfNoWaypoints) PatrolRoam();
    }

    private void UpdateChase()
    {
        if (canSeePlayer)
        {
            lostSightTimer = 0f;
            lastKnownPlayerPosition = targetCharacter.position;
        }
        else
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= loseSightGrace)
            {
                lastKnownPlayerPosition = SnapToNavMesh(lastKnownPlayerPosition);
                StartInvestigating();
                return;
            }
        }

        MoveTo(canSeePlayer ? targetCharacter.position : lastKnownPlayerPosition, chaseSpeed);
    }

    private void UpdateInvestigate()
    {
        if (canSeePlayer)
        {
            StartChasing();
            return;
        }

        investigateTimer += Time.deltaTime;
        if (investigateTimer >= investigateTimeout)
        {
            StopChasingAndResumePatrol();
            return;
        }

        if (!searching)
        {
            MoveTo(lastKnownPlayerPosition, patrolSpeed * 1.3f);

            if (HasArrived())
            {
                searching = true;
                searchTimer = 0f;
                agent.isStopped = true;
                SetAnimBool(searchingHash, hasSearchingParam, true);
            }
            return;
        }

        searchTimer += Time.deltaTime;
        transform.Rotate(Vector3.up, Mathf.Sin(Time.time * 2f) * searchTurnSpeed * Time.deltaTime);

        if (searchTimer >= searchDuration) StopChasingAndResumePatrol();
    }

    private void StartChasing()
    {
        bool wasChasing = currentState == AIState.Chase;

        currentState = AIState.Chase;
        searching = false;
        hasRoamDestination = false;
        waitingAtPoint = false;
        lostSightTimer = 0f;
        lastKnownPlayerPosition = targetCharacter.position;
        ClearDestinationCache();

        agent.isStopped = false;
        MoveTo(targetCharacter.position, chaseSpeed);

        SetAnimBool(chasingHash, hasChasingParam, true);
        SetAnimBool(searchingHash, hasSearchingParam, false);
        if (animator != null) animator.speed = chaseAnimatorSpeed;

        if (!wasChasing) onChaseStarted?.Invoke();
    }

    private void StartInvestigating()
    {
        bool wasChasing = currentState == AIState.Chase;

        currentState = AIState.Investigating;
        searching = false;
        searchTimer = 0f;
        investigateTimer = 0f;
        hasRoamDestination = false;
        waitingAtPoint = false;
        ClearDestinationCache();

        agent.isStopped = false;

        SetAnimBool(chasingHash, hasChasingParam, false);
        SetAnimBool(searchingHash, hasSearchingParam, false);
        if (animator != null) animator.speed = 1f;

        if (wasChasing) onChaseEnded?.Invoke();
    }

    private void StopChasingAndResumePatrol()
    {
        currentState = AIState.Patrol;
        searching = false;
        hasRoamDestination = false;
        waitingAtPoint = false;
        waitTimer = 0f;
        ClearDestinationCache();

        agent.isStopped = false;

        SetAnimBool(chasingHash, hasChasingParam, false);
        SetAnimBool(searchingHash, hasSearchingParam, false);
        if (animator != null) animator.speed = 1f;
    }

    private bool HasValidWaypoints()
    {
        if (waypoints == null) return false;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null) return true;
        }
        return false;
    }

    private void PatrolWaypoints()
    {

        int safety = waypoints.Length;
        while (waypoints[currentWaypointIndex] == null && safety-- > 0)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        if (waitingAtPoint)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waypointWaitTime)
            {
                waitingAtPoint = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                ClearDestinationCache();
                agent.isStopped = false;
            }
            return;
        }

        MoveTo(waypoints[currentWaypointIndex].position, patrolSpeed);

        if (HasArrived())
        {
            waitingAtPoint = true;
            waitTimer = 0f;
            agent.isStopped = true;
        }
    }

    private void PatrolRoam()
    {
        if (waitingAtPoint)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= roamWaitTime)
            {
                waitingAtPoint = false;
                hasRoamDestination = false;
                agent.isStopped = false;
            }
            return;
        }

        if (!hasRoamDestination)
        {
            if (!TryPickRoamDestination(out roamDestination)) return;
            hasRoamDestination = true;
            roamTimer = 0f;
            ClearDestinationCache();
        }

        roamTimer += Time.deltaTime;
        MoveTo(roamDestination, patrolSpeed);

        if (HasArrived() || roamTimer >= roamTimeout)
        {
            waitingAtPoint = true;
            waitTimer = 0f;
            agent.isStopped = true;
        }
    }

    private bool TryPickRoamDestination(out Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();

        for (int i = 0; i < 15; i++)
        {
            Vector3 random = transform.position + Random.insideUnitSphere * roamRadius;
            random.y = transform.position.y;

            if (!NavMesh.SamplePosition(random, out NavMeshHit hit, roamRadius * 0.5f, NavMesh.AllAreas)) continue;
            if (Vector3.Distance(transform.position, hit.position) < 3f) continue;

            if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                destination = hit.position;
                return true;
            }
        }

        destination = transform.position;
        return false;
    }

    private void MoveTo(Vector3 position, float speed)
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        agent.isStopped = false;
        agent.speed = speed;

        bool moved = !hasRequestedDestination || (position - lastRequestedDestination).sqrMagnitude > 0.25f;
        bool intervalPassed = Time.time - lastDestinationTime >= destinationUpdateInterval;

        if (moved && intervalPassed)
        {
            agent.SetDestination(SnapToNavMesh(position));
            lastRequestedDestination = position;
            lastDestinationTime = Time.time;
            hasRequestedDestination = true;
        }
    }

    private bool HasArrived()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return true;
        if (!hasRequestedDestination) return false;
        if (agent.pathPending) return false;
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid) return true;

        return agent.remainingDistance <= agent.stoppingDistance + arriveTolerance;
    }

    private void ClearDestinationCache()
    {
        hasRequestedDestination = false;
        lastDestinationTime = -10f;
    }

    private Vector3 SnapToNavMesh(Vector3 position)
    {
        return NavMesh.SamplePosition(position, out NavMeshHit hit, 2.5f, NavMesh.AllAreas) ? hit.position : position;
    }

    private void UpdateAnimator()
    {
        if (animator != null && hasSpeedParam)
        {
            animator.SetFloat(speedHash, agent.velocity.magnitude, 0.1f, Time.deltaTime);
        }
    }

    private void UpdateChaseMusic()
    {
        if (chaseMusicSource == null) return;

        bool shouldPlay = currentState == AIState.Chase;
        float target = shouldPlay ? chaseMusicMaxVolume : 0f;

        if (shouldPlay && !chaseMusicSource.isPlaying) chaseMusicSource.Play();

        chaseMusicSource.volume = Mathf.MoveTowards(
            chaseMusicSource.volume, target, chaseMusicMaxVolume * chaseMusicFadeSpeed * Time.deltaTime);

        if (!shouldPlay && chaseMusicSource.isPlaying && chaseMusicSource.volume <= 0.001f)
        {
            chaseMusicSource.Stop();
        }
    }

    private void SetAnimBool(int hash, bool exists, bool value)
    {
        if (animator != null && exists) animator.SetBool(hash, value);
    }

    private void CatchPlayer()
    {
        if (hasCaughtPlayer) return;

        hasCaughtPlayer = true;
        currentState = AIState.Caught;
        onPlayerCaught?.Invoke();

        StartCoroutine(JumpscareSequence());
    }

    private float bloodAlpha = 0f;
    private static Texture2D bloodTexture;

    private static void CreateBloodTexture()
    {
        int size = 128;
        bloodTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        bloodTexture.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float maxDist = center.magnitude;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;

                float alpha = Mathf.SmoothStep(0.15f, 0.92f, dist);
                pixels[y * size + x] = new Color(0.7f, 0.02f, 0.02f, alpha);
            }
        }
        bloodTexture.SetPixels(pixels);
        bloodTexture.Apply();
    }

    private void OnGUI()
    {
        if (showBloodAttackEffect && bloodAlpha > 0.005f)
        {
            if (bloodTexture == null) CreateBloodTexture();
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, bloodAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bloodTexture, ScaleMode.StretchToFill);
            GUI.color = oldColor;
        }
    }

    private IEnumerator JumpscareSequence()
    {

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        agent.enabled = false;

        disabledByCatch.Clear();
        playerControllerWasEnabled = playerController != null && playerController.enabled;
        if (playerController != null) playerController.enabled = false;

        DisableForCatch(playerMoveScript);
        DisableForCatch(cameraLookScript);
        DisableControlScriptsByName(targetCharacter);
        DisableControlScriptsByName(playerCamera);
        if (disableOnCatch != null)
        {
            foreach (MonoBehaviour b in disableOnCatch) DisableForCatch(b);
        }

        if (chaseMusicSource != null)
        {
            chaseMusicSource.volume = 0f;
            chaseMusicSource.Stop();
        }

        if (jumpscareSource != null)
        {
            if (jumpscareClip != null) jumpscareSource.clip = jumpscareClip;
            if (jumpscareSource.clip != null) jumpscareSource.Play();
        }
        else if (jumpscareClip != null)
        {
            AudioSource.PlayClipAtPoint(jumpscareClip, transform.position);
        }

        if (attackHitClip != null)
        {
            AudioSource.PlayClipAtPoint(attackHitClip, transform.position);
        }

        if (animator != null)
        {
            animator.speed = 2.2f;
            SetAnimBool(chasingHash, hasChasingParam, false);
            SetAnimBool(searchingHash, hasSearchingParam, false);
            if (hasCaughtParam) animator.SetTrigger(caughtHash);
        }

        if (playerCamera != null)
        {
            SnapToPlayerFace();
        }

        bloodAlpha = 1.0f;

        Camera camComponent = playerCamera != null ? playerCamera.GetComponent<Camera>() : null;
        float originalFOV = camComponent != null ? camComponent.fieldOfView : 60f;

        Vector3 originalCamLocalPos = playerCamera != null ? playerCamera.localPosition : Vector3.zero;
        float scaledEye = eyeHeight * ScaleY;
        float timer = 0f;

        while (timer < vibrationDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / vibrationDuration);

            float curIntensity = Mathf.Lerp(vibrationIntensity, vibrationIntensity * 0.25f, progress);
            float curAngular = Mathf.Lerp(angularShakeIntensity, angularShakeIntensity * 0.2f, progress);

            bloodAlpha = Mathf.Lerp(1.0f, 0.4f, progress);

            if (playerCamera != null)
            {
                Vector3 head = transform.position + Vector3.up * scaledEye;
                Vector3 look = head - playerCamera.position;
                if (look.sqrMagnitude > 0.0001f)
                {
                    Quaternion baseRot = Quaternion.LookRotation(look.normalized);

                    Quaternion jitter = Quaternion.Euler(
                        Random.Range(-curAngular, curAngular),
                        Random.Range(-curAngular, curAngular),
                        Random.Range(-curAngular * 1.6f, curAngular * 1.6f)
                    );
                    playerCamera.rotation = baseRot * jitter;
                }

                playerCamera.localPosition = originalCamLocalPos + Random.insideUnitSphere * curIntensity;
            }

            if (camComponent != null)
            {

                camComponent.fieldOfView = Mathf.Lerp(originalFOV - 10f, originalFOV, progress);
            }

            yield return null;
        }

        if (playerCamera != null) playerCamera.localPosition = originalCamLocalPos;
        if (camComponent != null) camComponent.fieldOfView = originalFOV;

        yield return new WaitForSeconds(delayBeforeRespawn);

        bloodAlpha = 0f;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RespawnPlayer();

            yield return null;
            RestoreAfterRespawn();
        }
        else
        {
            int index = SceneManager.GetActiveScene().buildIndex;
            if (index >= 0) SceneManager.LoadScene(index);
            else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void SnapToPlayerFace()
    {
        if (playerCamera == null) return;

        Vector3 flatForward = playerCamera.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.001f) flatForward = targetCharacter != null ? targetCharacter.forward : Vector3.forward;
        flatForward.Normalize();

        float distance = faceDistance;
        int count = Physics.RaycastNonAlloc(playerCamera.position, flatForward, hitBuffer, faceDistance + 0.3f,
            obstacleMask, QueryTriggerInteraction.Ignore);

        float nearest = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            Transform t = hitBuffer[i].transform;
            if (t == transform || t.IsChildOf(transform) || IsPlayerTransform(t)) continue;
            if (hitBuffer[i].distance < nearest) nearest = hitBuffer[i].distance;
        }
        if (nearest < float.MaxValue) distance = Mathf.Clamp(nearest - 0.3f, 0.5f, faceDistance);

        Vector3 position = playerCamera.position + flatForward * distance;

        if (Physics.Raycast(position + Vector3.up * 1.5f, Vector3.down, out RaycastHit floorHit, 15f, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            position.y = floorHit.point.y;
        }
        else
        {
            position.y = transform.position.y;
        }

        transform.position = position;

        Vector3 toCamera = playerCamera.position - transform.position;
        toCamera.y = 0f;
        if (toCamera.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(toCamera.normalized);
    }

    private void DisableControlScriptsByName(Transform root)
    {
        if (root == null) return;

        foreach (MonoBehaviour s in root.GetComponents<MonoBehaviour>())
        {
            if (s == null || s == this) continue;

            System.Type type = s.GetType();
            string ns = type.Namespace ?? string.Empty;
            if (ns.StartsWith("UnityEngine") || ns.StartsWith("Unity.") || ns.StartsWith("TMPro")) continue;

            string n = type.Name.ToLowerInvariant();
            if (n.Contains("look") || n.Contains("movement") || n.Contains("camera"))
            {
                DisableForCatch(s);
            }
        }
    }

    private void DisableForCatch(Behaviour b)
    {
        if (b == null || b == this || !b.enabled) return;
        b.enabled = false;
        disabledByCatch.Add(b);
    }

    private void RestoreAfterRespawn()
    {
        foreach (Behaviour b in disabledByCatch)
        {
            if (b != null) b.enabled = true;
        }
        disabledByCatch.Clear();

        if (playerController != null && playerControllerWasEnabled) playerController.enabled = true;

        ResetEnemy();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = currentState == AIState.Chase ? Color.red :
                       currentState == AIState.Investigating ? Color.yellow : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, closeSenseRange);

        Vector3 left = Quaternion.Euler(0f, -fieldOfViewAngle * 0.5f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, fieldOfViewAngle * 0.5f, 0f) * transform.forward;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eye, left * detectionRadius);
        Gizmos.DrawRay(eye, right * detectionRadius);

        if (Application.isPlaying && currentState != AIState.Patrol)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(eye, lastKnownPlayerPosition);
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
        }

        if (waypoints != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);

                Transform next = waypoints[(i + 1) % waypoints.Length];
                if (next != null) Gizmos.DrawLine(waypoints[i].position, next.position);
            }
        }
    }
}