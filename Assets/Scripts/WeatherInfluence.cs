using UnityEngine;

public class WeatherInfluence : MonoBehaviour
{
    private RobotController robot;
    public enum Weather { Sunny, Cloudy, Rainy, Stormy, Snowy }

    void Start()
    {
        robot = FindObjectOfType<RobotController>();
    }

    public void SetWeather(Weather weather)
    {
        switch (weather)
        {
            case Weather.Sunny:
                if (robot != null)
                {
                    robot.stateManager.SetState(BotState.Playful);
                }
                break;
            case Weather.Rainy:
                if (robot != null)
                {
                    robot.stateManager.SetState(BotState.Sleepy);
                }
                break;
            case Weather.Stormy:
                if (robot != null)
                {
                    // React to storm
                }
                break;
        }
    }

    public Weather SimulateWeather()
    {
        // Random weather simulation
        return (Weather)Random.Range(0, 5);
    }
}
