using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float health = 100f;

    [Header("Hit Flash")]
    [SerializeField] private Renderer[] enemyRenderers;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashCoroutine;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        if (enemyRenderers == null || enemyRenderers.Length == 0)
        {
            enemyRenderers = GetComponentsInChildren<Renderer>();
        }
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