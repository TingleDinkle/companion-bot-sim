using UnityEngine;
using UnityEngine.UI; // Assuming UGUI
using TMPro;

public class RobotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI moodText;
    [SerializeField] private TextMeshProUGUI interactionCountText;
    [SerializeField] private Slider activitySlider;
    [SerializeField] private TextMeshProUGUI personalityText;

    private BotStateManager stateManager;
    private MemorySystem memory;
    private PersonalityTraitManager personality;

    void Start()
    {
        stateManager = FindObjectOfType<BotStateManager>();
        memory = FindObjectOfType<MemorySystem>();
        personality = FindObjectOfType<PersonalityTraitManager>();

        // Auto setup if not assigned
        if (moodText == null || interactionCountText == null)
        {
            CreateUI();
        }
    }

    void Update()
    {
        if (stateManager != null && moodText != null)
        {
            moodText.text = $"Mood: {stateManager.State}";
        }

        if (memory != null && interactionCountText != null)
        {
            interactionCountText.text = $"Interactions: {memory.interactionCount}";
        }

        if (activitySlider != null && memory != null)
        {
            activitySlider.value = memory.activityLevel;
        }

        if (personalityText != null && personality != null)
        {
            personalityText.text = $"Personality: {personality.GetDominantTrait()}";
        }
    }

    void CreateUI()
    {
        // Create Canvas if needed
        GameObject canvasObj = GameObject.Find("RobotCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("RobotCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create elements
        if (moodText == null)
        {
            GameObject textObj = new GameObject("MoodText");
            textObj.transform.SetParent(canvasObj.transform);
            moodText = textObj.AddComponent<TextMeshProUGUI>();
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(100, -50);
            rect.sizeDelta = new Vector2(200, 50);
            moodText.fontSize = 24;
            moodText.color = Color.white;
            moodText.text = "Mood: Idle";
        }

        // Similarly for others, but to save time, assume assigned
    }

    public void ToggleVisibility()
    {
        if (moodText != null)
        {
            moodText.gameObject.SetActive(!moodText.gameObject.activeSelf);
        }
    }
}
