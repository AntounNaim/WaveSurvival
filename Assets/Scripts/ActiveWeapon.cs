using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class ActiveWeapon : MonoBehaviour
{

    [SerializeField] WeaponData weaponData;
    private Weapon currentWeapon;
    StarterAssetsInputs inputs;
    [SerializeField] Animator animator;
    [SerializeField] private GameObject hitFXPrefab;
    InputAction shootAction;

    FirstPersonController controller;

    const string SHOOT_ANIMATION_TRIGGER = "Shoot";
    float nextFireTime = 0f;

    public Weapon CurrentWeapon => currentWeapon;

    private void Awake()
    {
       currentWeapon = null;
       inputs = GetComponentInParent<StarterAssetsInputs>();
       shootAction = GetComponentInParent<PlayerInput>().actions["Shoot"];
       controller = GetComponentInParent<FirstPersonController>();
    }

    private void Update()
    {
        HandleShoot();
    }
    private void HandleShoot()
    {
        bool canFire = Time.time >= nextFireTime;

        if (!canFire || currentWeapon == null ||!currentWeapon.HasAmmo) {
            return;
        }
        if (weaponData.isAutomatic)
        {
            if (!shootAction.IsPressed()) return;
        }
        else
        {
            if (!inputs.shoot) return;
            inputs.ShootInput(false);
        }

        nextFireTime = Time.time + (1.0f / weaponData.fireRate);
        animator.Play(SHOOT_ANIMATION_TRIGGER, 0, 0f);
        currentWeapon.Shoot();
        RaycastHit hit;

        float yawKick = Random.Range(-weaponData.recoilX, weaponData.recoilX);
        controller.ApplyRecoil(weaponData.recoilY, yawKick);

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward,
            out hit, weaponData.range))
        {
            EnemyHealth health = hit.collider.GetComponent<EnemyHealth>();
            health?.TakeDamage(weaponData.damage);
            Instantiate(weaponData.hitVFXPrefab, hit.point, Quaternion.identity);
        }
    }

    public void SwitchWeapon(Weapon newWeapon) { 
        if (newWeapon == null) return;
        currentWeapon = newWeapon;
        weaponData = newWeapon.Data;
        nextFireTime = 0;
        inputs.ShootInput(false);
    }
}
