using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{

    [Header("Ammo")]
    public int maxAmmo = 20;

    [Header("Identity")]
    public string weaponName;

    [Header("Combat")]
    public int damage;
    public float range;
    public bool isAutomatic;
    public float fireRate = 100.0f;

    [Header("VFX")]
    public GameObject hitVFXPrefab;

    [Header("Camera shake")]
    public float shakeIntensity = 0.005f;

    [Header("Recoil parameters")]
    public float recoilX = 0.5f;
    public float recoilY = 2f;

}
