using UnityEngine;
using System.Collections;
using TMPro;

public class Weapon : MonoBehaviour
{
    [Header("Weapon References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform muzzle;
    [SerializeField] private BulletTracer bulletTracerPrefab;

    [Header("Weapon Stats")]
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField] private float damage = 25f;

    [Header("Fire Rate")]
    [SerializeField] private float shotsPerSecond = 8f;

    [Tooltip("Aktiviert: Maustaste halten. Deaktiviert: einzeln klicken.")]
    [SerializeField] private bool automaticFire = true;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("Ammo UI")]
    [SerializeField] private TMP_Text ammoText;

    [Header("Aim")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float aimFOV = 40f;
    [SerializeField] private float aimSpeed = 10f;

    private int currentAmmo;
    private float nextFireTime;
    private bool isReloading;
    private bool isAiming;

    private void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoUI();
    }

    private void Update()
    {
        Aim();

        // Nachladen mit R
        if (Input.GetKeyDown(KeyCode.R)
            && !isReloading
            && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
        }

        // Automatisch oder einzeln schießen
        bool wantsToShoot = automaticFire
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (wantsToShoot && !isReloading)
        {
            if (currentAmmo > 0 && Time.time >= nextFireTime)
            {
                Shoot();

                currentAmmo--;
                UpdateAmmoUI();

                nextFireTime = Time.time + 1f / Mathf.Max(shotsPerSecond, 0.01f);
            }
            else if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
            }
        }
    }

    private void Aim()
    {
        isAiming = Input.GetMouseButton(1);

        float targetFOV = isAiming ? aimFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * aimSpeed
        );
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        UpdateAmmoUI();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;

        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (ammoText == null)
            return;

        if (isReloading)
        {
            ammoText.text = "Reloading...";
        }
        else
        {
            ammoText.text = currentAmmo + " / " + magazineSize;
        }
    }

    private Vector3 GetTargetPoint()
    {
        Ray cameraRay = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint =
            cameraRay.origin + cameraRay.direction * range;

        if (Physics.Raycast(
            cameraRay,
            out RaycastHit cameraHit,
            range,
            hitLayers))
        {
            targetPoint = cameraHit.point;
        }

        return targetPoint;
    }

    private void Shoot()
    {
        Vector3 targetPoint = GetTargetPoint();

        Vector3 shootDirection =
            (targetPoint - muzzle.position).normalized;

        if (Physics.Raycast(muzzle.position, shootDirection, out RaycastHit weaponHit, range,hitLayers))
        {
            Enemy enemy =
                weaponHit.collider.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            BulletTracer tracer = Instantiate(bulletTracerPrefab);
            tracer.Show(muzzle.position, weaponHit.point);
        }
        else
        {
            Vector3 endPoint =
                muzzle.position + shootDirection * range;

            BulletTracer tracer = Instantiate(bulletTracerPrefab);
            tracer.Show(muzzle.position, endPoint);
        }
    }
}