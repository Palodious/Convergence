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
    [SerializeField] int gravity; // set player gravity

    [SerializeField] int shootDamage; //damage output
    [SerializeField] int shootDist; // damage dealt distance
    [SerializeField] float shootRate; //set rate of fire

    //Player movement
    Vector3 moveDir;
    Vector3 playerVel;

    int jumpCount; // for double jumps

    float shootTimer; // time for rounds before disappearing

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
