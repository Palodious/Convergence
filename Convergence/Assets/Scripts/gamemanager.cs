using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class gamemanager : MonoBehaviour
{
    public static gamemanager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    public TMP_Text gameGoalCountText;
    public Image playerHPBar;
    public Image playerEnergyBar;
    public Image playerAmmoBar;
    public Image playerDamageIndicator;

    public GameObject player;
    public playerController controller;

    public bool isPaused;

    float timeScaleOrig;

    int totalObjectives = 1; 
    int completedObjectives = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerController playerController1 = player.GetComponent<playerController>();
        playerController playerController = playerController1;
        controller = playerController;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }
    }

    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void RegisterObjectiveComplete()
    {
        completedObjectives++;
        UpdateGoalUI();

        if (completedObjectives >= totalObjectives)
        {
            OnMissionComplete(true);
        }
    }

    void UpdateGoalUI()
    {
        gameGoalCountText.text = $"{completedObjectives} / {totalObjectives}";
    }

    void OnMissionComplete(bool success)
    {
        statePause();

        if (success)
        {
            menuActive = menuWin;
        }
        else
        {
            menuActive = menuLose;
        }
        menuActive.SetActive(true);
    }

    public void youLose()
    {
        OnMissionComplete(false);
    }
}
