using UnityEngine;
using System.Collections.Generic;

public class GestureRecognition : MonoBehaviour
{
    [SerializeField] private float minimumDistance = 50f;
    [SerializeField] private float maximumTime = 2f;

    private List<Vector2> points = new List<Vector2>();
    private float startTime;
    private bool isTracking;

    private RobotController robot;

    void Start()
    {
        robot = FindObjectOfType<RobotController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartTracking();
        }

        if (isTracking)
        {
            TrackPoint();
        }

        if (Input.GetMouseButtonUp(0))
        {
            RecognizeGesture();
        }
    }

    private void StartTracking()
    {
        points.Clear();
        points.Add(Input.mousePosition);
        startTime = Time.time;
        isTracking = true;
    }

    private void TrackPoint()
    {
        Vector2 currentPoint = Input.mousePosition;
        if (Vector2.Distance(points[points.Count - 1], currentPoint) > 10f)
        {
            points.Add(currentPoint);
        }
    }

    private void RecognizeGesture()
    {
        isTracking = false;

        // Add null check and length
        if (points == null || points.Count < 10 || Time.time - startTime > maximumTime)
        {
            return;
        }

        Vector2 start = points[0];
        Vector2 end = points[points.Count - 1];

        if (Vector2.Distance(start, end) < minimumDistance)
        {
            // Check for circle
            if (IsCircle())
            {
                Debug.Log("Circle gesture detected!");
                if (robot != null)
                {
                    robot.stateManager.SetState(BotState.Excited);
                    robot.OnInteraction();
                }
            }
        }
        else
        {
            // Check for wave (side-to-side)
            if (Vector2.Distance(start, new Vector2(end.x, start.y)) > minimumDistance &&
                Mathf.Abs(end.y - start.y) < minimumDistance / 2)
            {
                Debug.Log("Wave gesture detected!");
                if (robot != null)
                {
                    robot.stateManager.SetState(BotState.Playful);
                    robot.OnInteraction();
                }
            }
        }
    }

    private bool IsCircle()
    {
        Vector2 center = Vector2.zero;
        foreach (var p in points)
        {
            center += p;
        }
        center /= points.Count;

        float radiusVariance = 0f;
        float avgRadius = 0f;
        foreach (var p in points)
        {
            float r = Vector2.Distance(center, p);
            avgRadius += r;
        }
        avgRadius /= points.Count;

        foreach (var p in points)
        {
            float r = Vector2.Distance(center, p);
            radiusVariance += Mathf.Pow(r - avgRadius, 2);
        }

        return radiusVariance / points.Count < 10000f; // Low variance = circle
    }
}
