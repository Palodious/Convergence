using UnityEngine;
using TMPro;

public class MissionHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text missionNameText;
    [SerializeField] private TMP_Text objectiveProgressText;
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private GameObject missionFailPanel;

    private void OnEnable()
    {
        if (Mission.instance != null)
        {
            missionNameText.text = Mission.instance.missionName;
            UpdateObjectiveProgress(0, Mission.instance.totalObjectives);
            missionCompletePanel.SetActive(false);
            missionFailPanel.SetActive(false);

            Mission.instance.OnObjectiveProgressChanged += UpdateObjectiveProgress;
            Mission.instance.OnMissionCompleted += OnMissionCompleted;
        }
    }

    private void OnDisable()
    {
        if (Mission.instance != null)
        { 
            Mission.instance.OnObjectiveProgressChanged -= UpdateObjectiveProgress;
            Mission.instance.OnMissionCompleted -= OnMissionCompleted;
        }
    }

    private void UpdateObjectiveProgress(int completed, int total)
    {
        objectiveProgressText.text = $"Objectives: {completed} / {total}";
    }

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

