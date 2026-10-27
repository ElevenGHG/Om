using UnityEngine;
using System.Collections;
using System;           
using TMPro;

public class EnemyWeapon : MonoBehaviour
{
    [Header("Weapon References")]
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

    [SerializeField] private GameObject player;
    [SerializeField] private Enemy enemy;

    [SerializeField] private float viewDistance = 20f;

    private int currentAmmo;
    private float nextFireTime;
    private bool isReloading;

    private void Start()
    {
        currentAmmo = magazineSize;
        player = GameObject.FindGameObjectWithTag("Player");
        enemy = GetComponentInParent<Enemy>();
    }

    private bool CanSeePlayer()
    {
        // Richtung vom Gegner zum Spieler berechnen.
        Vector3 directionToPlayer =
            (player.transform.position - muzzle.position).normalized;

        // Abstand zum Spieler berechnen.
        float distanceToPlayer = Vector3.Distance(
            muzzle.position,
            player.transform.position
        );

        // Prüfen, was der Gegner zuerst trifft.
        if (Physics.Raycast(
            muzzle.position,
            directionToPlayer,
            out RaycastHit hit,
            Mathf.Min(viewDistance, distanceToPlayer),
            hitLayers))
        {
            // Sichtkontakt besteht, wenn der Treffer zum Spieler gehört.
            return hit.transform == player.transform
                || hit.transform.IsChildOf(player.transform);
        }

        // Kein Spieler getroffen: kein Sichtkontakt.
        return false;
    }

    private void Update()
    {
        // Prüfen, ob ein Spieler zugewiesen wurde.
        if (player == null)
            return;

        // Nachladen, wenn Munition fehlt und gerade nicht nachgeladen wird.
        if (!isReloading && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
        }

        // Abstand zwischen Gegner und Spieler berechnen.
        float distanceToPlayer = Vector3.Distance(
            muzzle.position,
            player.transform.position
        );

        // Prüfen, ob der Spieler nah genug ist.
        bool playerInRange = distanceToPlayer <= range;

        // Prüfen, ob der Spieler in Sichtweite ist.
        bool playerVisible = CanSeePlayer();

        // Nur schießen, wenn der Spieler in Reichweite und sichtbar ist.
        if (playerInRange && playerVisible && enemy != null && enemy.IsAimedAtPlayer() && !isReloading)
        {
            if (currentAmmo > 0 && Time.time >= nextFireTime)
            {
                Shoot();

                currentAmmo--;

                nextFireTime = Time.time
                    + 1f / Mathf.Max(shotsPerSecond, 0.01f);
            }
            else if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
            }
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
    }

    private void Shoot()
    {
        Vector3 targetPoint = player.transform.position;

        Vector3 shootDirection =
            (targetPoint - muzzle.position).normalized;

        if (Physics.Raycast(muzzle.position, shootDirection, out RaycastHit weaponHit, range, hitLayers))
        {
            ThirdPersonController player =
                weaponHit.collider.GetComponentInParent<ThirdPersonController>();

            if (player != null)
            {
                player.TakeDamage((int)damage);
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