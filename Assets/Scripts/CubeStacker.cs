using UnityEngine;
using System.Collections.Generic;

public class CubeStacker : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int maxCubes = 5;
    [SerializeField] private float cubeSpacing = 1.5f;
    [SerializeField] private Vector3 stackBasePosition = Vector3.zero;
    [SerializeField] private LayerMask placementLayer;

    private List<StackableCube> stackedCubes = new List<StackableCube>();
    private bool gameActive = false;
    private int score = 0;

    private RobotController robot;

    void Start()
    {
        robot = FindObjectOfType<RobotController>();
        // Optionally spawn cubes near the bot or in a designated area
    }

    public void StartGame()
    {
        if (gameActive) return;

        gameActive = true;
        score = 0;
        stackBasePosition = robot != null ? robot.transform.position + transform.forward * 3f : Vector3.zero;
        stackedCubes.Clear();
        Debug.Log("Cube Stacking Game Started!");
    }

    public void EndGame()
    {
        gameActive = false;
        // Clean up cubes?
        Debug.Log("Game ended with score: " + score);
    }

    public void AddCubeToStack(Vector3 position)
    {
        if (!gameActive || stackedCubes.Count >= maxCubes) return;

        // Instantiate or spawn cube at position
        GameObject cubeObj = Instantiate(cubePrefab, position, Quaternion.identity);
        StackableCube cube = cubeObj.GetComponent<StackableCube>();
        if (cube == null)
        {
            cube = cubeObj.AddComponent<StackableCube>();
        }

        // Place on top of stack
        float stackHeight = stackedCubes.Count * cubeSpacing;
        Vector3 placePos = stackBasePosition + Vector3.up * stackHeight;
        cube.PlaceOnSurface(placePos);

        stackedCubes.Add(cube);
        score++;

        // Reward robot
        if (robot != null)
        {
            robot.memory.RecordInteraction();
            robot.stateManager.SetState(BotState.Excited);
        }
    }

    public void OnStackBroken(StackableCube brokenCube)
    {
        if (stackedCubes.Contains(brokenCube))
        {
            // Stack broken, maybe end game or penalize
            Debug.Log("Stack broken!");
            if (robot != null)
            {
                robot.stateManager.SetState(BotState.Playful); // Or disappointed
            }
            // Could reset part of stack or end
        }
    }

    // For robot interaction
    public Vector3 GetNextStackPosition()
    {
        float stackHeight = stackedCubes.Count * cubeSpacing;
        return stackBasePosition + Vector3.up * stackHeight;
    }

    public bool CanAddCube => gameActive && stackedCubes.Count < maxCubes;

    void OnDrawGizmosSelected()
    {
        if (gameActive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(stackBasePosition, Vector3.one * 0.5f);
        }
    }
}
