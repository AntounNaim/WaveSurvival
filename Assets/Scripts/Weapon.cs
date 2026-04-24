using UnityEngine;
using StarterAssets;
using Cinemachine;

public class Weapon : MonoBehaviour
{
    [SerializeField] WeaponData weaponData;
    [SerializeField] ParticleSystem muzzleFlash;
    private CinemachineImpulseSource impulseSource;

    int currentAmmo;
    public int CurrentAmmo => currentAmmo;
    public bool HasAmmo => currentAmmo > 0;
    public WeaponData Data => weaponData;

    private void Awake()
    {
        currentAmmo = weaponData.maxAmmo;
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shoot() {

        if (!HasAmmo) return;

        currentAmmo--;
        muzzleFlash.Play();
        impulseSource.GenerateImpulse();
        Debug.Log(impulseSource);

    }

    public void RefillAmmo()
    {
        currentAmmo = weaponData.maxAmmo;
    }

}
