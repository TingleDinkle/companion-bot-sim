using UnityEngine;

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

public class EmotionTransitionEngine : MonoBehaviour
{
    private Dictionary<BotState, TransitionProbabilities> stateTransitions;
    private float emotionalConfidence = 0.5f;

    void Awake()
    {
        InitializeTransitionMatrices();
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

    public BotState CalculateNextState(BotState currentState, float activityLevel, bool recentlyInteracted, bool isDayTime)
    {
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
        // Bias towards excited states after interaction
        if (recentlyInteracted)
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
        if (activityLevel < 0.3f)
        {
            adjustedTransitions.toSleepy += 0.5f;
            adjustedTransitions.toIdle += 0.2f;
            adjustedTransitions.toExcited -= 0.3f;
            adjustedTransitions.toPlayful -= 0.4f;
        }
        else if (activityLevel > 0.7f)
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
            emotionalConfidence = Mathf.Lerp(emotionalConfidence, 0.8f, 0.05f); // Increase confidence slowly
        }
        else
        {
            emotionalConfidence = Mathf.Lerp(emotionalConfidence, 0.3f, 0.1f); // Decrease confidence if staying
        }

        return nextState;
    }

    public float GetEmotionalConfidence => emotionalConfidence;
}
