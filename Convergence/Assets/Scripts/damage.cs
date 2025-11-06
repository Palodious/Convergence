using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{
    public enum damageType { moving, melee, DOT, homing } // Defines the different types of damage this object can apply

    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount; // Amount of damage dealt each hit or tick
    [SerializeField] float damageRate; // Rate at which DOT damage applies
    [SerializeField] int speed; // Movement speed of projectiles
    [SerializeField] int destroyTime;

    bool isDamaging; // Used to prevent multiple DOT ticks from overlapping

    // Start is called before the first frame update
    void Start()
    {
        if (type == damageType.moving || type == damageType.homing)
        {
            Destroy(gameObject, destroyTime);

            if (type == damageType.moving)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
        else if (type == damageType.melee)
        {
            Destroy(gameObject, 0.2f); // Melee hitboxes are short
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (type == damageType.homing)
        {
            rb.linearVelocity = (gamemanager.instance.player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        
        if (dmg != null && type != damageType.DOT)
        {
            dmg.takeDamage(damageAmount); 
        }

        // Destroys projectile after contact for most damage types
        if (type == damageType.moving || type == damageType.homing || type == damageType.melee)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        // Handles DOT — applies damage periodically
        if (dmg != null && type == damageType.DOT && !isDamaging)
        {
            StartCoroutine(damageOther(dmg)); // Begins damage-over-time coroutine
        }
    }

    IEnumerator damageOther(IDamage d)
    {
        isDamaging = true; // Prevents overlapping DOT ticks
        d.takeDamage(damageAmount); // Applies one tick of DOT damage
        yield return new WaitForSeconds(damageRate); // Waits before next tick
        isDamaging = false; // Allows next DOT cycle to occur
    }
}
