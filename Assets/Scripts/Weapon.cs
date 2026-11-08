
using System.Collections;
using UnityEngine;
using TMPro;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float damage = 10f;
    public float fireRate = 0.5f;
    public int magazineSize = 30;
    public float reloadTime = 2f;

    [Header("Aim Settings")]
    public Camera playerCamera;
    public float normalFOV = 60f;
    public float aimingFOV = 40f;
    public float aimSpeed = 10f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    public Transform cameraTransform;

    bool canShoot = true;
    bool isReloading = false;
    bool isAiming = false;

    int currentAmmo;

    private void Start()
    {
        currentAmmo = magazineSize;
        updateAmmoText();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null)
            playerCamera.fieldOfView = normalFOV;
    }

    private void Update()
    {
        // Aiming
        isAiming = Input.GetMouseButton(1);

        if (playerCamera != null)
        {
            float targetFOV = isAiming ? aimingFOV : normalFOV;

            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                aimSpeed * Time.deltaTime
            );
        }

        // Shooting
        if (Input.GetMouseButton(0) &&
            currentAmmo > 0 &&
            canShoot &&
            !isReloading)
        {
            shoot();
        }

        // Manual Reload
        else if (Input.GetKeyDown(KeyCode.R) &&
                 currentAmmo < magazineSize &&
                 !isReloading)
        {
            StartCoroutine(Reload());
        }

        // Automatic Reload
        else if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    void shoot()
    {
        currentAmmo--;
        updateAmmoText();

        RaycastHit hit;

        if (Physics.Raycast(
            cameraTransform.position,
            cameraTransform.forward,
            out hit,
            100f))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        canShoot = false;
        StartCoroutine(ShootGun());
    }

    void updateAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + magazineSize;
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        canShoot = false;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        updateAmmoText();

        isReloading = false;
        canShoot = true;
    }

    IEnumerator ShootGun()
    {
        yield return new WaitForSeconds(fireRate);

        canShoot = true;
    }
}
