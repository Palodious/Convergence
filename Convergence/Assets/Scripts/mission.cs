using System;
using UnityEngine;

// Manages the current mission state, tracks objectives,
// and notifies listeners when objectives are completed or mission ends.
// Singleton pattern for easy global access.
public class Mission : MonoBehaviour
{
    public static Mission instance;

    public string missionName;
    public int totalObjectives = 1;
    public int objectivesCompleted { get; private set; } = 0;
    public bool missionStarted { get; private set; } = false;

    // Event raised when objective progress changes: (completed, total)
    public event Action<int, int> OnObjectiveProgressChanged;

    // Event raised when mission completes or fails: true=success, false=failure
    public event Action<bool> OnMissionCompleted;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Starts the mission and resets progress.
    public void StartMission()
    {
        if (missionStarted)
            return;

        missionStarted = true;
        objectivesCompleted = 0;
        OnObjectiveProgressChanged?.Invoke(objectivesCompleted, totalObjectives);
    }

    // Marks an objective as completed and checks for mission completion.
    public void CompleteObjective()
    {
        CompleteObjective("Unnamed Objective");
    }

    // Marks a named objective as completed and logs it for clarity.
    public void CompleteObjective(string objectiveName)
    {
        if (!missionStarted)
            return;

        objectivesCompleted++;

        Debug.Log($"Objective Completed: {objectiveName} ({objectivesCompleted}/{totalObjectives})");

        OnObjectiveProgressChanged?.Invoke(objectivesCompleted, totalObjectives);

        if (objectivesCompleted >= totalObjectives)
            CompleteMission();
    }

    // Marks the mission as successfully completed.
    public void CompleteMission()
    {
        missionStarted = false;
        OnMissionCompleted?.Invoke(true);
    }

    // Marks the mission as failed.
    public void FailMission()
    {
        missionStarted = false;
        OnMissionCompleted?.Invoke(false);
    }
}