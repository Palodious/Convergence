using UnityEngine;
using System.Collections;

public class playerAbilities : MonoBehaviour
{
    [SerializeField] playerController controller;
    [SerializeField] LayerMask enemyMask;

    // Rift Pulse (Q)
    [SerializeField] int pulseDamage;
    [SerializeField] float pulseRange;
    [SerializeField] float pulseCooldown;

    // Rift Surge (E)
    [SerializeField] float surgeDuration;
    [SerializeField] float surgeSpeedBoost;
    [SerializeField] float surgeDamageBoost;
    [SerializeField] float surgeCooldown;

    // Rift Collapse (R)
    [SerializeField] int collapseDamage;
    [SerializeField] float collapseRadius;
    [SerializeField] float collapseCooldown;
    [SerializeField] float collapseSlowTime;
    [SerializeField] float collapseSlowScale;

    // Rift Jump (F)
    [SerializeField] float jumpDistance;
    [SerializeField] float jumpCooldown;

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

    // Start is called before the first frame update
    void Start()
    {
        if (controller == null)
            controller = GetComponent<playerController>();

        // Corrected: access through property instead of private variable
        originalSpeed = controller.Speed;
    }

    // Update is called once per frame
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

    // Q – short-range burst
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

    // E – temporary boost
    IEnumerator RiftSurge()
    {
        surgeTimer = 0;
        isSurging = true;
        surgeEndTime = Time.time + surgeDuration;

        // Corrected: Use Speed property and added damageBoost field
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

    // R – heavy AoE + slow-mo
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

    // F – teleport forward
    void RiftJump()
    {
        jumpTimer = 0;
        RaycastHit hit;
        Vector3 targetPos = transform.position + transform.forward * jumpDistance;

        if (Physics.Raycast(transform.position, transform.forward, out hit, jumpDistance))
            targetPos = hit.point - transform.forward * 1f;

        // Corrected: use Controller property instead of private field
        controller.Controller.enabled = false;
        transform.position = targetPos;
        controller.Controller.enabled = true;
    }
}
