using UnityEngine;
using TMPro;

// Controls the mission UI elements, displaying mission name,
// objective progress, and completion/failure messages.
// Listens to Mission events for live updates.

public class MissionHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text missionNameText;
    [SerializeField] private TMP_Text objectiveProgressText;
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private GameObject missionFailPanel;

    private void OnEnable()
    {
        if (Mission.instance != null)
        {
            // Initialize HUD with mission data
            missionNameText.text = Mission.instance.missionName;
            UpdateObjectiveProgress(0, Mission.instance.totalObjectives);

            // Hide mission end panels initially
            missionCompletePanel.SetActive(false);
            missionFailPanel.SetActive(false);

            // Subscribe to mission events to update UI dynamically
            Mission.instance.OnObjectiveProgressChanged += UpdateObjectiveProgress;
            Mission.instance.OnMissionCompleted += OnMissionCompleted;
        }
    }

    private void OnDisable()
    {
        if (Mission.instance != null)
        {
            // Unsubscribe from events to prevent memory leaks
            Mission.instance.OnObjectiveProgressChanged -= UpdateObjectiveProgress;
            Mission.instance.OnMissionCompleted -= OnMissionCompleted;
        }
    }

    // Updates objective count text.
    private void UpdateObjectiveProgress(int completed, int total)
    {
        objectiveProgressText.text = $"Objectives: {completed} / {total}";
    }

    // Shows mission complete or fail panel based on mission outcome.
    private void OnMissionCompleted(bool success)
    {
        if (success)
        {
            missionCompletePanel.SetActive(true);
            missionFailPanel.SetActive(false);
        }
        else
        {
            missionCompletePanel.SetActive(false);
            missionFailPanel.SetActive(true);
        }
    }
}

