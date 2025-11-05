using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] NavMeshAgent agent;

    [SerializeField] Renderer model;
    [SerializeField] Transform headPos;

    [SerializeField] int HP;

    [SerializeField] int FOV;
    [SerializeField] int faceTargetSpeed;

    [SerializeField] bool canShoot; // Enables or disables shooting from Inspector
    [SerializeField] GameObject projectile; // Prefab for bullet attack
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPOS;

    Color colorOrig;

    bool playerInTrigger;

    float shootTimer;
    float angleToPlayer;
    float stoppingDistOrig;
    Vector3 playerDir;

    [SerializeField] bool hasFlamethrower; // Enables flamethrower attack
    [SerializeField] bool hasShock; // Enables electric shock ability
    [SerializeField] bool hasLeap; // Enables leap and stomp attack
    [SerializeField] bool hasDash; // Enables charging dash attack
    [SerializeField] bool hasMelee; // Enables close-range melee attack

    [SerializeField] GameObject meleePrefab; // Prefab for melee hit
    [SerializeField] float meleeRange; // Distance required to use melee
    [SerializeField] float meleeCD; // Cooldown between melee hits
    [SerializeField] Transform meleePOS; // Spawn point for melee prefab
    float meleeTimer; // Tracks melee cooldown

    [SerializeField] GameObject flamethrowerPrefab; // Prefab for flamethrower attack
    [SerializeField] float flamethrowerDuration; // How long the flamethrower lasts
    [SerializeField] float flamethrowerRange; // Range of flamethrower
    [SerializeField] float flamethrowerCD; // Time before flamethrower can be used again
    [SerializeField] Transform flamethrowerPOS; // Spawn point for flamethrower prefab
    float flamethrowerTimer; // Timer for flamethrower CD

    [SerializeField] GameObject shockPrefab; // Prefab for shock attack
    [SerializeField] float shockRange; // Radius of shockwave
    [SerializeField] float shockCD; // CD time before shock can be used again
    [SerializeField] Transform shockPOS; // Spawn point for shock prefab
    float shockTimer; // Timer for shock ability

    [SerializeField] GameObject leapPrefab; // Prefab for leap attack
    [SerializeField] float stompRadius; // Area that gets hit when landing
    [SerializeField] int leapDamage; // Damage caused when stomping down
    [SerializeField] float leapForce; // Force of jump movement
    [SerializeField] float leapCD; // Time before another leap is possible
    [SerializeField] Transform leapPOS; // Spawn point for leap prefab
    float leapTimer; // Timer for leap CD

    [SerializeField] GameObject dashPrefab; // Prefab for dash attack
    [SerializeField] int dashDamage; // Damage caused by dash impact
    [SerializeField] float dashKnockbackForce; // Force that pushes player backwards
    [SerializeField] float dashSpeed; // Speed of dash attack
    [SerializeField] float dashCD; // Time before dash can be used again
    [SerializeField] Transform dashPOS; // Spawn point for dash prefab
    float dashTimer; // Timer for dash CD

    [SerializeField] int maxShield; // Maximum shield capacity
    [SerializeField] int shieldHP; // Current active shield value, editable in Inspector
    [SerializeField] float shieldRegenRate; // How fast the shield regenerates
    [SerializeField] float shieldDelay; // Delay before shield starts to recharge
    bool shieldBroken; // Checks if shield has been broken
    float shieldTimer; // Timer for shield regen delay

    [SerializeField] Transform[] patrolPoints; // Array of patrol points
    int patrolIndex; // Keeps track of which point it’s moving toward
    bool isPatrolling; // Enables or disables patrol mode
    void Start()
    {
        colorOrig = model.material.color;
        stoppingDistOrig = agent.stoppingDistance;

        shieldHP = maxShield; // Sets shield to max value on start
        isPatrolling = patrolPoints != null && patrolPoints.Length > 0; // Enables patrol if points exist
    }
    void Update()
    {
        shootTimer += Time.deltaTime;
        meleeTimer += Time.deltaTime;
        flamethrowerTimer += Time.deltaTime;
        shockTimer += Time.deltaTime;
        leapTimer += Time.deltaTime;
        dashTimer += Time.deltaTime;

        bool seesPlayer = playerInTrigger && canSeePlayer();

        if (seesPlayer)
        {
            agent.stoppingDistance = stoppingDistOrig;
            agent.SetDestination(gamemanager.instance.player.transform.position);
            faceTarget();

            float distToPlayer = Vector3.Distance(transform.position, gamemanager.instance.player.transform.position);

            if (hasMelee && distToPlayer <= meleeRange && meleeTimer >= meleeCD)
            {
                Melee(); // Performs melee hit if within range
            }

            if (canShoot && shootTimer >= shootRate && distToPlayer > meleeRange)
            {
                Shoot(); // Fires bullet when timer reaches rate
            }

            if (hasFlamethrower && flamethrowerTimer >= flamethrowerCD)
            {
                StartCoroutine(Flamethrower()); // Starts flamethrower ability
            }

            if (hasShock && shockTimer >= shockCD)
            {
                StartCoroutine(Shock()); // Triggers electrical shock
            }

            if (hasLeap && leapTimer >= leapCD)
            {
                StartCoroutine(Leap()); // Performs leap and stomp attack
            }

            if (hasDash && dashTimer >= dashCD)
            {
                StartCoroutine(Dash()); // Performs charging dash
            }
        }
        else if (isPatrolling)
        {
            agent.stoppingDistance = 0;
            Patrol(); // Moves between patrol points when player not detected
        }

        ShieldRegen(); // Handles shield regeneration when possible
    }
    bool canSeePlayer()
    {
        playerDir = gamemanager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        if (angleToPlayer > FOV * 0.5f) return false;

        RaycastHit hit;
        Vector3 rayStart = headPos.position + transform.forward * 0.2f; // prevent self-hit
        if (Physics.Raycast(rayStart, playerDir.normalized, out hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
    void faceTarget()
    {
        Vector3 dir = gamemanager.instance.player.transform.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceTargetSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }
    public void takeDamage(int amount)
    {
        if (shieldHP > 0)
        {
            shieldHP -= amount; // Damage absorbed by shield first

            if (shieldHP <= 0)
            {
                shieldHP = 0;
                shieldBroken = true; // Marks shield as broken
                StartCoroutine(flashShieldBreak()); // Plays break effect
            }
            else
            {
                StartCoroutine(flashShield()); // Flashes when shield takes damage
            }

            shieldTimer = 0; // Resets shield regen timer
            return;
        }

        HP -= amount;

        if (HP <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }
    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
    IEnumerator flashShield()
    {
        model.material.color = Color.yellow; // Flashes yellow when shield absorbs hit
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
    IEnumerator flashShieldBreak()
    {
        model.material.color = Color.white; // Changes color to white when shield is broken
        yield return new WaitForSeconds(0.2f);
        model.material.color = colorOrig;
    }
    void Shoot()
    {
        shootTimer = 0; // Resets shoot cooldown timer
        Instantiate(projectile, shootPOS.position, transform.rotation); // Spawns projectile prefab from shoot position
    }
    void Melee()
    {
        meleeTimer = 0; // Resets melee cooldown
        Instantiate(meleePrefab, shootPOS.position, transform.rotation); // Spawns melee prefab for visual/damage zone
    }
    void Patrol()
    {
        // Moves between patrol points if there are assigned points
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length; // Loops back to first point after reaching last
            agent.SetDestination(patrolPoints[patrolIndex].position); // Updates next patrol target position
        }
    }
    void ShieldRegen()
    {
        // Regenerates shield over time if not broken
        if (shieldHP < maxShield && !shieldBroken)
        {
            shieldTimer += Time.deltaTime; // Tracks elapsed time since last shield hit

            if (shieldTimer >= shieldDelay)
            {
                shieldHP += Mathf.CeilToInt(shieldRegenRate * Time.deltaTime); // Gradually restores shield
                shieldHP = Mathf.Clamp(shieldHP, 0, maxShield); // Ensures value stays within limits
            }
        }
        // Handles shield recovery after being completely broken
        else if (shieldBroken && shieldHP <= 0)
        {
            shieldTimer += Time.deltaTime; // Starts delay timer after shield breaks
            if (shieldTimer >= shieldDelay * 2)
            {
                shieldBroken = false; // Marks shield as active again
                shieldHP = 1; // Restores small portion of shield to reactivate it
            }
        }
    }
    IEnumerator Flamethrower()
    {
        flamethrowerTimer = 0; // Resets flamethrower CD timer
        Instantiate(flamethrowerPrefab, flamethrowerPOS.position, transform.rotation); // Spawns flamethrower prefab
        yield return new WaitForSeconds(flamethrowerDuration); // Waits for flamethrower duration
    }
    IEnumerator Shock()
    {
        shockTimer = 0; // Resets shock CD timer
        Instantiate(shockPrefab, shockPOS.position, transform.rotation); // Spawns shock prefab
        yield return null;
    }
    IEnumerator Leap()
    {
        leapTimer = 0; // Resets leap CD timer
        Instantiate(leapPrefab, leapPOS.position, transform.rotation); // Spawns leap prefab
        yield return new WaitForSeconds(0.1f); // Small delay before leap

        agent.enabled = false; // Disables NavMeshAgent so physics can move the enemy
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // Allows physics control
        rb.AddForce((gamemanager.instance.player.transform.position - transform.position).normalized * leapForce, ForceMode.VelocityChange); // Launches enemy toward player
        yield return new WaitForSeconds(1f); // Waits for jump to complete
        rb.isKinematic = true; // Restores kinematic state
        agent.enabled = true; // Re-enables NavMeshAgent for normal movement

        // Applies stomp damage to any player caught within landing radius
        Collider[] hits = Physics.OverlapSphere(transform.position, stompRadius);
        foreach (Collider hit in hits)
        {
            IDamage dmg = hit.GetComponent<IDamage>();
            if (dmg != null && hit.CompareTag("Player"))
            {
                dmg.takeDamage(leapDamage); // Applies damage from landing stomp
            }
        }
    }
    IEnumerator Dash()
    {
        dashTimer = 0; // Resets dash CD timer
        Instantiate(dashPrefab, dashPOS.position, transform.rotation); // Spawns dash prefab

        Vector3 dir = (gamemanager.instance.player.transform.position - transform.position).normalized; // Calculates dash direction toward player
        Vector3 startPos = transform.position; // Records starting position
        Vector3 endPos = startPos + dir * 8f; // Determines where dash will end based on range

        float elapsed = 0f;
        float dashDuration = 0.3f; // Time taken to complete dash
                                   // Moves smoothly from start to end using Lerp
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / dashDuration);
            yield return null;
        }

        // Detects players hit by dash impact
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
        foreach (Collider hit in hits)
        {
            IDamage dmg = hit.GetComponent<IDamage>();
            if (dmg != null && hit.CompareTag("Player"))
            {
                dmg.takeDamage(dashDamage); // Applies impact damage
                Rigidbody playerRB = hit.GetComponent<Rigidbody>(); // Gets player’s Rigidbody for knockback
                if (playerRB != null)
                {
                    Vector3 knockDir = (hit.transform.position - transform.position).normalized; // Finds direction away from enemy
                    playerRB.AddForce(knockDir * dashKnockbackForce, ForceMode.Impulse); // Pushes player backward
                }
            }
        }
    }
}