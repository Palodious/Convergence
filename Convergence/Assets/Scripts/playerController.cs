using System.Collections;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    // -------------------------------
    // --- Player Movement Settings ---
    // -------------------------------
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] float speed;
    [SerializeField] int sprintMod;
    [SerializeField] float jumpHeight;
    [SerializeField] float jumpSpeed;
    [SerializeField] int maxJumps;
    [SerializeField] float gravity;
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
    [SerializeField] float crouchSpeedMod = 0.5f; // Movement speed reduction while crouched
    [SerializeField] float crouchHeight = 1.0f; // Height of character while crouched
    float originalHeight; // Stores original standing height
    // --------------------------
    // --- Combat & Shooting ---
    // --------------------------
    [SerializeField] int shootDamage;
    [SerializeField] float shootDist;
    [SerializeField] float shootRate;
    [SerializeField] int ammo; // Current ammo
    [SerializeField] int maxAmmo; // Maximum ammo capacity
    [SerializeField] float shootEnergyCost = 5f; // Energy cost per shot (adjustable in Inspector)
    // ------------------------
    // --- Health & Shields ---
    // ------------------------
    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] float healthRegenRate; // Rate of HP restoration
    [SerializeField] float healthRegenDelay; // Delay before HP starts regenerating
    [SerializeField] int shield; // Current shield amount
    [SerializeField] int maxShield; // Maximum shield capacity
    [SerializeField] float shieldRegenRate; // Rate of shield recovery
    [SerializeField] float shieldRegenDelay; // Delay before shield begins recovering
    [SerializeField] Renderer model; // Player model renderer for visual feedback (flashes, effects)
    // ------------------------
    // --- Energy System ---
    // ------------------------
    [SerializeField] float energy; // Current energy amount
    [SerializeField] float maxEnergy = 100f; // Maximum energy capacity
    [SerializeField] float energyRegenRate = 15f; // Rate of energy regeneration per second
    [SerializeField] float energyRegenDelay = 2f; // Delay before energy starts regenerating
    float energyRegenTimer; // Timer for energy regeneration
    // ---------------------
    // --- Player Boosts ---
    // ---------------------
    [HideInInspector] public float damageBoost = 1f; // Used by abilities for temporary damage increase
    // ------------------------
    // --- Internal Tracking ---
    // ------------------------
    Vector3 moveDir;
    Vector3 playerVel;
    int jumpCount;
    int HPOrig; // Stores original HP for reference
    float shootTimer;
    float healthRegenTimer; // Timer for HP regeneration
    float shieldRegenTimer; // Timer for shield regeneration
    bool shieldBroken; // True when shield is depleted
    bool isSliding; // True while slide coroutine is active
    bool isDodging; // True while dodge coroutine is active
    bool isWallRunning; // True while wall-run is active
    bool canWallRun = true; // Prevents instant re-wall-running
    bool isCrouching; // True when crouching
    float dodgeTimer; // Timer for dodge cooldown
    float wallRunTimer; // Timer for wall-run duration
    bool isGliding; // Tracks if currently gliding
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
        HPOrig = HP;
        if (gamemanager.instance != null)
            updatePlayerUI(); // safe null-checked
        HP = maxHP;
        shield = maxShield;
        energy = maxEnergy;
        originalHeight = controller.height;
        shieldBroken = false;
        if (model != null)
            colorOrig = model.material.color; // Store original model color
    }

    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red); // Shows firing direction

        shootTimer += Time.deltaTime;
        dodgeTimer += Time.deltaTime;

        movement(); // Handles all basic movement inputs
        sprint(); // Handles sprint start/stop input

        // --- Shooting ---
        if (Input.GetMouseButton(0) && shootTimer >= shootRate)
        {
            shoot();
        }

        // --- Glide Activation ---
        if (!controller.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.G))
                StartGlide();
            if (Input.GetKeyUp(KeyCode.G))
                StopGlide();
        }
        else if (isGliding)
        {
            StopGlide(); // Stop glide on landing
        }

        handleHealthRegen(); // Restores HP gradually
        handleShieldRegen(); // Restores shield gradually
        handleEnergyRegen(); // Restores Energy gradually
    }

    void movement()
    {
        // --- Gravity & Ground Check ---
        if (controller.isGrounded)
        {
            if (playerVel.y < 0)
                playerVel.y = -2f; // keeps player grounded
            jumpCount = 0;
        }
        else
        {
            // --- Gravity / Glide Control ---
            // Applies normal gravity unless gliding (press G mid-air)
            if (isGliding)
                playerVel.y = Mathf.Max(playerVel.y - (glideGravity * 0.2f * Time.deltaTime), -glideGravity); // gentle descent while gliding
            else
                playerVel.y -= gravity * Time.deltaTime; // regular fall speed
        }

        // --- Directional Movement ---
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump(); // Handle jump input
        controller.Move(playerVel * Time.deltaTime); // Apply vertical motion (jump / gravity)

        // --- Advanced Movement Inputs ---
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.C) && !isSliding && controller.isGrounded)
            StartCoroutine(PerformSlide()); // Slide when holding shift + crouch
        else if (Input.GetKey(KeyCode.C))
            crouch(); // Hold C to crouch
        else if (Input.GetKeyUp(KeyCode.C))
            uncrouch(); // Release C to stand

        // --- Glide Activation ---
        if (!controller.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.G))
                StartGlide(); // Start gliding in mid-air
            if (Input.GetKeyUp(KeyCode.G))
                StopGlide(); // Stop gliding when G is released
        }
        else if (isGliding)
        {
            StopGlide(); // Automatically stop glide upon landing
        }

        // --- Wall Running ---
        if (!isWallRunning && canWallRun)
            CheckWallRun(); // Detect and initiate wall run
    }


    void sprint()
    {
        // Multiplies or divides speed on sprint key press/release
        if (Input.GetKeyDown(KeyCode.LeftShift))
            speed *= sprintMod;
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            speed /= sprintMod;
    }

    void jump()
    {
        // Handles jumping logic, including multiple jumps
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            // Give a strong upward push
            playerVel.y = jumpSpeed * jumpHeight;
            jumpCount++;
        }
    }

    void crouch()
    {
        if (!isCrouching)
        {
            isCrouching = true;
            controller.height = crouchHeight;
            speed *= crouchSpeedMod;
        }
    }

    void uncrouch()
    {
        if (isCrouching)
        {
            isCrouching = false;
            controller.height = originalHeight;
            speed /= crouchSpeedMod;
        }
    }

    void shoot()
    {
        // Handles shooting logic with ammo and energy checks
        if (ammo <= 0 || !CanUseEnergy(shootEnergyCost)) return; // Prevent fire if out of ammo or energy
        UseEnergy(shootEnergyCost);

        shootTimer = 0; // Resets cooldown
        ammo--;         // Consume one round

        RaycastHit hit;
        // Fires ray from camera forward
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name); // Logs hit object name
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage((int)(shootDamage * damageBoost)); // Applies modified damage if boost active
        }
    }

    IEnumerator PerformSlide()
    {
        isSliding = true; // Locks slide state
        float elapsed = 0f;

        Vector3 slideDir = moveDir.normalized;
        if (slideDir == Vector3.zero) slideDir = transform.forward; // Defaults to facing direction

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            controller.Move(slideDir * slideSpeed * Time.deltaTime);
            yield return null;
        }

        isSliding = false; // Ends slide state
    }

    void StartGlide()
    {
        if (!controller.isGrounded && !isGliding)
        {
            isGliding = true;
            playerVel.y = -1f; // small downward force to start descent
        }
    }

    void StopGlide()
    {
        if (isGliding)
            isGliding = false;
    }

    IEnumerator Glide()
    {
        while (isGliding && !controller.isGrounded)
        {
            playerVel.y = Mathf.Max(playerVel.y - glideGravity * Time.deltaTime, -glideGravity);
            controller.Move(playerVel * Time.deltaTime);
            yield return null;
        }
    }

    void CheckWallRun()
    {
        RaycastHit leftHit, rightHit;

        // Casts rays on both sides to detect walls
        bool leftWall = Physics.Raycast(transform.position, -transform.right, out leftHit, wallCheckDistance);
        bool rightWall = Physics.Raycast(transform.position, transform.right, out rightHit, wallCheckDistance);

        if (leftWall) StartCoroutine(PerformWallRun(leftHit.normal));
        else if (rightWall) StartCoroutine(PerformWallRun(rightHit.normal));
    }

    IEnumerator PerformWallRun(Vector3 wallNormal)
    {
        isWallRunning = true;
        canWallRun = false;
        wallRunTimer = 0f;

        while (wallRunTimer < wallRunDuration && !controller.isGrounded)
        {
            wallRunTimer += Time.deltaTime;
            Vector3 alongWall = Vector3.Cross(wallNormal, Vector3.up);
            controller.Move(alongWall * wallRunSpeed * Time.deltaTime);
            playerVel.y = 0;
            yield return null;
        }

        isWallRunning = false;
        yield return new WaitForSeconds(wallRunCooldown);
        canWallRun = true;
    }

    void handleHealthRegen()
    {
        // Regenerates health slowly after delay if below max
        if (HP < maxHP && HP > 0)
        {
            healthRegenTimer += Time.deltaTime;
            if (healthRegenTimer >= healthRegenDelay)
            {
                HP += Mathf.CeilToInt(healthRegenRate * Time.deltaTime);
                HP = Mathf.Clamp(HP, 0, maxHP);
            }
        }
    }

    void handleShieldRegen()
    {
        // Regenerates shield if not broken and below max
        if (shield < maxShield && !shieldBroken)
        {
            shieldRegenTimer += Time.deltaTime;
            if (shieldRegenTimer >= shieldRegenDelay)
            {
                shield += Mathf.CeilToInt(shieldRegenRate * Time.deltaTime);
                shield = Mathf.Clamp(shield, 0, maxShield);
            }
        }
        else if (shieldBroken && shield >= maxShield)
            resetShieldBreak();
    }

    void handleEnergyRegen()
    {
        // Regenerates energy after delay
        if (energy < maxEnergy)
        {
            energyRegenTimer += Time.deltaTime;
            if (energyRegenTimer >= energyRegenDelay)
            {
                energy += energyRegenRate * Time.deltaTime;
                energy = Mathf.Clamp(energy, 0, maxEnergy);
            }
        }
    }

    public bool CanUseEnergy(float amount)
    {
        return energy >= amount;
    }

    public void UseEnergy(float amount)
    {
        energy -= amount;
        energyRegenTimer = 0f;
    }

    void triggerShieldBreak()
    {
        shieldBroken = true;
        shield = 0;
        if (model != null)
        {
            model.material.color = Color.blue;
            StartCoroutine(ResetModelColor());
        }
        Debug.Log("Shield Broken!");
    }

    void resetShieldBreak()
    {
        shieldBroken = false;
        Debug.Log("Shield Restored!");
    }

    IEnumerator ResetModelColor()
    {
        yield return new WaitForSeconds(0.2f);
        if (model != null)
            model.material.color = colorOrig;
    }

    public void addAmmo(int value)
    {
        ammo += value;
        if (ammo > maxAmmo)
            ammo = maxAmmo;
    }

    public void takeDamage(int amount)
    {
        if (shield > 0)
        {
            shield -= amount;
            shieldRegenTimer = 0;

            if (model != null)
            {
                model.material.color = Color.cyan;
                StartCoroutine(ResetModelColor());
            }

            if (shield <= 0)
                triggerShieldBreak();
            return;
        }

        HP -= amount;
        updatePlayerUI();
        StartCoroutine(screenFlashDamage());
        healthRegenTimer = 0;

        if (model != null)
        {
            model.material.color = Color.red;
            StartCoroutine(ResetModelColor());
        }

        if (HP <= 0)
            gamemanager.instance.youLose(); // Triggers game over
    }

    public void updatePlayerUI()
    {
        if (gamemanager.instance != null && gamemanager.instance.playerHPBar != null)
            gamemanager.instance.playerHPBar.fillAmount = (float)HP / maxHP;
    }
    IEnumerator screenFlashDamage()
    {
        if (gamemanager.instance.playerDamageIndicator != null)
            gamemanager.instance.playerDamageIndicator.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        if (gamemanager.instance.playerDamageIndicator != null)
            gamemanager.instance.playerDamageIndicator.gameObject.SetActive(false);
    }
}
