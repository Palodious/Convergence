using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;

    // HP and Shield
    [SerializeField] int HP;
    [SerializeField] int shieldHP;  // shield points
    [SerializeField] float shieldRegenRate; // how fast the shield regens
    bool shieldBroken;  // used to stop regen when gone
    bool shieldActive; // stops flashing multiple times

    // Attacks
    [SerializeField] GameObject projectile; // projectile replaces bullet
    [SerializeField] Transform shootPOS;
    [SerializeField] Transform meleePOS; // melee spawn position
    [SerializeField] float shootRate;
    [SerializeField] float shootRange;  // how far enemy can shoot
    [SerializeField] float meleeRate; // time between melee swings
    [SerializeField] float meleeRange; // distance to hit with melee
    float shootTimer;
    float meleeTimer; // used for melee cooldown

    // Trigger + Detection
    bool playerInTrigger;
    bool playerExitTrigger; // used for patrol resume

    // Patrol
    [SerializeField] bool enablePatrol; // toggle patrol on or off
    [SerializeField] Transform[] patrolPoints; // patrol points
    int patrolIndex; // keeps track of where enemy goes next

    // Difficulty Select
    public enum EnemyDifficulty { Normal, Hard, Elite, Boss }  // lets me pick in inspector
    public EnemyDifficulty difficulty;

    Color colorOrig;

    void Awake()
    {
        colorOrig = model.material.color;

        if (enablePatrol && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position); // start patrol
    }

    void Update()
    {
        shootTimer += Time.deltaTime;
        meleeTimer += Time.deltaTime;
        RegenerateShield(); // shield regen if not broken

        // patrol logic if no player in range
        if (enablePatrol && !playerInTrigger)
        {
            if (agent.remainingDistance < 0.5f)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
            return; // stop here so patrol doesn’t mix with chase
        }

        // chase + attack
        if (playerInTrigger)
        {
            Transform player = gamemanager.instance.player.transform;
            agent.SetDestination(player.position);

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= meleeRange && meleeTimer >= meleeRate)
            {
                meleeTimer = 0;
                melee(); // melee attack
            }
            else if (distance <= shootRange && shootTimer >= shootRate)
            {
                shootTimer = 0;
                shoot(); // shoot attack
            }
        }

        if (playerExitTrigger && enablePatrol)
        {
            playerExitTrigger = false;
            agent.SetDestination(patrolPoints[patrolIndex].position); // resume patrol
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            playerExitTrigger = false; // start chasing again
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            playerExitTrigger = true; // resume patrol when player leaves
        }
    }

    public void takeDamage(int amount)
    {
        if (shieldHP > 0)
        {
            shieldHP -= amount;
            if (!shieldActive) StartCoroutine(FlashShield()); // flash cyan

            if (shieldHP <= 0)
            {
                shieldBroken = true;
                StartCoroutine(FlashShieldBreak()); // flash white when shield breaks
            }
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

    void RegenerateShield()
    {
        if (shieldBroken && shieldHP <= 0)
            return;

        shieldHP += Mathf.RoundToInt(shieldRegenRate * Time.deltaTime);
    }

    IEnumerator FlashShield()
    {
        shieldActive = true;
        model.material.color = Color.cyan;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
        shieldActive = false;
    }

    IEnumerator FlashShieldBreak()
    {
        model.material.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        model.material.color = colorOrig;
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    void shoot()
    {
        Instantiate(projectile, shootPOS.position, transform.rotation); // spawn projectile
    }

    void melee()
    {
        Instantiate(projectile, meleePOS.position, transform.rotation); // spawn melee hit
    }
}
