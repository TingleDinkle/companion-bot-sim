using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MemoryPatternGame : MonoBehaviour
{
    [SerializeField] private List<Light> patternLights = new List<Light>();
    [SerializeField] private AudioSource gameAudio;
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private float sequencePause = 1f;
    [SerializeField] private int maxSequenceLength = 5;

    private List<int> sequence = new List<int>();
    private List<int> playerSequence = new List<int>();
    private int currentStep = 0;
    private bool isShowingPattern = false;
    private bool isGameActive = false;

    private RobotController robot;

    void Start()
    {
        robot = FindObjectOfType<RobotController>();
        if (gameAudio == null)
        {
            gameAudio = GetComponent<AudioSource>();
        }
    }

    public void StartGame()
    {
        if (isGameActive) return;
        isGameActive = true;
        sequence.Clear();
        playerSequence.Clear();
        currentStep = 0;
        AddToSequence();
        StartCoroutine(ShowPattern());
        Debug.Log("Memory Pattern Game Started!");
    }

    private void AddToSequence()
    {
        sequence.Add(Random.Range(0, patternLights.Count));
    }

    private IEnumerator ShowPattern()
    {
        isShowingPattern = true;
        foreach (int lightIndex in sequence)
        {
            patternLights[lightIndex].enabled = true;
            if (gameAudio != null) gameAudio.Play();
            yield return new WaitForSeconds(flashDuration);
            patternLights[lightIndex].enabled = false;
            yield return new WaitForSeconds(sequencePause);
        }
        isShowingPattern = false;
        Debug.Log("Repeat the pattern by clicking the lights!");
    }

    public void OnLightClicked(int lightIndex)
    {
        if (isShowingPattern || !isGameActive) return;

        playerSequence.Add(lightIndex);
        patternLights[lightIndex].enabled = true;
        StartCoroutine(FlashLight(lightIndex));

        if (playerSequence[currentStep] != sequence[currentStep])
        {
            GameOver();
            return;
        }

        currentStep++;
        if (currentStep >= sequence.Count)
        {
            LevelComplete();
        }
    }

    private IEnumerator FlashLight(int index)
    {
        yield return new WaitForSeconds(flashDuration / 2);
        patternLights[index].enabled = false;
    }

    private void LevelComplete()
    {
        currentStep = 0;
        playerSequence.Clear();
        if (sequence.Count < maxSequenceLength)
        {
            AddToSequence();
            RewardRobot();
        }
        else
        {
            WinGame();
        }
        StartCoroutine(ShowPattern());
    }

    private void GameOver()
    {
        isGameActive = false;
        Debug.Log("Game Over! Try again.");
        if (robot != null)
        {
            robot.stateManager.SetState(BotState.Playful); // Encourage retry
        }
        sequence.Clear();
        playerSequence.Clear();
    }

    private void WinGame()
    {
        isGameActive = false;
        Debug.Log("Congratulations! You mastered the pattern!");
        if (robot != null)
        {
            robot.memory.RecordInteraction();
            robot.stateManager.SetState(BotState.Excited);
        }
    }

    private void RewardRobot()
    {
        if (robot != null)
        {
            robot.OnInteraction();
        }
    }
}
