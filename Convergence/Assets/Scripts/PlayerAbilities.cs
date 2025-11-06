using UnityEngine;
using System.Collections;

public class playerAbilities : MonoBehaviour
{
    [SerializeField] playerController controller;
    [SerializeField] LayerMask enemyMask;

    // Rift Pulse
    [SerializeField] int pulseDamage;
    [SerializeField] float pulseRange;
    [SerializeField] float pulseCooldown;
    [SerializeField] int pulseEnergyCost;

    // Rift Surge
    [SerializeField] float surgeDuration;
    [SerializeField] float surgeSpeedBoost;
    [SerializeField] float surgeDamageBoost;
    [SerializeField] float surgeCooldown;
    [SerializeField] int surgeEnergyCost;

    // Rift Collapse
    [SerializeField] int collapseDamage;
    [SerializeField] float collapseRadius;
    [SerializeField] float collapseCooldown;
    [SerializeField] int collapseEnergyCost;
    [SerializeField] float collapseSlowTime;
    [SerializeField] float collapseSlowScale;

    // Rift Jump
    [SerializeField] float jumpDistance;
    [SerializeField] float jumpCooldown;
    [SerializeField] int jumpEnergyCost;

    // Timers
    float pulseTimer;
    float surgeTimer;
    float collapseTimer;
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
        pulseTimer += Time.deltaTime;
        surgeTimer += Time.deltaTime;
        collapseTimer += Time.deltaTime;
        jumpTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && pulseTimer >= pulseCooldown)
            StartCoroutine(RiftPulse());

        if (Input.GetKeyDown(KeyCode.E) && surgeTimer >= surgeCooldown)
            StartCoroutine(RiftSurge());

        if (Input.GetKeyDown(KeyCode.R) && collapseTimer >= collapseCooldown)
            StartCoroutine(RiftCollapse());

        if (Input.GetKeyDown(KeyCode.F) && jumpTimer >= jumpCooldown)
            RiftJump();

        if (isSurging && Time.time >= surgeEndTime)
            EndSurge();

    }

    IEnumerator RiftPulse()
    {
        pulseTimer = 0;
        Collider[] hits = Physics.OverlapSphere(transform.position, pulseRange, enemyMask);
        foreach (Collider hit in hits)
        {
            IDamage dmg = hit.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(pulseDamage);
        }
        yield return null;
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
    }

    void EndSurge()
    {
        isSurging = false;
        controller.Speed = originalSpeed;
        controller.damageBoost = originalDamageBoost;
    }

    IEnumerator RiftCollapse()
    {
        collapseTimer = 0;
        Collider[] hits = Physics.OverlapSphere(transform.position, collapseRadius, enemyMask);
        foreach (Collider hit in hits)
        {
            IDamage dmg = hit.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(collapseDamage);
        }

        Time.timeScale = collapseSlowScale;
        yield return new WaitForSecondsRealtime(collapseSlowTime);
        Time.timeScale = 1f;
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
}

