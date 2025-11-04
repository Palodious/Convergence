using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP;
    [SerializeField] float speed;
    [SerializeField] int sprintMod;
    [SerializeField] float JumpSpeed;
    [SerializeField] int maxJumps;
    [SerializeField] float gravity;

    [SerializeField] int shootDamage;
    [SerializeField] float shootDist;
    [SerializeField] float shootRate;

    [SerializeField] int maxAmmo;
    [SerializeField] int ammo;

    // --- Added for Abilities ---
    [HideInInspector] public float damageBoost = 1f; // Used by playerAbilities for temporary damage increase

    Vector3 moveDir;
    Vector3 playerVel;

    int jumpCount;

    float shootTimer;

    // --- Added Properties for Cross-Script Access ---
    public CharacterController Controller => controller; // Allows external access to CharacterController
    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    public int HPValue
    {
        get => HP;
        set => HP = value;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        shootTimer += Time.deltaTime;

        movement();
        sprint();
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            playerVel = Vector3.zero;
            jumpCount = 0;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            playerVel.y = JumpSpeed;
            jumpCount++;
        }
    }

    void shoot()
    {
        shootTimer = 0;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage((int)(shootDamage * damageBoost)); // Apply boosted damage during surge
            }
        }
    }

    public void addAmmo(int value)
    {
        ammo += value;
        if (ammo > maxAmmo)
            ammo = maxAmmo;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
            // You Lose!!!
            gamemanager.instance.youLose();
        }
    }
}
