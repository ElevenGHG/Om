using UnityEngine;
using System.Collections;

public class BulletTracer : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float duration = 0.05f;

    public void Show(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        StartCoroutine(RemoveTracer());
    }

    private IEnumerator RemoveTracer()
    {
        yield return new WaitForSeconds(duration);

        Destroy(gameObject);
    }
}