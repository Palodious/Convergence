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

    //slide
    [SerializeField] float slideDuration = 1f; 
    [SerializeField] float slideSpeed = 12f;
    //wall run
    [SerializeField] float wallRunSpeed = 7f;
    [SerializeField] float wallRunGravity = 5f;
    [SerializeField] float wallCheckDist = 1f;
    [SerializeField] LayerMask wallMask;
    // glide 
    [SerializeField] float glideGravityScale = 0.3f;
    //crouch
    [SerializeField] float crouchHeight = 1f;
    [SerializeField] float normalHeight = 2f;
    [SerializeField] float crouchSpeedMod = 0.5f;
    //dodge
    [SerializeField] float dodgeSpeed = 15f;
    [SerializeField] float dodgeDuration = 0.25f;
    [SerializeField] float dodgeCooldown = 1f;


    //Player movement
    Vector3 moveDir;
    Vector3 playerVel;
    Vector3 wallNormal;
    Vector3 dodgeDir;

    int jumpCount; // for double jumps
    float shootTimer; 
    float slideTimer;
    float dodgeTimer;
    float dodgeCDTimer;

    bool isSliding;
    bool isWallRunning; //for wall run
    bool isGliding; // for glide
    bool isCrouching; // for crouch
    bool isDodging; // for dodge

     

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
        if (isDodging)
        {
            dodgeTimer += Time.deltaTime;
            controller.Move(dodgeDir * dodgeSpeed * Time.deltaTime);
            if (dodgeTimer >= dodgeDuration) stopDodge();
            return;
        }

        if (controller.isGrounded)
        {
            playerVel = Vector3.zero;
            jumpCount = 0;
            isWallRunning = false;
            isGliding = false;
        }
        else
        {
            if (!isWallRunning && !isGliding)
                playerVel.y -= gravity * Time.deltaTime;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;

        float moveSpeed = isCrouching ? speed * crouchSpeedMod : speed;
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
            jump();

        if (!controller.isGrounded && playerVel.y < 0 && Input.GetButton("Jump"))
            startGlide();
        else if (isGliding && (!Input.GetButton("Jump") || controller.isGrounded))
            stopGlide();

        if (!controller.isGrounded && Input.GetAxis("Vertical") > 0)
            checkWallRun();
        else if (isWallRunning && controller.isGrounded)
            isWallRunning = false;

        if (Input.GetButtonDown("Crouch") && controller.isGrounded && !isSliding)
            toggleCrouch();

        if (Input.GetButtonDown("Slide") && controller.isGrounded && !isSliding)
            startSlide();

        if (isSliding)
        {
            slideTimer += Time.deltaTime;
            controller.Move(transform.forward * slideSpeed * Time.deltaTime);
            if (slideTimer >= slideDuration)
                stopSlide();
        }

        if (Input.GetButtonDown("Dodge") && dodgeCDTimer >= dodgeCooldown && !isDodging)
            startDodge();

        controller.Move(playerVel * Time.deltaTime);

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
            shoot();
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
    // Slide start and stop
    void startSlide()
    {
        isSliding = true;
        slideTimer = 0;
        controller.height = crouchHeight;
    }
    void stopSlide()
    {
        isSliding = false;
        controller.height = normalHeight;
    }

    // Crouch toggle
    void toggleCrouch()
    {
        isCrouching = !isCrouching;
        controller.height = isCrouching ? crouchHeight : normalHeight;
    }

    // Glide
    void startGlide()
    {
        isGliding = true;
        playerVel.y = Mathf.Max(playerVel.y, -gravity * glideGravityScale);
    }
    void stopGlide() { isGliding = false; }

    // Wall-run
    void checkWallRun()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.right, out hit, wallCheckDist, wallMask))
            startWallRun(hit.normal);
        else if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDist, wallMask))
            startWallRun(hit.normal);
        else if (isWallRunning) stopWallRun();
    }
    void startWallRun(Vector3 normal)
    {
        isWallRunning = true;
        wallNormal = normal;
        playerVel.y = 0;
        Vector3 alongWall = Vector3.Cross(wallNormal, Vector3.up);
        controller.Move(alongWall * wallRunSpeed * Time.deltaTime);
        playerVel.y -= wallRunGravity * Time.deltaTime;
    }
    void stopWallRun() { isWallRunning = false; }

    // Dodge Roll
    void startDodge()
    {
        isDodging = true;
        dodgeTimer = 0;
        dodgeCDTimer = 0;
        dodgeDir = transform.forward;
    }
    void stopDodge()
    {
        isDodging = false;
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


    // Slide start and stop
    void startSlide()
    {
        isSliding = true;
        slideTimer = 0;
        controller.height = crouchHeight;
    }
    void stopSlide()
    {
        isSliding = false;
        controller.height = normalHeight;
    }

}
