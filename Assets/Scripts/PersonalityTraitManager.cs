using UnityEngine;

public enum PersonalityTrait
{
    Playful,
    Curious,
    Energetic,
    Calm,
    Adventurous,
    Cautious
}

[System.Serializable]
public class TraitData
{
    public PersonalityTrait trait;
    public float value; // 0-1
    public float adaptationRate = 0.01f;
}

public class PersonalityTraitManager : MonoBehaviour
{
    [SerializeField] private TraitData[] traits = new TraitData[]
    {
        new TraitData { trait = PersonalityTrait.Playful, value = 0.5f },
        new TraitData { trait = PersonalityTrait.Curious, value = 0.5f },
        new TraitData { trait = PersonalityTrait.Energetic, value = 0.5f },
        new TraitData { trait = PersonalityTrait.Calm, value = 0.5f },
        new TraitData { trait = PersonalityTrait.Adventurous, value = 0.5f },
        new TraitData { trait = PersonalityTrait.Cautious, value = 0.5f }
    };

    private MemorySystem memory;
    private RobotController robot;

    void Start()
    {
        memory = GetComponent<MemorySystem>();
        robot = GetComponent<RobotController>();
        // Load traits from PlayerPrefs or something
        LoadTraits();
    }

    void OnApplicationQuit()
    {
        SaveTraits();
    }

    public float GetTraitValue(PersonalityTrait trait)
    {
        foreach (var t in traits)
        {
            if (t.trait == trait) return t.value;
        }
        return 0.5f;
    }

    public void AdaptTrait(PersonalityTrait trait, float target, float amount)
    {
        foreach (var t in traits)
        {
            if (t.trait == trait)
            {
                t.value = Mathf.Lerp(t.value, target, t.adaptationRate * amount);
                break;
            }
        }
    }

    // Based on interactions, evolve personality
    public void OnPlayfulInteraction()
    {
        AdaptTrait(PersonalityTrait.Playful, 1f, 0.1f);
        AdaptTrait(PersonalityTrait.Curious, 0f, 0.05f); // Less curious
    }

    public void OnCalmInteraction()
    {
        AdaptTrait(PersonalityTrait.Calm, 1f, 0.1f);
        AdaptTrait(PersonalityTrait.Energetic, 0f, 0.05f);
    }

    // Influence robot behavior based on traits
    public BotState SuggestPreferredState()
    {
        // Simple: highest trait determines
        PersonalityTrait dominant = PersonalityTrait.Playful;
        float maxVal = 0f;

        foreach (var t in traits)
        {
            if (t.value > maxVal)
            {
                maxVal = t.value;
                dominant = t.trait;
            }
        }

        switch (dominant)
        {
            case PersonalityTrait.Playful: return BotState.Playful;
            case PersonalityTrait.Curious: return BotState.Curious;
            case PersonalityTrait.Energetic: return BotState.Excited;
            case PersonalityTrait.Calm: return BotState.Idle;
            default: return BotState.Idle;
        }
    }

    public void InfluenceActivity()
    {
        float energy = GetTraitValue(PersonalityTrait.Energetic);
        if (robot != null && robot.memory != null)
        {
            // Adjust activity level subtly
            robot.memory.activityLevel = Mathf.Lerp(robot.memory.activityLevel, energy, 0.02f);
        }
    }

    private void LoadTraits()
    {
        foreach (var t in traits)
        {
            t.value = PlayerPrefs.GetFloat($"Trait_{t.trait}", t.value);
        }
    }

    private void SaveTraits()
    {
        foreach (var t in traits)
        {
            PlayerPrefs.SetFloat($"Trait_{t.trait}", t.value);
        }
        PlayerPrefs.Save();
    }

    public PersonalityTrait GetDominantTrait()
    {
        PersonalityTrait dominant = PersonalityTrait.Playful;
        float maxVal = 0f;

        foreach (var t in traits)
        {
            if (t.value > maxVal)
            {
                maxVal = t.value;
                dominant = t.trait;
            }
        }
        return dominant;
    }
}
