using NUnit.Framework;
using StarterAssets;
using UnityEngine;
using System.Collections.Generic;

public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField] ActiveWeapon activeWeapon;

    StarterAssetsInputs inputs;
    Weapon[] allWeapons;
    List<Weapon> unlockedWeapons= new List<Weapon>();
    int currentIndex = -1;

    private void Awake()
    {
        inputs = GetComponentInParent<StarterAssetsInputs>();
        allWeapons = GetComponentsInChildren<Weapon>();
    }

    private void Start()
    {
        foreach (Weapon weapon in allWeapons) {
            weapon.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!inputs.switchWeapon || unlockedWeapons.Count < 2) return;

        inputs.SwitchWeaponInput(false);
        int nextIndex = (currentIndex + 1) % unlockedWeapons.Count;
        EquipWeapon(nextIndex);

    }

    public void UnlockWeapon(Weapon weapon) {
        if (unlockedWeapons.Contains(weapon)) return;

        unlockedWeapons.Add(weapon);
        EquipWeapon(unlockedWeapons.Count - 1);
    }

    void EquipWeapon(int index) {

        foreach (Weapon weapon in unlockedWeapons) { 
            weapon.gameObject.SetActive(false);

            currentIndex = index;
            unlockedWeapons[index].gameObject.SetActive(true);
            activeWeapon.SwitchWeapon(unlockedWeapons[index]);
        }
    }
}
