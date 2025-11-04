using UnityEngine;

public class pickupItem : MonoBehaviour
{
    public enum PickupType { Health, Ammo }
    [SerializeField] PickupType type;
<<<<<<< Updated upstream
    [SerializeField] int amount;
=======
    [SerializeField] int amount = 0; 
>>>>>>> Stashed changes

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
<<<<<<< Updated upstream

=======
        
>>>>>>> Stashed changes
    }

    // Update is called once per frame
    void Update()
    {
<<<<<<< Updated upstream

    }

    // Detects when the player collides with the pickup
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController player = other.GetComponent<playerController>();

            if (player != null)
            {
                if (type == PickupType.Health)
                {
                    // Corrected: HP is private, use property instead
                    player.HPValue += amount;
                }
                else if (type == PickupType.Ammo)
                {
                    player.addAmmo(amount);
                }
            }

            Destroy(gameObject);
        }
    }
=======
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerController player = other.GetComponent<playerController>(); 

            if(player != null)
            {
                if(type == PickupType.Health)
                {
                    player.HP += amount; 
                }
                else if (type == PickupType.Ammo)
                {
                    player.addAmmo(amount); 
                }
            }
        }
    }

>>>>>>> Stashed changes
}
