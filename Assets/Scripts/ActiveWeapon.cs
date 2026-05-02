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
    private int pickupLayerMask;
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
   
   // Create layer mask that ignores Pickup layer
   int pickupLayer = LayerMask.NameToLayer("Pickup");
   pickupLayerMask = ~(1 << pickupLayer);
}

    private void OnEnable()
    {
        // Reset input state when enabled
        if (inputs != null)
            inputs.ShootInput(false);
    }

    private void Update()
    {
        // Safety check - if time is frozen, don't process
        if (Time.timeScale == 0f) return;
        
        HandleShoot();
    }
    
    private void HandleShoot()
    {
        bool canFire = Time.time >= nextFireTime;

        if (!canFire || currentWeapon == null || !currentWeapon.HasAmmo) {
            return;
        }
        
        // Check if shoot button is pressed
        bool isShooting = false;
        if (weaponData.isAutomatic)
        {
            isShooting = shootAction.IsPressed();
        }
        else
        {
            isShooting = inputs.shoot;
        }
        
        if (!isShooting) return;
        
        // Reset input for semi-auto weapons immediately to prevent multiple shots
        if (!weaponData.isAutomatic)
        {
            inputs.ShootInput(false);
        }

        nextFireTime = Time.time + (1.0f / weaponData.fireRate);
        animator.Play(SHOOT_ANIMATION_TRIGGER, 0, 0f);
        currentWeapon.Shoot();
        RaycastHit hit;

        explosiveProcCounter++;
        
        float yawKick = Random.Range(-weaponData.recoilX, weaponData.recoilX);
        controller.ApplyRecoil(weaponData.recoilY, yawKick);

        if (AudioManager.Instance != null)
        {
            if (weaponData.weaponName == "AR")
                AudioManager.Instance.PlaySFX(AudioManager.Instance.arShootSound, 0.5f);
            else if (weaponData.weaponName == "Sniper")
                AudioManager.Instance.PlaySFX(AudioManager.Instance.rifleShootSound, 0.7f);
            else if (weaponData.weaponName == "Pistol")
                AudioManager.Instance.PlaySFX(AudioManager.Instance.pistolShootSound, 0.2f);
        }

        if (CrosshairManager.Instance != null)
        {
            CrosshairManager.Instance.AddSpread(0.2f);
        }

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward,
            out hit, weaponData.range, pickupLayerMask))
        {
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name}, Tag: {hit.collider.tag}, Layer: {hit.collider.gameObject.layer}");
            EnemyHealth health = hit.collider.GetComponent<EnemyHealth>();
            
            if (health != null)
            {
                Debug.Log($"ENEMY HIT! Health before: {health.CurrentHealth}");
                int totalDamage = weaponData.damage;
                if (UpgradeManager.Instance != null)
                    totalDamage += UpgradeManager.Instance.GetDamageBonus();
                
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
                
                if (UpgradeManager.Instance != null && UpgradeManager.Instance.HasExplosiveRounds())
                {
                    if (explosiveProcCounter >= 5)
                    {
                        explosiveProcCounter = 0;
                        
                        if (explosionVFXPrefab != null)
                        {
                            GameObject explosion = Instantiate(explosionVFXPrefab, hit.point, Quaternion.identity);
                            Destroy(explosion, 1f);
                        }
                        
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
        explosiveProcCounter = 0;
    }
}