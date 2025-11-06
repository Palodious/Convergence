using UnityEngine;
using System.Collections;

public class PlayerAbilities : MonoBehaviour
{
    [SerializeField] playerController controller;
    [SerializeField] GameObject surgeEffect;
    [SerializeField] ParticleSystem jumpEffect;
    [SerializeField] AudioClip jumpSound;

    AudioSource audioSource;
    CharacterController charController;

    [SerializeField] float surgeDuration = 5f;
    [SerializeField] float surgeSpeedBoost = 1.5f;
    [SerializeField] float surgeDamageBoost = 2f;
    [SerializeField] float surgeCooldown = 8f;
    [SerializeField] float surgeEnergyCost = 25f;

    [SerializeField] float jumpDistance = 8f;
    [SerializeField] float jumpCooldown = 6f;
    [SerializeField] float jumpEnergyCost = 20f;

    bool canSurge = true;
    bool canJump = true;
    bool isSurging = false;

    float surgeEndTime;
    float originalSpeed;
    int originalDamage;

    void Awake()
    {
        controller = GetComponent<playerController>();
        audioSource = GetComponent<AudioSource>();
        charController = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (controller != null)
        {
            originalSpeed = controller.Speed;
            originalDamage = (int)typeof(playerController)
                .GetField("shootDamage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(controller);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canSurge)
            TryActivateSurge();

        if (Input.GetKeyDown(KeyCode.F) && canJump)
            TryActivateJump();

        if (isSurging && Time.time >= surgeEndTime)
            EndSurge();
    }

    void TryActivateSurge()
    {
        if (!canSurge) return;
        if (!controller.CanUseEnergy(surgeEnergyCost)) return;

        controller.UseEnergy(surgeEnergyCost);
        StartCoroutine(RiftSurge());
    }

    IEnumerator RiftSurge()
    {
        canSurge = false;
        isSurging = true;
        surgeEndTime = Time.time + surgeDuration;

        if (surgeEffect != null)
            Instantiate(surgeEffect, transform.position, Quaternion.identity);

        controller.Speed *= surgeSpeedBoost;
        controller.damageBoost = surgeDamageBoost;

        yield return new WaitForSeconds(surgeDuration);
        EndSurge();

        yield return new WaitForSeconds(surgeCooldown);
        canSurge = true;
    }

    void EndSurge()
    {
        isSurging = false;
        if (controller != null)
        {
            controller.Speed = originalSpeed;
            controller.damageBoost = 1f;
        }
    }

    void TryActivateJump()
    {
        if (!canJump) return;
        if (!controller.CanUseEnergy(jumpEnergyCost)) return;

        controller.UseEnergy(jumpEnergyCost);
        StartCoroutine(RiftJump());
    }

    IEnumerator RiftJump()
    {
        canJump = false;

        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + transform.forward * jumpDistance;

        if (jumpEffect != null)
            Instantiate(jumpEffect, startPos, Quaternion.identity);

        if (audioSource != null && jumpSound != null)
            audioSource.PlayOneShot(jumpSound);

        if (charController != null)
        {
            charController.enabled = false;
            transform.position = targetPos;
            charController.enabled = true;
        }
        else
        {
            transform.position = targetPos;
        }

        if (jumpEffect != null)
            Instantiate(jumpEffect, targetPos, Quaternion.identity);

        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }
}
