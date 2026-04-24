using UnityEngine;

public class WeaponPickup : Pickup
{
    [SerializeField] WeaponData weaponData;

    protected override bool OnPickedUp(Collider player)
    {
        WeaponSwitcher switcher = player.GetComponentInChildren<WeaponSwitcher>();
        if (switcher == null) return false;

        Weapon[] weapons = player.GetComponentsInChildren<Weapon>(true);

        foreach (Weapon weapon in weapons) {
            if (weapon.Data == weaponData) {

                //Unlock weapon
                switcher.UnlockWeapon(weapon);
                return true;
            
            }
        }
        return false;

    }

}
