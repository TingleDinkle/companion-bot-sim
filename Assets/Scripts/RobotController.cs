using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class RobotController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float angularSpeed = 120f;
    [SerializeField] private LayerMask navMeshLayer;

    [Header("AI Wandering")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderInterval = 3f;
    [SerializeField] private float idleTime = 1f;

    private NavMeshAgent agent;
    private Vector3 currentTarget;
    private float lastWanderTime;
    private float idleTimer = 0f;

    [SerializeField] private bool isWandering = true;

    private InputAction moveAction;
    private InputAction interactAction;
    private Vector2 moveInput;

    // Adaptation and memory
    private MemorySystem memory;
    private BotStateManager stateManager;
    private System.Collections.Generic.List<Vector3> recentPositions;
    private const int MaxRecentPositions = 5;
    private float lastInteractionTime;
    private const float InteractionWindow = 30f; // Seconds

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = maxSpeed;
        agent.angularSpeed = angularSpeed;

        // Initialize adaptation and state
        memory = GetComponent<MemorySystem>();
        if (memory == null)
        {
            memory = gameObject.AddComponent<MemorySystem>();
        }
        stateManager = GetComponent<BotStateManager>();
        if (stateManager == null)
        {
            stateManager = gameObject.AddComponent<BotStateManager>();
        }
        recentPositions = new System.Collections.Generic.List<Vector3>();
        lastInteractionTime = Time.time;

        // Get actions from InputSystem
        var playerInputActions = new InputSystem_Actions(); // Assuming this is the asset
        moveAction = playerInputActions.Player.Move;
        interactAction = playerInputActions.Player.Interact;
        moveAction.Enable();
        interactAction.Enable();

        if (isWandering)
        {
            WanderToNewPoint();
        }
    }

    void OnDestroy()
    {
        moveAction.Disable();
        interactAction.Disable();
    }

    void Update()
    {
        // Update memory
        bool hasInteractedRecently = (Time.time - lastInteractionTime) < InteractionWindow;
        memory.UpdateActivityLevel(hasInteractedRecently);

        // Adjust idle time based on activity level
        float effectiveIdleTime = idleTime / Mathf.Max(0.1f, memory.activityLevel);

        moveInput = moveAction.ReadValue<Vector2>();

        if (moveInput.magnitude > 0.1f)
        {
            // Manual control mode
            isWandering = false;
            agent.ResetPath();

            // Rotate based on horizontal input (like turning left/right)
            transform.Rotate(Vector3.up, moveInput.x * angularSpeed * Time.deltaTime);

            // Move forward/backward
            transform.Translate(Vector3.forward * moveInput.y * maxSpeed * Time.deltaTime);

            idleTimer = 0f; // Reset idle timer
        }
        else
        {
            // Switch to wandering if not manually controlling
            if (!isWandering)
            {
                isWandering = true;
                WanderToNewPoint();
            }

            // Handle wandering behavior
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                // Record visit
                memory.RecordSpotVisit(transform.position);
                recentPositions.Add(transform.position);
                if (recentPositions.Count > MaxRecentPositions)
                {
                    recentPositions.RemoveAt(0);
                }

                idleTimer += Time.deltaTime;
                if (idleTimer >= effectiveIdleTime)
                {
                    WanderToNewPoint();
                    idleTimer = 0f;
                }
            }
        }
    }

    void WanderToNewPoint()
    {
        Vector3 targetPosition;
        if (Random.value < 0.2f && memory.favoriteSpot != transform.position)
        {
            // Go to favorite spot with some variation
            targetPosition = memory.favoriteSpot + Random.insideUnitSphere * 2f;
            targetPosition.y = memory.favoriteSpot.y;
        }
        else
        {
            // Random direction
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            targetPosition = randomDirection + transform.position;
            targetPosition.y = transform.position.y;
        }

        // Find valid position
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, wanderRadius, navMeshLayer))
        {
            // Check if not too close to recent positions
            bool tooClose = false;
            foreach (Vector3 recent in recentPositions)
            {
                if (Vector3.Distance(hit.position, recent) < 1f)
                {
                    tooClose = true;
                    break;
                }
            }
            if (!tooClose)
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        // Fallback to random sampling
        for (int i = 0; i < 10; i++) // Try up to 10 times
        {
            Vector3 fallbackDirection = Random.insideUnitSphere * wanderRadius;
            fallbackDirection += transform.position;
            fallbackDirection.y = transform.position.y;

            if (NavMesh.SamplePosition(fallbackDirection, out hit, wanderRadius, navMeshLayer))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        // If all fail, stay put
        Debug.LogWarning("No valid wander position found");
    }

    public void OnInteraction()
    {
        lastInteractionTime = Time.time;
        memory.RecordInteraction();
        // Perhaps trigger a response behavior
        Debug.Log("Robot interacted! Total interactions: " + memory.interactionCount);
    }

    // For debugging or output, you can add methods to trigger specific behaviors
    public void SetWandering(bool wandering)
    {
        isWandering = wandering;
        if (wandering)
        {
            WanderToNewPoint();
        }
        else
        {
            agent.ResetPath();
        }
    }

    public void GoToPoint(Vector3 point)
    {
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, 1f, navMeshLayer))
        {
            agent.SetDestination(hit.position);
            isWandering = false;
        }
    }
}
