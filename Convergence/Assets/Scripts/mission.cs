using System;
using UnityEngine;

public class Mission : MonoBehaviour
{
    public static Mission instance;

    [Header("Mission Info")]
    public string missionName;
    public int totalObjectives = 1;
    public int objectivesCompleted { get; private set; } = 0;
    public bool missionStarted { get; private set; } = false;

    public event Action<int, int> OnObjectiveProgressChanged;
    public event Action<bool> OnMissionCompleted;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void StartMission()
    {
        if (missionStarted) 
            return;

        missionStarted = true;
        objectivesCompleted = 0;
        OnObjectiveProgressChanged?.Invoke(objectivesCompleted, totalObjectives);

        Debug.Log($"Mission '{missionName}' started!");
    }

    public void CompleteObjective()
    {
        if (!missionStarted) 
            return;

        objectivesCompleted++;
        OnObjectiveProgressChanged?.Invoke(objectivesCompleted, totalObjectives);

        Debug.Log($"Objective completed: {objectivesCompleted}/{totalObjectives}");

        if (objectivesCompleted >= totalObjectives)
        {
            CompleteMission();
        }
    }

    public void CompleteMission()
    {
        missionStarted = false;
        OnMissionCompleted?.Invoke(true);
        Debug.Log($"Mission '{missionName}' completed!");
    }

    public void FailMission()
    {
        missionStarted = false;
        OnMissionCompleted?.Invoke(false);
        Debug.Log($"Mission '{missionName}' failed.");
    }
}

