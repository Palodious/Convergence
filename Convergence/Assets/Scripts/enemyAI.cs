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

    [SerializeField] bool canMelee; // Enables or disables melee attacks from Inspector
    [SerializeField] GameObject melee; // Prefab for melee attack
    [SerializeField] float meleeRate; // Rate between melee attacks
    [SerializeField] float meleeRange; // Distance required to use melee
    [SerializeField] Transform meleePOS; // Spawn point for melee attacks

    Color colorOrig;

    bool playerInTrigger;

    float shootTimer;
    float meleeTimer;
    float angleToPlayer;
    float stoppingDistOrig;
    Vector3 playerDir;

    [SerializeField] int maxShield; // Maximum shield capacity
    [SerializeField] int shieldHP; // Current active shield value, editable in Inspector
    [SerializeField] float shieldRegenRate; // How fast the shield regenerates
    [SerializeField] float shieldDelay; // Delay before shield starts to recharge
    bool shieldBroken; // Checks if shield has been broken
    float shieldTimer; // Timer for shield regen delay

    [SerializeField] bool canPatrol = true;
    [SerializeField] Transform[] patrolPoints; // Array of patrol points
    int patrolIndex; // Keeps track of which point it’s moving toward
    bool isPatrolling; // Enables or disables patrol mode

    void Start()
    {
        colorOrig = model.material.color;
        stoppingDistOrig = agent.stoppingDistance;

        shieldHP = maxShield; // Sets shield to max value on start
        isPatrolling = canPatrol && patrolPoints != null && patrolPoints.Length > 0; // Enables patrol if points exist and toggle is on
    }
    void Update()
    {
        shootTimer += Time.deltaTime;
        meleeTimer += Time.deltaTime;

        bool seesPlayer = playerInTrigger && canSeePlayer();

        if (seesPlayer)
        {
            agent.stoppingDistance = stoppingDistOrig;

            float distToPlayer = Vector3.Distance(transform.position, gamemanager.instance.player.transform.position);

            // Melee attack when in range
            if (canMelee && meleeTimer >= meleeRate && distToPlayer <= meleeRange)
            {
                Melee();
            }
            // Shoot when farther than melee range
            if (canShoot && shootTimer >= shootRate && distToPlayer > meleeRange)
            {
                Shoot();
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

        Debug.DrawRay(headPos.position, playerDir); // Debug ray identical to smaller script

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            Debug.Log(hit.collider.name);

            if (angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
            {
                agent.SetDestination(gamemanager.instance.player.transform.position);

                if (shootTimer >= shootRate)
                {
                    Shoot();
                }

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
        if (shieldHP > 0)
        {
            shieldHP -= amount; // Damage absorbed by shield first

            if (shieldHP <= 0)
            {
                shieldHP = 0;
                shieldBroken = true; // Marks shield as broken
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
    void Shoot()
    {
        shootTimer = 0; // Resets shoot cooldown timer
        Instantiate(projectile, shootPOS.position, transform.rotation); // Spawns projectile prefab from shoot position
    }
    void Melee()
    {
        meleeTimer = 0; // Resets melee cooldown timer
        Instantiate(melee, meleePOS.position, meleePOS.rotation); // Spawns melee hitbox prefab
    }
    void Patrol()
    {
        // Prevents errors if patrol points are missing or empty
        if (patrolPoints == null || patrolPoints.Length == 0) return;
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
}