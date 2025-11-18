using UnityEngine;

public class PerformanceOptimizer : MonoBehaviour
{
    private ConfigurationManager config;

    void Start()
    {
        config = FindObjectOfType<ConfigurationManager>();
        if (config != null)
        {
            ApplySettings();
        }
    }

    void Update()
    {
        if (config.dynamicQuality)
        {
            AdjustQuality();
        }
    }

    void AdjustQuality()
    {
        // Dynamic quality based on frame time
        float frameTime = Time.deltaTime;
        if (frameTime > 0.06f && QualitySettings.antiAliasing > 0) // > ~16fps
        {
            QualitySettings.antiAliasing -= 2;
        }
        else if (frameTime < 0.03f && QualitySettings.antiAliasing < 8) // < 33fps
        {
            QualitySettings.antiAliasing += 2;
        }
    }

    public void ApplySettings()
    {
        QualitySettings.vSyncCount = config.lowResourceMode ? 1 : 0;
        Application.targetFrameRate = config.targetFrameRate;
    }
}
