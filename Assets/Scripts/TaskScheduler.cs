using UnityEngine;
using System.Collections.Generic;

public class TaskScheduler : MonoBehaviour
{
    private RobotController robot;
    private List<ScheduledTask> tasks = new List<ScheduledTask>();

    [System.Serializable]
    public class ScheduledTask
    {
        public float time;
        public string description;
        public System.Action action;
    }

    void Start()
    {
        robot = FindObjectOfType<RobotController>();
        ScheduleInitialTasks();
    }

    void Update()
    {
        for (int i = tasks.Count - 1; i >= 0; i--)
        {
            if (Time.time >= tasks[i].time)
            {
                tasks[i].action?.Invoke();
                tasks.RemoveAt(i);
            }
        }
    }

    private void ScheduleInitialTasks()
    {
        // Schedule proactive behaviors
        ScheduleTask(Time.time + 60f, "Morning greeting", () =>
        {
            if (robot != null)
                robot.stateManager.SetState(BotState.Excited);
        });

        ScheduleTask(Time.time + 3600f, "Hourly activity check", () =>
        {
            if (robot != null && robot.memory.activityLevel < 0.5f)
                robot.stateManager.SetState(BotState.Curious);
        });
    }

    public void ScheduleTask(float time, string desc, System.Action action)
    {
        tasks.Add(new ScheduledTask { time = time, description = desc, action = action });
    }
}
