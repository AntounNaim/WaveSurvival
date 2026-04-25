using UnityEngine;

public class HealthPickup : Pickup
{
    [SerializeField] private int healAmount = 25;
    [SerializeField] private GameObject pickupVFX;
    
    protected override bool OnPickedUp(Collider player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        
        if (playerHealth == null) return false;
        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth) return false;
        
        playerHealth.Heal(healAmount);
        
        // Play pickup effects
        if (pickupVFX != null)
            Instantiate(pickupVFX, transform.position, Quaternion.identity);
        
        return true;
    }
}