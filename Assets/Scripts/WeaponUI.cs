using TMPro;
using UnityEngine;

public class WeaponUI : MonoBehaviour
{

    [SerializeField] ActiveWeapon activeWeapon;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] TextMeshProUGUI weaponNameText;

    void Update()
    {
        Weapon weapon = activeWeapon.CurrentWeapon;

        if (weapon == null) {
            ammoText.text = "";
            weaponNameText.text = "";
            return;
        }

        weaponNameText.text = weapon.Data.weaponName;
        ammoText.text = $"{weapon.CurrentAmmo} / {weapon.Data.maxAmmo}";
    }
}
