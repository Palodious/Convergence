using System.Collections;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    // -------------------------------
    // --- Player Movement Settings ---
    // -------------------------------
    [SerializeField] CharacterController controller; // Character controller used for movement and collisions
    [SerializeField] LayerMask ignoreLayer; // Layers to ignore in raycasts
    [SerializeField] float speed; // Normal walking speed
    [SerializeField] int sprintMod; // Speed multiplier when sprinting
    [SerializeField] float jumpSpeed; // Upward velocity when jumping
    [SerializeField] int maxJumps; // Number of allowed jumps before landing
    [SerializeField] float gravity; // Gravitational force applied to player
    // -----------------------------
    // --- Advanced Movement ---
    // -----------------------------
    [SerializeField] float slideSpeed; // Speed during slide
    [SerializeField] float slideDuration; // Duration of slide movement
    [SerializeField] float dodgeDistance; // Distance covered during a dodge roll
    [SerializeField] float dodgeCooldown; // Time before dodge can be used again
    [SerializeField] float glideGravity; // Gravity applied when gliding (lower for floaty effect)
    [SerializeField] float wallRunSpeed; // Speed while running along a wall
    [SerializeField] float wallRunDuration; // Maximum wall-run duration
    [SerializeField] float wallRunCooldown; // Time before wall-run can be reused
    [SerializeField] float wallCheckDistance; // Distance to check for wall proximity (raycast)
    // --------------------------
    // --- Combat & Shooting ---
    // --------------------------
    [SerializeField] int shootDamage; // Damage dealt per shot
    [SerializeField] float shootDist; // Distance bullet travels (raycast)
    [SerializeField] float shootRate; // Time between shots
    [SerializeField] int ammo; // Current ammo
    [SerializeField] int maxAmmo; // Maximum ammo capacity
    // ------------------------
    // --- Health & Shields ---
    // ------------------------
    [SerializeField] int HP; // Current health points
    [SerializeField] int maxHP; // Maximum HP value
    [SerializeField] float healthRegenRate; // Rate of HP restoration
    [SerializeField] float healthRegenDelay; // Delay before HP starts regenerating
    [SerializeField] int shield; // Current shield amount
    [SerializeField] int maxShield; // Maximum shield capacity
    [SerializeField] float shieldRegenRate; // Rate of shield recovery
    [SerializeField] float shieldRegenDelay; // Delay before shield begins recovering
    [SerializeField] Renderer model; // Player model renderer for visual feedback (flashes, effects)
    // ---------------------
    // --- Player Boosts ---
    // ---------------------
    [HideInInspector] public float damageBoost = 1f; // Used by abilities for temporary damage increase
    // ------------------------
    // --- Internal Tracking ---
    // ------------------------
    Vector3 moveDir; // Stores direction of movement
    Vector3 playerVel; // Stores vertical velocity (for jump / gravity)
    int jumpCount; // Tracks current jump count
    float shootTimer; // Timer to control firing rate
    float healthRegenTimer; // Timer for HP regeneration
    float shieldRegenTimer; // Timer for shield regeneration
    bool shieldBroken; // True when shield is depleted
    bool isSliding; // True while slide coroutine is active
    bool isDodging; // True while dodge coroutine is active
    bool isWallRunning; // True while wall-run is active
    bool canWallRun = true; // Prevents instant re-wall-running
    float dodgeTimer; // Timer for dodge cooldown
    float wallRunTimer; // Timer for wall-run duration
    Color colorOrig; // Stores original model color for feedback resets
    // -------------------
    // --- Properties ---
    // -------------------
    public CharacterController Controller => controller;
    public float Speed { get => speed; set => speed = value; }
    public int HPValue { get => HP; set => HP = value; }
    // Start initializes HP, shield, and visuals
    void Start()
    {
        HP = maxHP;
        shield = maxShield;
        shieldBroken = false;
        if (model != null)
            colorOrig = model.material.color; // Store original model color
    }
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        shootTimer += Time.deltaTime;

        movement();
        sprint();
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            playerVel = Vector3.zero;
            jumpCount = 0;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
    }

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

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            playerVel.y = JumpSpeed;
            jumpCount++;
        }
    }

    void shoot()
    {
        shootTimer = 0;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage((int)(shootDamage * damageBoost));
            }
        }
    }

    public void addAmmo(int value)
    {
        ammo += value;
        if (ammo > maxAmmo)
            ammo = maxAmmo;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
            gamemanager.instance.youLose();
        }
    }
}
