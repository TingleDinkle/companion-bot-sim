using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccessibilityManager : MonoBehaviour
{
    private ConfigurationManager config;
    private RobotUI ui;
    private TextMeshProUGUI voiceOverText;

    void Start()
    {
        config = FindObjectOfType<ConfigurationManager>();
        ui = FindObjectOfType<RobotUI>();
        CreateVoiceOver();
    }

    void Update()
    {
        if (!config.keyboardShortcuts) return;

        if (Input.GetKeyDown(config.toggleUIKey))
        {
            ui.ToggleVisibility();
        }

        if (Input.GetKeyDown(config.spawnCubeKey))
        {
            var stacker = FindObjectOfType<CubeStacker>();
            if (stacker != null)
            {
                stacker.StartGame();
            }
        }
    }

    private void CreateVoiceOver()
    {
        GameObject canvasObj = GameObject.Find("VoiceOverCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("VoiceOverCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            voiceOverText = new GameObject("VoiceOverText").AddComponent<TextMeshProUGUI>();
            voiceOverText.transform.SetParent(canvasObj.transform);
            voiceOverText.rectTransform.anchoredPosition = new Vector2(0, -200);
            voiceOverText.rectTransform.sizeDelta = new Vector2(400, 50);
            voiceOverText.fontSize = 24;
            voiceOverText.color = Color.green;
            voiceOverText.gameObject.SetActive(false);
        }
    }

    public void Announce(string message)
    {
        if (voiceOverText != null && config.voiceOver)
        {
            StartCoroutine(ShowAnnouncement(message));
        }
    }

    System.Collections.IEnumerator ShowAnnouncement(string message)
    {
        voiceOverText.text = message;
        voiceOverText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        voiceOverText.gameObject.SetActive(false);
    }
}
