using UnityEngine;
using System.Collections;

<<<<<<< HEAD
public class PlayerAbilities : MonoBehaviour
{
    [SerializeField] playerController controller;
    [SerializeField] int pulseDamage;
    [SerializeField] float pulseRadius;
    [SerializeField] float pulseForce;
    [SerializeField] float pulseCooldown;
    [SerializeField] int pulseFlowCost;
    [SerializeField] GameObject pulseEffect;

    [SerializeField] int surgeFlowCost;
    [SerializeField] float surgeCooldown;
    [SerializeField] float surgeDuration;
    [SerializeField] float surgeSpeedBoost;
    [SerializeField] float surgeDamageBoost;
    [SerializeField] GameObject surgeEffect;

    [SerializeField] int collapseDamage;
    [SerializeField] float collapseRadius;
    [SerializeField] float collapseCooldown;
    [SerializeField] int collapseFlowCost;
    [SerializeField] float collapseSlowMoScale;
    [SerializeField] float collapseSlowMoTime;
    [SerializeField] GameObject collapseEffect;

    [SerializeField] float teleportDistance;
    [SerializeField] float teleportCooldown;
    [SerializeField] float teleportFlowCost;
    [SerializeField] float teleportDuration;
    [SerializeField] ParticleSystem teleportEffect;
    [SerializeField] AudioClip teleportSound;

    bool canPulse = true;
    bool canSurge = true;
    bool canCollapse = true;
    bool isSurging = false;

    float surgeTimer;
    float surgeEndTime;
    float originalSpeed;
    int originalDamage;

    private float cooldownTimer;
    private bool isOnCooldown = false;
    private AudioSource audioSource;
    private CharacterController charController;
    private Rigidbody rb;
    private bool useCharacterController = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<playerController>() ?? gameObject.AddComponent<playerController>();
        charController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        useCharacterController = charController != null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (controller == null)
        {
         
        }

        originalSpeed = controller != null ? controller.speed : 0;
        originalDamage = controller != null ? controller.shootDamage : 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Ability1") && canPulse)
            StartCoroutine(RiftPulse());

        if (Input.GetButtonDown("Ability2") && canSurge)
            StartCoroutine(RiftSurge());

        if (Input.GetButtonDown("Ability3") && canCollapse)
            StartCoroutine(RiftCollapse());

        if (isSurging)
        {
            surgeTimer += Time.deltaTime;
            if (surgeTimer >= surgeEndTime)
                EndSurge();
        }

        if (Input.GetButtonDown("Teleport") // Or configurable key
        {
            TryActivateTeleport();
        }


        HandleCooldown();
        HandleInput();
    }

    private void HandleCooldown()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isOnCooldown = false;
                cooldownTimer = 0f;
                // could trigger an event or UI update
            }
        }
    }
    private bool IsReady()
    {
        return !isOnCooldown && HasEnoughFlow();
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = teleportCooldown;
    }



    IEnumerator RiftPulse()
    {
        canPulse = false;

        if (pulseEffect != null)
            Instantiate(pulseEffect, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, pulseRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                IDamage dmg = hit.GetComponent<IDamage>();
                if (dmg != null)
                    dmg.takeDamage(pulseDamage);

                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddExplosionForce(pulseForce, transform.position, pulseRadius);
            }
        }

        yield return new WaitForSeconds(pulseCooldown);
        canPulse = true;
    }

    IEnumerator RiftSurge()
    {
        canSurge = false;
        isSurging = true;
        surgeTimer = 0;
        surgeEndTime = surgeDuration;

        if (surgeEffect != null)
            Instantiate(surgeEffect, transform.position, Quaternion.identity);

        if (controller != null)
        {
            controller.speed = Mathf.RoundToInt(controller.speed * surgeSpeedBoost);
            controller.shootDamage = Mathf.RoundToInt(controller.shootDamage * surgeDamageBoost);
        }

        yield return new WaitForSeconds(surgeCooldown);
        canSurge = true;
=======
public class playerAbilities : MonoBehaviour
{
    [SerializeField] playerController controller;
    [SerializeField] LayerMask enemyMask;

    // Rift Surge
    [SerializeField] float surgeDuration;
    [SerializeField] float surgeSpeedBoost;
    [SerializeField] float surgeDamageBoost;
    [SerializeField] float surgeCooldown;
    [SerializeField] int surgeEnergyCost;

    // Rift Jump
    [SerializeField] float jumpDistance;
    [SerializeField] float jumpCooldown;
    [SerializeField] int jumpEnergyCost;

    // Timers
    float surgeTimer;
    float jumpTimer;

    // Surge state
    bool isSurging;
    float surgeEndTime;
    float originalSpeed;
    float originalDamageBoost = 1f;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<playerController>();

        originalSpeed = controller.GetComponent<playerController>().Speed;
    }
    void Update()
    {
        surgeTimer += Time.deltaTime;
        jumpTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.E) && surgeTimer >= surgeCooldown)
            StartCoroutine(RiftSurge());

        if (Input.GetKeyDown(KeyCode.F) && jumpTimer >= jumpCooldown)
            RiftJump();

        if (isSurging && Time.time >= surgeEndTime)
            EndSurge();
    }
    IEnumerator RiftSurge()
    {
        surgeTimer = 0;
        isSurging = true;
        surgeEndTime = Time.time + surgeDuration;

        controller.Speed = originalSpeed * surgeSpeedBoost;
        controller.damageBoost = surgeDamageBoost;

        yield return new WaitForSeconds(surgeDuration);
        EndSurge();
>>>>>>> parent of fce34b8 (Merge branch 'main' into Dev)
    }
    void EndSurge()
    {
        isSurging = false;
<<<<<<< HEAD

        if (controller != null)
        {
            controller.speed = originalSpeed;
            controller.shootDamage = originalDamage;
        }
    }

    IEnumerator RiftCollapse()
    {
        canCollapse = false;

        if (collapseEffect != null)
            Instantiate(collapseEffect, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, collapseRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                IDamage dmg = hit.GetComponent<IDamage>();
                if (dmg != null)
                    dmg.takeDamage(collapseDamage);
            }
        }

        Time.timeScale = collapseSlowMoScale;
        yield return new WaitForSecondsRealtime(collapseSlowMoTime);
        Time.timeScale = 1f;

        yield return new WaitForSeconds(collapseCooldown);
        canCollapse = true;
    }


=======
        controller.Speed = originalSpeed;
        controller.damageBoost = originalDamageBoost;
    }
    void RiftJump()
    {
        // reset cooldown timer and spend energy
        jumpTimer = 0f;
        controller.UseEnergy(jumpEnergyCost);
        // spawn start VFX + start sound at player position
        Vector3 startPos = controller.transform.position;
        Quaternion startRot = controller.transform.rotation;
        if (EffectPool.Instance != null)
            EffectPool.Instance.Spawn(startPos, startRot, null);
        // compute target position (forward dash)
        Vector3 targetPos = transform.position + transform.forward * jumpDistance;
        // move player safely when using CharacterController
        CharacterController cc = controller.Controller;
        if (cc != null)
        {
            cc.enabled = false;               // disable to avoid CharacterController collision issues
            transform.position = targetPos;   // teleport player
            cc.enabled = true;                // re-enable controller
        }
        else
        {
            transform.position = targetPos;
        }
        // spawn end VFX at landing position (no sound required if you keep only start sound)
        if (EffectPool.Instance != null)
            EffectPool.Instance.Spawn(targetPos, startRot, null);
        // set jumpTimer so cooldown logic in Update works
        jumpTimer = 0f;
    }
>>>>>>> parent of fce34b8 (Merge branch 'main' into Dev)
}
