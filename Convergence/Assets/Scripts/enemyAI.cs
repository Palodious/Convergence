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

    [SerializeField] GameObject bullet;
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

    [SerializeField] ParticleSystem flamethrowerFX; // Visual effect for flamethrower
    [SerializeField] int flamethrowerDamage; // Damage per tick of flamethrower
    [SerializeField] float flamethrowerDuration; // How long the flamethrower lasts
    [SerializeField] float flamethrowerDOTDuration; // How long fire damage lasts after hit
    [SerializeField] float flamethrowerDOTRate; // How often DOT ticks
    [SerializeField] float flamethrowerRange; // Range of flamethrower
    float flamethrowerTimer; // Timer for flamethrower cooldown

    [SerializeField] int shockDamage; // Damage of electrical shock
    [SerializeField] float shockRange; // Radius of shockwave
    [SerializeField] float shockCooldown; // Cooldown time before shock can be used again
    float shockTimer; // Timer for shock ability

    [SerializeField] float leapForce; // Force of jump movement
    [SerializeField] float stompRadius; // Area that gets hit when landing
    [SerializeField] int leapDamage; // Damage caused when stomping down
    [SerializeField] float leapCooldown; // Time before another leap is possible
    float leapTimer; // Timer for leap cooldown

    [SerializeField] float dashSpeed; // Speed of dash attack
    [SerializeField] float dashCooldown; // Time before dash can be used again
    [SerializeField] int dashDamage; // Damage caused by dash impact
    [SerializeField] float dashKnockbackForce; // Force that pushes player backwards
    float dashTimer; // Timer for dash cooldown

    [SerializeField] int maxShield; // Maximum shield capacity
    [SerializeField] float shieldRegenRate; // How fast the shield regenerates
    [SerializeField] float shieldDelay; // Delay before shield starts to recharge
    int currentShield; // Current active shield value
    bool shieldBroken; // Checks if shield has been broken
    float shieldTimer; // Timer for shield regen delay

    [SerializeField] Transform[] patrolPoints; // Array of patrol points
    int patrolIndex; // Keeps track of which point it’s moving toward
    bool isPatrolling; // Enables or disables patrol mode

    [SerializeField] GameObject telegraphPrefab; // Prefab used for showing warnings
    [SerializeField] Color telegraphColor; // Color of warning indicator
    [SerializeField] float telegraphTime; // How long warning stays before attack

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        stoppingDistOrig = agent.stoppingDistance;

        currentShield = maxShield; // Sets shield to max value on start
        isPatrolling = patrolPoints != null && patrolPoints.Length > 0; // Enables patrol if points exist
    }

    // Update is called once per frame
    void Update()
    {
        shootTimer += Time.deltaTime;
        flamethrowerTimer += Time.deltaTime;
        shockTimer += Time.deltaTime;
        leapTimer += Time.deltaTime;
        dashTimer += Time.deltaTime;

        if (playerInTrigger && canSeePlayer())
        {
            if (shootTimer >= shootRate)
            {
                shoot();
            }

            if (hasFlamethrower && flamethrowerTimer >= flamethrowerDuration)
            {
                StartCoroutine(UseFlamethrower()); // Starts flamethrower ability
            }

            if (hasShock && shockTimer >= shockCooldown)
            {
                StartCoroutine(UseShock()); // Triggers electrical shock
            }

            if (hasLeap && leapTimer >= leapCooldown)
            {
                StartCoroutine(UseLeap()); // Performs leap and stomp attack
            }

            if (hasDash && dashTimer >= dashCooldown)
            {
                StartCoroutine(UseDash()); // Performs charging dash
            }
        }
        else if (isPatrolling)
        {
            Patrol(); // Moves between patrol points when player not detected
        }

        HandleShieldRegen(); // Handles shield regeneration when possible
    }

    bool canSeePlayer()
    {
        playerDir = gamemanager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(headPos.position, playerDir);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
            {
                agent.SetDestination(gamemanager.instance.player.transform.position);

                if (agent.remainingDistance <= stoppingDistOrig)
                    faceTarget();

                return true;
            }
        }
        return false;
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
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
        if (currentShield > 0)
        {
            currentShield -= amount; // Damage absorbed by shield first

            if (currentShield <= 0)
            {
                currentShield = 0;
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

    void shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPOS.position, transform.rotation);
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

    void HandleShieldRegen()
    {
        // Regenerates shield over time if not broken
        if (currentShield < maxShield && !shieldBroken)
        {
            shieldTimer += Time.deltaTime; // Tracks elapsed time since last shield hit

            if (shieldTimer >= shieldDelay)
            {
                currentShield += Mathf.CeilToInt(shieldRegenRate * Time.deltaTime); // Gradually restores shield
                currentShield = Mathf.Clamp(currentShield, 0, maxShield); // Ensures value stays within limits
            }
        }
        // Handles shield recovery after being completely broken
        else if (shieldBroken && currentShield <= 0)
        {
            shieldTimer += Time.deltaTime; // Starts delay timer after shield breaks
            if (shieldTimer >= shieldDelay * 2)
            {
                shieldBroken = false; // Marks shield as active again
                currentShield = 1; // Restores small portion of shield to reactivate it
            }
        }
    }

    IEnumerator UseFlamethrower()
    {
        flamethrowerTimer = 0; // Resets flamethrower cooldown timer
        if (flamethrowerFX != null)
            flamethrowerFX.Play(); // Plays flamethrower particle effect

        GameObject telegraph = ShowTelegraph(shootPOS.position, flamethrowerRange, false); // Shows warning before attack
        yield return new WaitForSeconds(telegraphTime); // Waits for warning duration before firing

        float elapsed = 0f;
        while (elapsed < flamethrowerDuration)
        {
            elapsed += Time.deltaTime; // Tracks attack duration
            RaycastHit hit;
            // Shoots a ray forward to detect player within range
            if (Physics.Raycast(shootPOS.position, transform.forward, out hit, flamethrowerRange))
            {
                IDamage dmg = hit.collider.GetComponent<IDamage>(); // Checks if object can take damage
                if (dmg != null && hit.collider.CompareTag("Player"))
                {
                    dmg.takeDamage(flamethrowerDamage); // Applies initial flamethrower hit damage
                    StartCoroutine(ApplyFireDOT(dmg)); // Starts DOT burn effect on player
                }
            }
            yield return null;
        }

        if (flamethrowerFX != null)
            flamethrowerFX.Stop(); // Stops flamethrower particle effect

        Destroy(telegraph); // Removes visual warning
    }

    IEnumerator ApplyFireDOT(IDamage dmg)
    {
        float timer = 0f;
        // Applies small repeated fire damage over a set duration
        while (timer < flamethrowerDOTDuration)
        {
            dmg.takeDamage(1); // Applies low tick damage each interval
            timer += flamethrowerDOTRate; // Tracks DOT time between ticks
            yield return new WaitForSeconds(flamethrowerDOTRate); // Waits between each burn tick
        }
    }

    IEnumerator UseShock()
    {
        shockTimer = 0; // Resets shock cooldown timer
        GameObject telegraph = ShowTelegraph(transform.position, shockRange, true); // Shows circular warning area
        yield return new WaitForSeconds(telegraphTime); // Waits before triggering shock

        Collider[] hits = Physics.OverlapSphere(transform.position, shockRange); // Detects all colliders in shock radius
        foreach (Collider hit in hits)
        {
            IDamage dmg = hit.GetComponent<IDamage>(); // Checks if target takes damage
            if (dmg != null && hit.CompareTag("Player"))
            {
                dmg.takeDamage(shockDamage); // Applies electric damage to player in range
            }
        }

        Destroy(telegraph); // Removes circular warning after effect
    }

    IEnumerator UseLeap()
    {
        leapTimer = 0; // Resets leap cooldown timer
        Vector3 target = gamemanager.instance.player.transform.position; // Gets player’s current position
        GameObject telegraph = ShowTelegraph(target, stompRadius, true); // Shows where enemy will land
        yield return new WaitForSeconds(telegraphTime); // Waits before jumping to give player time to dodge

        agent.enabled = false; // Disables NavMeshAgent so physics can move the enemy
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // Allows physics control
        rb.AddForce((target - transform.position).normalized * leapForce, ForceMode.VelocityChange); // Launches enemy toward player
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
                dmg.takeDamage((int)leapDamage); // Applies damage from landing stomp
            }
        }

        Destroy(telegraph); // Removes warning circle after attack completes
    }

    IEnumerator UseDash()
    {
        dashTimer = 0; // Resets dash cooldown timer
        Vector3 dir = (gamemanager.instance.player.transform.position - transform.position).normalized; // Calculates dash direction toward player
        Vector3 startPos = transform.position; // Records starting position
        Vector3 endPos = startPos + dir * 8f; // Determines where dash will end based on range

        GameObject telegraph = ShowTelegraph(endPos, 2f, false); // Displays dash direction warning
        yield return new WaitForSeconds(telegraphTime); // Waits before performing dash

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

        Destroy(telegraph); // Removes dash direction warning
    }

    GameObject ShowTelegraph(Vector3 pos, float size, bool isCircle)
    {
        // Creates temporary visual indicator for attacks
        if (telegraphPrefab == null) return null;
        GameObject telegraph = Instantiate(telegraphPrefab, pos, Quaternion.identity); // Spawns telegraph at position
        telegraph.transform.localScale = Vector3.one * size; // Sets size of indicator
        Renderer r = telegraph.GetComponent<Renderer>();
        if (r != null)
            r.material.color = telegraphColor; // Applies selected color for warning
        Destroy(telegraph, telegraphTime); // Destroys indicator after timer expires
        return telegraph; // Returns reference to telegraph object
    }
}
