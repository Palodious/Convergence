using UnityEngine;

public class pickupItem : MonoBehaviour
{
    public enum PickupType { Health, Ammo }
    [SerializeField] PickupType type;
    [SerializeField] int amount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
}
