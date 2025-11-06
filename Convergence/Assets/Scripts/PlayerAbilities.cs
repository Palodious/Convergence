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
    [SerializeField] GameObject pulsePrefab;
    [SerializeField] AudioClip pulseSound;

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
    float pulseTimer;
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
        pulseTimer += Time.deltaTime;
        surgeTimer += Time.deltaTime;
        jumpTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && pulseTimer >= pulseCooldown)
            StartCoroutine(RiftPulse());

        if (Input.GetKeyDown(KeyCode.E) && surgeTimer >= surgeCooldown)
            StartCoroutine(RiftSurge());

        if (Input.GetKeyDown(KeyCode.F) && jumpTimer >= jumpCooldown)
            RiftJump();

        if (isSurging && Time.time >= surgeEndTime)
            EndSurge();

    }

    IEnumerator RiftPulse()
    {
        pulseTimer = 0f;
        controller.UseEnergy(pulseEnergyCost);

        // spawn a dedicated pulse VFX and play a dedicated pulse SFX
        Vector3 vfxPos = controller.transform.position + Vector3.up * 0.5f;
        Quaternion vfxRot = controller.transform.rotation;

        if (EffectPool.Instance != null)
        {
            // prefer playerAbilities' pulsePrefab/pulseSound; fallback to pool's onboard ones
            EffectPool.Instance.SpawnOneShot(pulsePrefab != null ? pulsePrefab : EffectPool.Instance.prefab,
                                             vfxPos, vfxRot,
                                             pulseSound != null ? pulseSound : null,
                                             1.5f, 1f);
        }

        // damage handling
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

