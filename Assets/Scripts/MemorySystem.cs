using UnityEngine;

public class MemorySystem : MonoBehaviour
{
    private const string PlayTimeKey = "PlayTime";
    private const string FavoriteSpotKey = "FavoriteSpot";
    private const string InteractionCountKey = "InteractionCount";

    [SerializeField] private float adaptionSpeed = 0.01f;

    // Preferred play times (normalized 0-1 for day time)
    public float preferredPlayTime { get; private set; }

    // Favorite position (Vector3)
    public Vector3 favoriteSpot { get; private set; }

    // Total interactions
    public int interactionCount { get; private set; }

    // Activity level (how active the robot should be, 0-1)
    public float activityLevel { get; private set; }

    private void Start()
    {
        LoadMemory();
    }

    private void OnApplicationQuit()
    {
        SaveMemory();
    }

    public void RecordInteraction()
    {
        interactionCount++;
        // Adapt preferred play time based on current day time
        float dayTimeNormalized = (Time.time / (24 * 3600)) % 1; // Very simplified day time
        preferredPlayTime = Mathf.Lerp(preferredPlayTime, dayTimeNormalized, adaptionSpeed);
    }

    public void RecordSpotVisit(Vector3 spot)
    {
        favoriteSpot = Vector3.Lerp(favoriteSpot, spot, adaptionSpeed);
    }

    public void UpdateActivityLevel(bool hasInteractedRecently)
    {
        float targetLevel = hasInteractedRecently ? 1f : Mathf.Max(0.2f, preferredPlayTime * 0.8f);
        activityLevel = Mathf.Lerp(activityLevel, targetLevel, adaptionSpeed);
    }

    private void LoadMemory()
    {
        preferredPlayTime = PlayerPrefs.GetFloat(PlayTimeKey, 0.5f); // Default midday
        float x = PlayerPrefs.GetFloat(FavoriteSpotKey + "_x", transform.position.x);
        float y = PlayerPrefs.GetFloat(FavoriteSpotKey + "_y", transform.position.y);
        float z = PlayerPrefs.GetFloat(FavoriteSpotKey + "_z", transform.position.z);
        favoriteSpot = new Vector3(x, y, z);
        interactionCount = PlayerPrefs.GetInt(InteractionCountKey, 0);
        activityLevel = preferredPlayTime; // Initial
    }

    private void SaveMemory()
    {
        PlayerPrefs.SetFloat(PlayTimeKey, preferredPlayTime);
        PlayerPrefs.SetFloat(FavoriteSpotKey + "_x", favoriteSpot.x);
        PlayerPrefs.SetFloat(FavoriteSpotKey + "_y", favoriteSpot.y);
        PlayerPrefs.SetFloat(FavoriteSpotKey + "_z", favoriteSpot.z);
        PlayerPrefs.SetInt(InteractionCountKey, interactionCount);
        PlayerPrefs.Save();
    }
}
