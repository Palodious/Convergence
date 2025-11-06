using System.Collections;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] float speed;
    [SerializeField] int sprintMod;
    [SerializeField] float jumpHeight;
    [SerializeField] float jumpSpeed;
    [SerializeField] int maxJumps;
    [SerializeField] float gravity;

    [SerializeField] float glideGravity; // Gravity applied when gliding (lower for floaty effect)
    [SerializeField] float crouchSpeedMod = 0.5f; // Movement speed reduction while crouched
    [SerializeField] float crouchHeight = 1.0f; // Height of character while crouched
    float originalHeight; // Stores original standing height

    [SerializeField] int shootDamage;
    [SerializeField] float shootDist;
    [SerializeField] float shootRate;
    [SerializeField] int ammo; // Current ammo
    [SerializeField] int maxAmmo; // Maximum ammo capacity

    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] float healthRegenRate; // Rate of HP restoration
    [SerializeField] float healthRegenDelay; // Delay before HP starts regenerating
    [SerializeField] int shield; // Current shield amount
    [SerializeField] int maxShield; // Maximum shield capacity
    [SerializeField] float shieldRegenRate; // Rate of shield recovery
    [SerializeField] float shieldRegenDelay; // Delay before shield begins recovering
    [SerializeField] Renderer model; // Player model renderer for visual feedback (flashes, effects)

    [SerializeField] float energy; // Current energy amount
    [SerializeField] float maxEnergy = 100f; // Maximum energy capacity
    [SerializeField] float energyRegenRate = 15f; // Rate of energy regeneration per second
    [SerializeField] float energyRegenDelay = 2f; // Delay before energy starts regenerating
    float energyRegenTimer; // Timer for energy regeneration
 
    [HideInInspector] public float damageBoost = 1f; // Used by abilities for temporary damage increase

    Vector3 moveDir;
    Vector3 playerVel;
    int jumpCount;
    int HPOrig; // Stores original HP for reference
    float shootTimer;
    int energyOrig; // Stores original Energy for reference
    int ammoOrig; // Stores original Ammo for reference
    float healthRegenTimer; // Timer for HP regeneration
    float shieldRegenTimer; // Timer for shield regeneration
    bool shieldBroken; // True when shield is depleted
    bool isCrouching; // True when crouching
    bool isGliding; // Tracks if currently gliding
    Color colorOrig; // Stores original model color for feedback resets

    public CharacterController Controller => controller;
    public float Speed { get => speed; set => speed = value; }
    public int HPValue { get => HP; set => HP = value; }

    // Start initializes HP, shield, and visuals
    void Start()
    {
        HPOrig = HP;
        energyOrig = (int)energy;
        ammoOrig = ammo;
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

        movement(); // Handles all basic movement inputs
        sprint(); // Handles sprint start/stop input

        if (Input.GetMouseButton(0) && shootTimer >= shootRate)
        {
            shoot();
        }
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
        if (controller.isGrounded)
        {
            if (playerVel.y < 0)
                playerVel.y = -2f; // keeps player grounded
            jumpCount = 0;
        }
        else
        {
            // Applies normal gravity unless gliding (press G mid-air)
            if (isGliding)
                playerVel.y = Mathf.Max(playerVel.y - (glideGravity * 0.2f * Time.deltaTime), -glideGravity); // gentle descent while gliding
            else
                playerVel.y -= gravity * Time.deltaTime; // regular fall speed
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump(); // Handle jump input
        controller.Move(playerVel * Time.deltaTime); // Apply vertical motion (jump / gravity)

        if (Input.GetKey(KeyCode.C))
            crouch(); // Hold C to crouch
        else if (Input.GetKeyUp(KeyCode.C))
            uncrouch(); // Release C to stand

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
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            // Allows you to jump as high as you want
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
        // Handles shooting logic with ammo checks only
        if (ammo <= 0) return; // Prevent fire if out of ammo

        shootTimer = 0;
        ammo--;

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name); // Logs hit object name
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage((int)(shootDamage * damageBoost)); // Applies modified damage if boost active
        }

        updatePlayerUI(); // keep UI accurate after ammo use
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
        updatePlayerUI();
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
        updatePlayerUI();
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
        if (gamemanager.instance != null && gamemanager.instance.playerEnergyBar != null)
            gamemanager.instance.playerEnergyBar.fillAmount = (float)energy / maxEnergy;
        if (gamemanager.instance != null && gamemanager.instance.playerAmmoBar != null)
            gamemanager.instance.playerAmmoBar.fillAmount = (float)ammo / maxAmmo;
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
