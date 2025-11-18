using UnityEngine;
using System.Collections;

public class BotStateManager : MonoBehaviour
{
    [SerializeField] private BotState currentState = BotState.Idle;
    public BotState State => currentState;

    [SerializeField] private float stateDuration = 5f; // Base duration in seconds
    private float stateTimer = 0f;
    private float lastInteractionTime = 0f;
    private int interactionCount = 0;
    private float lastStateChangeTime = 0f;
    private const float stateChangeCooldown = 2f; // Minimum seconds between state changes

    // Emotional persistence (0-1, how aligned with current emotion the bot is)
    private float emotionalConfidence = 0.5f;

    private EmotionTransitionEngine emotionEngine;
    private MemorySystem memory;
    private ProceduralAnimator animator;
    private MaterialExpression materialExpression;
    private AudioManager audioManager;

    void Start()
    {
        memory = GetComponent<MemorySystem>();
        animator = GetComponent<ProceduralAnimator>() ?? gameObject.AddComponent<ProceduralAnimator>();
        materialExpression = GetComponent<MaterialExpression>() ?? gameObject.AddComponent<MaterialExpression>();
        emotionEngine = GetComponent<EmotionTransitionEngine>() ?? gameObject.AddComponent<EmotionTransitionEngine>();
        audioManager = AudioManager.Instance;

        UpdateStateTimer();
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0)
        {
            DetermineNextState();
            UpdateStateTimer();
        }

        // Handle state-specific logic
        switch (currentState)
        {
            case BotState.Idle:
                HandleIdle();
                break;
            case BotState.Curious:
                HandleCurious();
                break;
            case BotState.Excited:
                HandleExcited();
                break;
            case BotState.Sleepy:
                HandleSleepy();
                break;
            case BotState.Playful:
                HandlePlayful();
                break;
        }
    }

    public void RecordInteraction()
    {
        interactionCount++;
        lastInteractionTime = Time.time;
        // Excitement from interaction
        SetState(BotState.Excited);
    }

    private void DetermineNextState()
    {
        if (emotionEngine == null) return;

        // Check cooldown
        if (Time.time - lastStateChangeTime < stateChangeCooldown) return;

        float activity = memory?.activityLevel ?? 0.5f;
        bool hasInteractedRecently = (Time.time - lastInteractionTime) < 30f;
        float dayTime = (Time.time % 86400) / 3600;
        bool isDayTime = dayTime > 6 && dayTime < 22;

        BotState nextState = emotionEngine.CalculateNextState(currentState, activity, hasInteractedRecently, isDayTime);

        if (nextState != currentState)
        {
            SetState(nextState);
        }
    }

    private void UpdateStateTimer()
    {
        // Vary duration based on state and activity
        float multiplier = 1f;
        switch (currentState)
        {
            case BotState.Idle: multiplier = 0.5f; break;
            case BotState.Sleepy: multiplier = 2f; break;
            case BotState.Excited: multiplier = 0.3f; break;
        }
        stateTimer = stateDuration * multiplier * Random.Range(0.8f, 1.2f);
    }

    // State handlers
    private void HandleIdle()
    {
        // Subtle movement or effect
        if (animator != null)
        {
            animator.SetBobbing(true);
        }
    }

    private void HandleCurious()
    {
        // Tilt or look around
        if (animator != null)
        {
            animator.SetThinkingParticles(true);
        }
    }

    private void HandleExcited()
    {
        // Bounce and spin
        if (animator != null)
        {
            animator.SetBounce(true);
            animator.SetSpin(true);
        }

        // Desktop interaction: simulate mouse prank occasionally
        if (config != null && config.enableMousePranks && Time.time - lastStateChangeTime > 3f)
        {
            var interaction = FindObjectOfType<DesktopInteraction>();
            if (interaction != null)
            {
                Vector2 screenPoint = new Vector2(Screen.width / 2, Screen.height / 2);
                interaction.SimulateMousePrank(screenPoint);
            }
        }
    }

    private void HandleSleepy()
    {
        // Slow down, subtle effects
        if (animator != null)
        {
            animator.SetSlowMotion(true);
        }
    }

    private void HandlePlayful()
    {
        // Playful spins and movements
        if (animator != null)
        {
            animator.SetSpin(true);
            animator.SetThinkingParticles(true);
        }

        // Desktop prank: move a window randomly
        if (config != null && config.enableWindowInteractions && Time.time - lastStateChangeTime > 5f)
        {
            var interaction = FindObjectOfType<DesktopInteraction>();
            if (interaction != null)
            {
                interaction.MoveRandomWindow(Random.Range(-100, 100), Random.Range(-100, 100));
            }
        }
    }

    public void SetState(BotState newState)
    {
        if (currentState != newState)
        {
            Debug.Log($"Bot state changed from {currentState} to {newState}");
            currentState = newState;
            UpdateStateTimer();
            // Reset state-specific effects
            if (animator != null)
            {
                animator.Reset();
            }
        }
    }
}
