using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BotState
{
    Idle,
    Curious,
    Excited,
    Sleepy,
    Playful
}

[System.Serializable]
public class TransitionProbabilities
{
    [Range(0f, 1f)] public float toIdle;
    [Range(0f, 1f)] public float toCurious;
    [Range(0f, 1f)] public float toExcited;
    [Range(0f, 1f)] public float toSleepy;
    [Range(0f, 1f)] public float toPlayful;

    public void Normalize()
    {
        float total = toIdle + toCurious + toExcited + toSleepy + toPlayful;
        if (total > 0)
        {
            toIdle /= total;
            toCurious /= total;
            toExcited /= total;
            toSleepy /= total;
            toPlayful /= total;
        }
    }

    public BotState GetRandomState()
    {
        float rand = Random.value;
        if (rand < toIdle) return BotState.Idle;
        rand -= toIdle;
        if (rand < toCurious) return BotState.Curious;
        rand -= toCurious;
        if (rand < toExcited) return BotState.Excited;
        rand -= toExcited;
        if (rand < toSleepy) return BotState.Sleepy;
        rand -= toSleepy;
        return BotState.Playful; // Remaining percentile
    }

    public float GetToState(BotState state)
    {
        switch (state)
        {
            case BotState.Idle: return toIdle;
            case BotState.Curious: return toCurious;
            case BotState.Excited: return toExcited;
            case BotState.Sleepy: return toSleepy;
            case BotState.Playful: return toPlayful;
            default: return 0;
        }
    }

    public void SetToState(BotState state, float value)
    {
        switch (state)
        {
            case BotState.Idle: toIdle = value; break;
            case BotState.Curious: toCurious = value; break;
            case BotState.Excited: toExcited = value; break;
            case BotState.Sleepy: toSleepy = value; break;
            case BotState.Playful: toPlayful = value; break;
        }
        Clamp();
    }

    private void Clamp()
    {
        toIdle = Mathf.Clamp01(toIdle);
        toCurious = Mathf.Clamp01(toCurious);
        toExcited = Mathf.Clamp01(toExcited);
        toSleepy = Mathf.Clamp01(toSleepy);
        toPlayful = Mathf.Clamp01(toPlayful);
    }

    public void ScaleAwayFrom(BotState state, float scale)
    {
        if (state == BotState.Idle) toIdle *= (1f - scale);
        else if (state == BotState.Curious) toCurious *= (1f - scale);
        else if (state == BotState.Excited) toExcited *= (1f - scale);
        else if (state == BotState.Sleepy) toSleepy *= (1f - scale);
        else if (state == BotState.Playful) toPlayful *= (1f - scale);
        Clamp();
    }
}

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

    // Transition matrices for each state
    private Dictionary<BotState, TransitionProbabilities> stateTransitions;

    private MemorySystem memory;
    private ProceduralAnimator animator;
    private MaterialExpression materialExpression;
    private AudioManager audioManager;

    void Start()
    {
        memory = GetComponent<MemorySystem>();
        animator = GetComponent<ProceduralAnimator>() ?? gameObject.AddComponent<ProceduralAnimator>();
        materialExpression = GetComponent<MaterialExpression>() ?? gameObject.AddComponent<MaterialExpression>();
        audioManager = AudioManager.Instance;

        InitializeTransitionMatrices();
        UpdateStateTimer();
    }

    private void InitializeTransitionMatrices()
    {
        stateTransitions = new Dictionary<BotState, TransitionProbabilities>();

        // Define intelligent transitions for each state
        stateTransitions[BotState.Idle] = new TransitionProbabilities
        {
            toIdle = 0.3f, // Tend to stay idle
            toCurious = 0.4f, // Become curious
            toExcited = 0.1f, // Rarely excited
            toSleepy = 0.15f, // Can become sleepy
            toPlayful = 0.05f // Unlikely playful
        };

        stateTransitions[BotState.Curious] = new TransitionProbabilities
        {
            toIdle = 0.2f,
            toCurious = 0.25f, // Persist in curiosity
            toExcited = 0.2f, // Can get excited
            toSleepy = 0.05f,
            toPlayful = 0.3f // Likely to become playful
        };

        stateTransitions[BotState.Excited] = new TransitionProbabilities
        {
            toIdle = 0.4f, // Wind down to idle
            toCurious = 0.1f,
            toExcited = 0.2f, // Can stay excited
            toSleepy = 0.05f,
            toPlayful = 0.25f // Move to playful
        };

        stateTransitions[BotState.Sleepy] = new TransitionProbabilities
        {
            toIdle = 0.25f,
            toCurious = 0.1f,
            toExcited = 0.05f,
            toSleepy = 0.5f, // Likely to stay sleepy
            toPlayful = 0.1f
        };

        stateTransitions[BotState.Playful] = new TransitionProbabilities
        {
            toIdle = 0.2f,
            toCurious = 0.2f,
            toExcited = 0.3f, // High chance to excited
            toSleepy = 0.05f,
            toPlayful = 0.25f // Stay playful
        };

        // Normalize all
        foreach (var transitions in stateTransitions.Values)
        {
            transitions.Normalize();
        }
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
        // Check cooldown
        if (Time.time - lastStateChangeTime < stateChangeCooldown) return;

        // Get base transition probabilities
        if (!stateTransitions.TryGetValue(currentState, out TransitionProbabilities baseTransitions))
        {
            baseTransitions = stateTransitions[BotState.Idle]; // Fallback
        }

        TransitionProbabilities adjustedTransitions = new TransitionProbabilities
        {
            toIdle = baseTransitions.toIdle,
            toCurious = baseTransitions.toCurious,
            toExcited = baseTransitions.toExcited,
            toSleepy = baseTransitions.toSleepy,
            toPlayful = baseTransitions.toPlayful
        };

        // Adjust based on context
        float recentThreshold = 30f;
        bool hasInteractedRecently = (Time.time - lastInteractionTime) < recentThreshold;
        float activity = memory?.activityLevel ?? 0.5f;

        // Time of day
        float dayTime = (Time.time % 86400) / 3600;
        bool isDayTime = dayTime > 6 && dayTime < 22;

        // Bias towards excited states after interaction
        if (hasInteractedRecently)
        {
            adjustedTransitions.toExcited += 0.3f;
            adjustedTransitions.toPlayful += 0.2f;
            adjustedTransitions.toIdle -= 0.3f;
            adjustedTransitions.toSleepy -= 0.2f;
        }

        // Night time bias towards sleepy
        if (!isDayTime)
        {
            adjustedTransitions.toSleepy += 0.4f;
            adjustedTransitions.toIdle += 0.1f;
            adjustedTransitions.toExcited -= 0.3f;
            adjustedTransitions.toPlayful -= 0.2f;
        }

        // Low activity bias towards sleep or idle
        if (activity < 0.3f)
        {
            adjustedTransitions.toSleepy += 0.5f;
            adjustedTransitions.toIdle += 0.2f;
            adjustedTransitions.toExcited -= 0.3f;
            adjustedTransitions.toPlayful -= 0.4f;
        }
        else if (activity > 0.7f)
        {
            // High activity favors active states
            adjustedTransitions.toCurious += 0.2f;
            adjustedTransitions.toPlayful += 0.3f;
            adjustedTransitions.toExcited += 0.1f;
        }

        // Apply emotional persistence - less likely to change if confident
        if (emotionalConfidence > 0.7f)
        {
            // Bias towards staying in current state
            float currentBias = 0.4f / emotionalConfidence;
            adjustedTransitions.SetToState(currentState, adjustedTransitions.GetToState(currentState) + currentBias);

            // Reduce transitions away proportionally
            adjustedTransitions.ScaleAwayFrom(currentState, (1f - emotionalConfidence) * 0.5f);
        }

        adjustedTransitions.Normalize();

        BotState nextState = adjustedTransitions.GetRandomState();

        if (nextState != currentState)
        {
            SetState(nextState);
            emotionalConfidence = Mathf.Lerp(emotionalConfidence, 0.8f, 0.05f); // Increase confidence slowly
        }
        else
        {
            emotionalConfidence = Mathf.Lerp(emotionalConfidence, 0.3f, 0.1f); // Decrease confidence if staying
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
