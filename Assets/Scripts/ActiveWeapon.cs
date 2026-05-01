using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] WeaponData weaponData;
    private Weapon currentWeapon;
    StarterAssetsInputs inputs;
    [SerializeField] Animator animator;
    [SerializeField] private GameObject hitFXPrefab;
    [SerializeField] private GameObject explosionVFXPrefab;
    private int explosiveProcCounter = 0;
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

        if (!canFire || currentWeapon == null || !currentWeapon.HasAmmo) {
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

        // Increment shot counter for explosive rounds
        explosiveProcCounter++;
        
        // Apply recoil based on weapon
        float yawKick = Random.Range(-weaponData.recoilX, weaponData.recoilX);
        controller.ApplyRecoil(weaponData.recoilY, yawKick);

        // Add spread to crosshair
        if (CrosshairManager.Instance != null)
        {
            CrosshairManager.Instance.AddSpread(0.2f);
        }

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward,
            out hit, weaponData.range))
        {
            EnemyHealth health = hit.collider.GetComponent<EnemyHealth>();
            
            // ONLY proceed if we hit an enemy
            if (health != null)
            {
                int totalDamage = weaponData.damage;
                if (UpgradeManager.Instance != null)
                    totalDamage += UpgradeManager.Instance.GetDamageBonus();
                
                // Critical hit chance
                bool isCritical = false;
                if (UpgradeManager.Instance != null)
                {
                    int critChance = UpgradeManager.Instance.GetCriticalChance();
                    isCritical = Random.Range(0, 100) < critChance;
                    if (isCritical)
                    {
                        totalDamage *= 2;
                        Debug.Log($"CRITICAL HIT! Damage: {totalDamage}");
                    }
                }
                
                bool willKill = health.CurrentHealth <= totalDamage;
                
                health.TakeDamage(totalDamage);
                
                // Explosive Rounds - ONLY ON EVERY 5TH SHOT
                if (UpgradeManager.Instance != null && UpgradeManager.Instance.HasExplosiveRounds())
                {
                    if (explosiveProcCounter >= 5)
                    {
                        explosiveProcCounter = 0; // Reset counter
                        
                        // Spawn explosion VFX
                        if (explosionVFXPrefab != null)
                        {
                            GameObject explosion = Instantiate(explosionVFXPrefab, hit.point, Quaternion.identity);
                            Destroy(explosion, 1f);
                        }
                        
                        // AoE damage to nearby enemies
                        Collider[] nearby = Physics.OverlapSphere(hit.point, 2f);
                        foreach (Collider col in nearby)
                        {
                            EnemyHealth nearbyEnemy = col.GetComponent<EnemyHealth>();
                            if (nearbyEnemy != null && nearbyEnemy != health)
                            {
                                nearbyEnemy.TakeDamage(Mathf.RoundToInt(totalDamage * 0.7f));
                                Debug.Log("EXPLOSIVE ROUND! Damaged nearby enemy!");
                            }
                        }
                    }
                }
                
                // ONLY show hit marker when hitting an enemy
                if (HitMarker.Instance != null)
                {
                    HitMarker.Instance.ShowHitMarker(willKill, isCritical);
                }
                
                Instantiate(weaponData.hitVFXPrefab, hit.point, Quaternion.identity);
            }
        }
    }

    public void SwitchWeapon(Weapon newWeapon) { 
        if (newWeapon == null) return;
        currentWeapon = newWeapon;
        weaponData = newWeapon.Data;
        nextFireTime = 0;
        inputs.ShootInput(false);
        explosiveProcCounter = 0; // Reset counter when switching weapons
    }
}