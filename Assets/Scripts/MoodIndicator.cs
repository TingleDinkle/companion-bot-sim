using UnityEngine;
using TMPro; // Assuming TextMeshPro is available, if not use UnityEngine.UI

public class MoodIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshPro moodText; // Drag TMPro text component here
    [SerializeField] private BotStateManager stateManager;

    void Start()
    {
        if (moodText == null)
        {
            GameObject textObj = new GameObject("MoodText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = Vector3.up * 1.5f; // Above robot
            moodText = textObj.AddComponent<TextMeshPro>();
            moodText.fontSize = 4;
            moodText.color = Color.white;
            moodText.alignment = TextAlignmentOptions.Center;
            moodText.text = "Idle"; // Default
        }

        if (stateManager == null)
        {
            stateManager = GetComponentInParent<BotStateManager>();
        }
    }

    void Update()
    {
        if (stateManager != null && moodText != null)
        {
            moodText.text = stateManager.State.ToString();
            SetColorBasedOnMood(stateManager.State);
        }
    }

    private void SetColorBasedOnMood(BotState state)
    {
        if (moodText == null) return;

        switch (state)
        {
            case BotState.Idle:
                moodText.color = Color.gray;
                break;
            case BotState.Curious:
                moodText.color = Color.blue;
                break;
            case BotState.Excited:
                moodText.color = Color.yellow;
                break;
            case BotState.Sleepy:
                moodText.color = Color.cyan;
                break;
            case BotState.Playful:
                moodText.color = Color.green;
                break;
        }
    }
}
