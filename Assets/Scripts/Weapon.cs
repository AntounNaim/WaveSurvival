using UnityEngine;
using StarterAssets;
using Cinemachine;

public class Weapon : MonoBehaviour
{
    [SerializeField] WeaponData weaponData;
    [SerializeField] ParticleSystem muzzleFlash;
    private CinemachineImpulseSource impulseSource;

    int currentAmmo;
    int currentMaxAmmo; // Runtime max ammo
    
    public int CurrentAmmo => currentAmmo;
    public bool HasAmmo => currentAmmo > 0;
    public WeaponData Data => weaponData;

    private void Awake()
    {
        currentMaxAmmo = weaponData.maxAmmo;
        currentAmmo = currentMaxAmmo;
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shoot() 
    {
        if (!HasAmmo) return;

        currentAmmo--;
        muzzleFlash.Play();
        impulseSource.GenerateImpulse();
    }

    public void RefillAmmo()
    {
        currentAmmo = currentMaxAmmo;
    }
    
    public void IncreaseMaxAmmo(int amount)
    {
        currentMaxAmmo += amount;
        currentAmmo = currentMaxAmmo;
    }
    
    public int GetMaxAmmo()
    {
        return currentMaxAmmo;
    }
    
    public void ResetMaxAmmo()
    {
        currentMaxAmmo = weaponData.maxAmmo;
        currentAmmo = currentMaxAmmo;
    }
    public void AddAmmo(int amount)
{
    currentAmmo = Mathf.Min(currentAmmo + amount, currentMaxAmmo);
    Debug.Log($"Added {amount} ammo! Current: {currentAmmo}/{currentMaxAmmo}");
}
}