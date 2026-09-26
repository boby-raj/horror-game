using UnityEngine;
using UnityEngine.AI;

public class PatrolAndChaseAI : MonoBehaviour
{
    [Header("Target & Detection")]
    [Tooltip("Drag your Player / Capsule here")]
    public Transform targetCharacter;
    public float detectionRadius = 10.0f;
    public float catchDistance = 1.5f;

    [Header("Patrol Waypoints")]
    [Tooltip("Create empty GameObjects in your scene and drag them here")]
    public Transform[] waypoints;
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 4.5f;
    public float waypointWaitTime = 1.5f;

    [Header("Animation")]
    public string isChasingParam = "IsChasing";

    [Header("Audio")]
    public AudioSource chaseMusicSource;
    public AudioSource jumpscareSource;

    private NavMeshAgent agent;
    private Animator animator;
    
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isChasing = false;
    private bool hasCaughtPlayer = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        hasCaughtPlayer = false;
        isChasing = false;

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

    void Update()
    {
        if (targetCharacter == null || agent == null || hasCaughtPlayer) return;
        if (!agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, targetCharacter.position);

        // State 1: Caught
        if (distanceToPlayer <= catchDistance)
        {
            CatchPlayer();
        }
        // State 2: Chase Player
        else if (distanceToPlayer <= detectionRadius)
        {
            StartChasing();
            agent.SetDestination(targetCharacter.position);
        }
        // State 3: Patrol Area
        else
        {
            if (isChasing)
            {
                StopChasingAndResumePatrol();
            }

            PatrolLogic();
        }
    }

    private void PatrolLogic()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Check if Agatha reached her current patrol point
        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waypointWaitTime)
            {
                // Move to next waypoint in the list
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
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void StartChasing()
    {
        if (!isChasing)
        {
            isChasing = true;
            agent.speed = chaseSpeed;
            agent.isStopped = false;

            if (animator != null) animator.SetBool(isChasingParam, true);

            if (chaseMusicSource != null && !chaseMusicSource.isPlaying)
            {
                chaseMusicSource.Play();
            }
        }
    }

    private void StopChasingAndResumePatrol()
    {
        isChasing = false;
        agent.speed = patrolSpeed;

        if (animator != null) animator.SetBool(isChasingParam, false);

        if (chaseMusicSource != null && chaseMusicSource.isPlaying)
        {
            chaseMusicSource.Stop();
        }

        SetNextWaypointDestination();
    }

    private void CatchPlayer()
    {
        hasCaughtPlayer = true;
        isChasing = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (animator != null) animator.SetBool(isChasingParam, false);

        if (chaseMusicSource != null && chaseMusicSource.isPlaying) chaseMusicSource.Stop();
        if (jumpscareSource != null) jumpscareSource.Play();

        Debug.Log("JUMPSCARE TRIGGERED!");
    }

    private void OnDrawGizmosSelected()
    {
        // Red circle = Chase detection zone
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Yellow circle = Attack/Catch zone
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}