using UnityEngine;
using TMPro;

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
            Mission.instance.StartMission();
            UpdateHUD();

            // Hook into mission events here if needed
        }
    }

    private void Update()
    {
        if (Mission.instance != null)
        {
            UpdateHUD();
        }
    }

    private void UpdateHUD()
    {
        // Update mission name and objectives
        missionNameText.text = Mission.instance.missionName;
        objectiveProgressText.text = $"Objectives: {Mission.instance.objectivesCompleted}/{Mission.instance.totalObjectives}";

        // Show/hide panels based on mission state
        missionCompletePanel.SetActive(false);
        missionFailPanel.SetActive(false);

        // A check for mission complete or fail
        if (!Mission.instance.missionStarted)
        {
            if (Mission.instance.objectivesCompleted >= Mission.instance.totalObjectives)
            {
                missionCompletePanel.SetActive(true);
            }
            else
            {
                missionFailPanel.SetActive(true);
            }
        }
    }
}

