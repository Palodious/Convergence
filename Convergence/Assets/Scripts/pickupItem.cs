using UnityEngine;

public class pickupItem : MonoBehaviour
{
    public enum PickupType { Health, Ammo }
    [SerializeField] PickupType type;
    [SerializeField] int amount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController player = other.GetComponent<playerController>();

            if (player != null)
            {
                if (type == PickupType.Health)
                {
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
