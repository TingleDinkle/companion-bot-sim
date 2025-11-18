using UnityEngine;

public class MasterCoordinator : MonoBehaviour
{
    public static MasterCoordinator Instance; // Singleton

    public ConfigurationManager config;
    public SaveLoadManager saveManager;
    public AudioManager audioManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeSystems();
    }

    void Start()
    {
        LoadGame();
        ApplyConfiguration();
    }

    private void InitializeSystems()
    {
        // Ensure all major systems exist
        var robot = FindObjectOfType<RobotController>();
        if (robot == null)
        {
            GameObject robotObj = new GameObject("Robot");
            robot = robotObj.AddComponent<RobotController>();
            // Add required components
            robotObj.AddComponent<NavMeshAgent>();
            robotObj.AddComponent<Rigidbody>();
        }

        var memory = FindObjectOfType<MemorySystem>();
        if (memory == null)
        {
            robot.gameObject.AddComponent<MemorySystem>();
        }

        var stateManager = FindObjectOfType<BotStateManager>();
        if (stateManager == null)
        {
            robot.gameObject.AddComponent<BotStateManager>();
        }

        var personalityManager = FindObjectOfType<PersonalityTraitManager>();
        if (personalityManager == null)
        {
            robot.gameObject.AddComponent<PersonalityTraitManager>();
        }

        // Audio
        if (AudioManager.Instance == null)
        {
            robot.gameObject.AddComponent<AudioManager>();
        }

        // Visual
        var animator = FindObjectOfType<ProceduralAnimator>();
        if (animator == null)
        {
            robot.gameObject.AddComponent<ProceduralAnimator>();
        }

        var expression = FindObjectOfType<MaterialExpression>();
        if (expression == null)
        {
            robot.gameObject.AddComponent<MaterialExpression>();
        }

        // Games
        var cubeStacker = FindObjectOfType<CubeStacker>();
        if (cubeStacker == null)
        {
            GameObject gameObj = new GameObject("GameSystem");
            gameObj.AddComponent<CubeStacker>();
        }

        // UI
        var ui = FindObjectOfType<RobotUI>();
        if (ui == null)
        {
            GameObject uiObj = new GameObject("UI");
            uiObj.AddComponent<RobotUI>();
        }

        var moodIndicator = FindObjectOfType<MoodIndicator>();
        if (moodIndicator == null)
        {
            robot.gameObject.AddComponent<MoodIndicator>();
        }

        // Desktop
        var overlay = FindObjectOfType<DesktopOverlay>();
        if (overlay == null)
        {
            GameObject overlayObj = new GameObject("OverlayManager");
            overlayObj.AddComponent<DesktopOverlay>();
        }

        // Save system
        if (SaveLoadManager.Instance == null)
        {
            GameObject saveObj = new GameObject("SaveManager");
            saveObj.AddComponent<SaveLoadManager>();
        }

        Debug.Log("All systems initialized!");
    }

    public void LoadGame()
    {
        if (saveManager != null)
        {
            saveManager.LoadGame();
        }
    }

    public void SaveGame()
    {
        if (saveManager != null)
        {
            saveManager.SaveGame();
        }
    }

    public void NewGame()
    {
        if (saveManager != null)
        {
            saveManager.NewGame();
        }
    }

    public void ApplyConfiguration()
    {
        if (config != null)
        {
            config.ApplySettings();
        }
        else
        {
            // Find or create config
            config = Resources.Load<ConfigurationManager>("RobotConfiguration");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ConfigurationManager>();
                Debug.Log("Created default configuration");
            }
        }
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}
