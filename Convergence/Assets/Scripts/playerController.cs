using UnityEngine;

public class playerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    //layer map to player to prevent player form damaging self
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP;// so player can have health
    [SerializeField] int speed; //to give speed setting for player
    [SerializeField] int sprintSpeed; //set Player sprint setting
    [SerializeField] int jumpSpeed; // set jump setting
    [SerializeField] int maxJumps; // set jump count
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
