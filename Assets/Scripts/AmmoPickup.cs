using UnityEngine;

public class AmmoPickup : Pickup
{
    protected override bool OnPickedUp(Collider player)
    {
        WeaponSwitcher switcher = player.GetComponentInChildren<WeaponSwitcher>();
        if (switcher == null) return false;
        
        Weapon[] allWeapons = switcher.GetAllWeapons();
        bool anyRefilled = false;
        
        foreach (Weapon weapon in allWeapons)
        {
            if (weapon != null && weapon.CurrentAmmo < weapon.GetMaxAmmo())
            {
                weapon.RefillAmmo();
                anyRefilled = true;
            }
        }
        
        return anyRefilled;
    }
}
