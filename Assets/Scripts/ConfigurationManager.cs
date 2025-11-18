using UnityEngine;

[CreateAssetMenu(fileName = "RobotConfiguration", menuName = "Robot Configuration")]
public class ConfigurationManager : ScriptableObject
{
    [Header("Robot Behavior")]
    public float wanderRadius = 10f;
    public float idleTime = 1f;
    public float maxSpeed = 2f;
    public float angularSpeed = 120f;
    public bool enableManualControl = true;
    public bool enableAIWandering = true;

    [Header("Memory & Adaptation")]
    public float adaptationSpeed = 0.01f;
    public float interactionWindow = 30f;
    public bool rememberFavoriteSpots = true;
    public bool adaptivePersonality = true;

    [Header("Visual Settings")]
    public Color idleColor = Color.gray;
    public Color excitedColor = Color.yellow;
    public Color sleepyColor = Color.cyan;
    public float animationSpeed = 1f;
    public bool enableParticles = true;
    public bool enableExpressions = true;

    [Header("Audio Settings")]
    public bool enableSounds = true;
    public float masterVolume = 0.5f;
    public float effectsVolume = 0.7f;
    public float musicVolume = 0.3f;
    public bool playBackgroundMusic = false;
    public bool voiceFeedback = true;

    [Header("Game Settings")]
    public bool enableCubeStacking = true;
    public int maxCubeStacks = 5;
    public bool enableMemoryGame = true;
    public float gameDifficultyScale = 1f;

    [Header("Desktop Overlay")]
    public bool enableAlwaysOnTop = false;
    public bool enableTransparency = false;
    public int windowWidth = 400;
    public int windowHeight = 300;
    public Color transparentColor = Color.black;
    public float transparencyAlpha = 0.8f;

    [Header("UI Settings")]
    public bool showMoodIndicator = true;
    public bool showActivityMeter = true;
    public bool showInteractionCount = true;
    public bool showPersonalityInfo = true;
    public float uiScale = 1f;

    [Header("Performance")]
    public int targetFrameRate = 60;
    public bool dynamicQuality = true;
    public bool lowResourceMode = false;
    public float updateInterval = 0.016f; // ~60fps

    [Header("Accessibility")]
    public bool keyboardShortcuts = true;
    public bool voiceOver = false;
    public Language currentLanguage = Language.English;
    public KeyCode toggleUIKey = KeyCode.F1;
    public KeyCode spawnCubeKey = KeyCode.F2;

    public enum Language { English, Spanish, French, German }

    // Apply settings to components
    public void ApplySettings()
    {
        Application.targetFrameRate = targetFrameRate;

        // Find and update components
        var robot = FindObjectOfType<RobotController>();
        var stateManager = FindObjectOfType<BotStateManager>();
        var animator = FindObjectOfType<ProceduralAnimator>();
        var audioManager = AudioManager.Instance;
        var ui = FindObjectOfType<RobotUI>();

        if (robot != null)
        {
            // Sync RobotController settings
            var robotComponent = robot as RobotController; // If it's a MonoBehaviour
            // RobotController settings
            // (In practice, add fields to RobotController for this)
        }

        if (audioManager != null)
        {
            audioManager.SetMusicVolume(musicVolume);
        }

        // Similar for others...
        Debug.Log("Configuration applied!");
    }

    public void ResetToDefaults()
    {
        // Reset all to default values
        wanderRadius = 10f;
        maxSpeed = 2f;
        // etc.
        Debug.Log("Configuration reset to defaults");
    }

    public void ExportConfig(string fileName)
    {
        string json = JsonUtility.ToJson(this);
        System.IO.File.WriteAllText(fileName, json);
        Debug.Log($"Configuration exported to {fileName}");
    }

    public void ImportConfig(string fileName)
    {
        if (System.IO.File.Exists(fileName))
        {
            string json = System.IO.File.ReadAllText(fileName);
            JsonUtility.FromJsonOverwrite(json, this);
            ApplySettings();
            Debug.Log($"Configuration imported from {fileName}");
        }
        else
        {
            Debug.LogWarning($"Config file not found: {fileName}");
        }
    }
}
