using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public float health = 100f;

    [Header("Player Detection")]
    [SerializeField] private Transform player;
    [SerializeField] private float viewDistance = 20f;
    [SerializeField] private LayerMask visionLayers = ~0;

    [Header("Rotation")]
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float shootAngle = 10f;

    [Header("Hit Flash")]
    [SerializeField] private Renderer[] enemyRenderers;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashCoroutine;

    private static readonly int BaseColorID =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorID =
        Shader.PropertyToID("_Color");


    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        if (enemyRenderers == null || enemyRenderers.Length == 0)
        {
            enemyRenderers = GetComponentsInChildren<Renderer>();
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    private void Update()
    {
        // Nur drehen, wenn ein Spieler zugewiesen wurde
        // und der Gegner ihn sehen kann.
        if (player != null && CanSeePlayer())
        {
            LookAtPlayer();
        }
    }

    public bool IsAimedAtPlayer()
    {
        if (player == null)
            return false;

        // Richtung vom Gegner zum Spieler berechnen.
        Vector3 directionToPlayer =
            player.position - transform.position;

        // Nur die horizontale Richtung berücksichtigen.
        directionToPlayer.y = 0f;

        // Prüfen, ob die Richtung gültig ist.
        if (directionToPlayer.sqrMagnitude < 0.001f)
            return true;

        // Winkel zwischen Blickrichtung des Gegners
        // und Richtung zum Spieler berechnen.
        float angle = Vector3.Angle(
            transform.forward,
            directionToPlayer.normalized
        );

        // Nur schießen erlauben, wenn der Winkel klein genug ist.
        return angle <= shootAngle;
    }

    private bool CanSeePlayer()
    {
        // Position des Gegners auf Augenhöhe.
        Vector3 eyePosition = transform.position + Vector3.up;

        // Zielpunkt ungefähr auf Höhe des Oberkörpers.
        Vector3 targetPosition = player.position + Vector3.up;

        // Richtung vom Gegner zum Spieler.
        Vector3 directionToPlayer =
            targetPosition - eyePosition;

        // Abstand zwischen Gegner und Spieler.
        float distanceToPlayer = directionToPlayer.magnitude;

        // Prüfen, ob der Spieler innerhalb der Sichtweite ist.
        if (distanceToPlayer > viewDistance)
        {
            return false;
        }

        // Den Richtungsvektor auf die Länge 1 normieren.
        directionToPlayer.Normalize();

        // Prüfen, welches Objekt der Sichtstrahl zuerst trifft.
        if (Physics.Raycast(
            eyePosition,
            directionToPlayer,
            out RaycastHit hit,
            viewDistance,
            visionLayers))
        {
            // Sichtkontakt besteht, wenn der Spieler
            // oder eines seiner untergeordneten Objekte getroffen wird.
            return hit.transform == player ||
                   hit.transform.IsChildOf(player);
        }

        // Kein Objekt getroffen: kein Sichtkontakt.
        return false;
    }


    private void LookAtPlayer()
    {
        // Richtung zum Spieler berechnen.
        Vector3 direction = player.position - transform.position;

        // Nur horizontal drehen, damit der Gegner
        // sich nicht nach oben oder unten neigt.
        direction.y = 0f;

        // Verhindern, dass eine ungültige Richtung entsteht,
        // wenn Gegner und Spieler exakt übereinander stehen.
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        // Gewünschte Rotation in Richtung des Spielers berechnen.
        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        // Den Gegner weich in Richtung des Spielers drehen.
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }


    public void TakeDamage(float damage)
    {
        health -= damage;

        Debug.Log("Gegner hat noch " + health + " Leben.");

        if (health <= 0)
        {
            Die();
            return;
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(HitFlash());
    }


    private IEnumerator HitFlash()
    {
        foreach (Renderer rend in enemyRenderers)
        {
            if (rend == null) continue;

            rend.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor(BaseColorID, flashColor);
            propertyBlock.SetColor(ColorID, flashColor);

            rend.SetPropertyBlock(propertyBlock);
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (Renderer rend in enemyRenderers)
        {
            if (rend == null) continue;

            rend.SetPropertyBlock(null);
        }

        flashCoroutine = null;
    }


    private void Die()
    {
        Destroy(gameObject);
    }
}