using UnityEngine;

public class playerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    //layer map to player to prevent player form damaging self
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP;// so player can have health
    [SerializeField] int speed; //to give speed setting for player
    [SerializeField] int sprintMod; //set Player sprint setting
    [SerializeField] int JumpSpeed; // set jump setting
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
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        shootTimer += Time.deltaTime;

        movement();
        sprint();

    }

    void movement()
    {
        if (controller.isGrounded) //sets up player movement and jump
        {
            playerVel = Vector3.zero;
            jumpCount = 0;
        }
        else    // gives gravity so player returns to griound after jumping
        {
            playerVel.y -= gravity * Time.deltaTime;
        }
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);
        //fires weapon when butto pressed
        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
    }
    // sprint function set
    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    void jump() // jump function
    {
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            playerVel.y = JumpSpeed;
            jumpCount++;
        }
    }

    void shoot() // shoot function using raycast
    {
        shootTimer = 0;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }
    }

    public void takeDamage(int amount) //take damage
    {
        HP -= amount;
        if (HP <= 0)
        {
            // You Lose!!!
            gamemanager.instance.youLose();
        }
    }


}
