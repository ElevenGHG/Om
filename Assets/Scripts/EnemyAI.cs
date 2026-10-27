using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float attackDistance = 8f;
    [SerializeField] private float searchDuration = 5f;
    [SerializeField] private float arrivalDistance = 0.5f;
    [SerializeField] private float turnSpeed = 5f;

    private NavMeshAgent agent;
    private Enemy enemy;

    private Vector3 startPosition;
    private Vector3 lastKnownPlayerPosition;

    private float searchTimer;

    private enum AIState
    {
        Idle,
        Chasing,
        Attacking,
        Searching,
        Returning
    }

    private AIState currentState = AIState.Idle;

    // Die Waffe darf nur im Angriffsmodus schießen.
    public bool CanShoot { get; private set; }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        if (player == null)
        {
            GameObject target =
                GameObject.FindGameObjectWithTag("Player");

            if (target != null)
                player = target.transform;
        }

        if (agent == null || enemy == null || player == null)
        {
            Debug.LogError(
                "EnemyAI: NavMeshAgent, Enemy oder Player fehlt!",
                this
            );

            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError(
                "EnemyAI: Gegner steht nicht auf dem NavMesh!",
                this
            );

            enabled = false;
            return;
        }

        startPosition = transform.position;
        lastKnownPlayerPosition = startPosition;

        // Die Drehung wird durch Enemy bzw. dieses Skript gesteuert.
        agent.updateRotation = false;
        agent.isStopped = true;
    }

    private void Update()
    {
        if (player == null || !agent.isOnNavMesh)
            return;

        CanShoot = false;

        bool canSeePlayer = enemy.CanSeePlayerForAI();

        // SPIELER SICHTBAR
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;

            float distance = Vector3.Distance(
                transform.position,
                player.position
            );

            if (distance > attackDistance)
            {
                // Zu weit entfernt: Spieler verfolgen.
                currentState = AIState.Chasing;

                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {
                // Nah genug: stehen bleiben und angreifen.
                currentState = AIState.Attacking;

                agent.isStopped = true;
                agent.ResetPath();

                CanShoot = true;
            }

            return;
        }

        // SICHT VERLOREN
        if (currentState == AIState.Chasing ||
            currentState == AIState.Attacking)
        {
            currentState = AIState.Searching;
            searchTimer = 0f;

            agent.isStopped = false;
            agent.SetDestination(lastKnownPlayerPosition);
        }

        // LETZTE BEKANNTE POSITION SUCHEN
        if (currentState == AIState.Searching)
        {
            RotateTowardsMovement();

            if (HasReachedDestination())
            {
                agent.isStopped = true;
                searchTimer += Time.deltaTime;

                if (searchTimer >= searchDuration)
                {
                    currentState = AIState.Returning;

                    agent.isStopped = false;
                    agent.SetDestination(startPosition);
                }
            }
        }
        // ZUM STARTPUNKT ZURÜCKKEHREN
        else if (currentState == AIState.Returning)
        {
            RotateTowardsMovement();

            if (HasReachedDestination())
            {
                agent.isStopped = true;
                currentState = AIState.Idle;
            }
        }
        // AUF DEM STARTPUNKT WARTEN
        else if (currentState == AIState.Idle)
        {
            agent.isStopped = true;
        }
    }

    private void RotateTowardsMovement()
    {
        if (agent.isStopped || agent.velocity.sqrMagnitude < 0.01f)
            return;

        Vector3 direction = agent.velocity;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    private bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        if (!agent.hasPath)
            return true;

        return agent.remainingDistance <=
            Mathf.Max(arrivalDistance, agent.stoppingDistance);
    }
}