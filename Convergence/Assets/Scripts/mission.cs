using UnityEngine;
using TMPro;

public class Mission : MonoBehaviour
{
    public static Mission instance;
    public string missionName;
    private string missionDescription;
    public int totalObjectives;
    private TMP_Text objectivesText;
    private GameObject missionActiveUI;
    private GameObject missionCompleteUI;
    private GameObject missionFailUI;
    public int objectivesCompleted;
    public bool missionStarted;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateObjectiveUI();
        missionActiveUI.SetActive(false);
        missionCompleteUI.SetActive(false);
        missionFailUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartMission();
        }
    }

    public void StartMission()
    {
        if (missionStarted)
            return;

        missionStarted = true;
        objectivesCompleted = 0;
        missionActiveUI.SetActive(true);
        missionCompleteUI.SetActive(false);
        missionFailUI.SetActive(false);
        UpdateObjectiveUI();

        Debug.Log($"Mission '{missionName}' started!");
    }

    public void CompleteObjective()
    {
        if (!missionStarted)
            return;

        objectivesCompleted++;
        UpdateObjectiveUI();
        Debug.Log($"Objective completed! {objectivesCompleted} / {totalObjectives}");

        if (objectivesCompleted >= totalObjectives)
        {
            CompleteMission();
        }
    }

    public void FailMission()
    {
        if (!missionStarted)
            return;

        missionStarted = false;
        missionActiveUI.SetActive(false);
        missionFailUI.SetActive(true);

        Debug.Log($"Mission '{missionName}' failed!");
    }

    private void CompleteMission()
    {
        missionStarted = false;
        missionActiveUI.SetActive(false);
        missionCompleteUI.SetActive(true);

        Debug.Log($"Mission '{missionName}' completed!");
    }

    private void UpdateObjectiveUI()
    {
        if (objectivesText != null)
            objectivesText.text = $"Objectives: {objectivesCompleted}/{totalObjectives}";
    }
}

