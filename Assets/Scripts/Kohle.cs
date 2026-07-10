using UnityEngine;

public class Kohle : MonoBehaviour
{
    private bool isCollected = false;

    public Vector3 offset = new Vector3(1f, 0.5f, 0f);
    public float kohleWert = 10f;


    public void Collect()
    {
        isCollected = true;

        GetComponent<Collider>().enabled = false;
    }

    void Update()
    {
        if (isCollected)
        {
            Transform player = PlayerController.Instance.transform;

            transform.position = PlayerController.Instance.transform.position
                   + PlayerController.Instance.transform.forward * 1.5f
                   + Vector3.up * 0.5f;
        }

        if (PlayerController.Instance.kohleInInventory <= 0 && isCollected) 
        {
            Destroy(gameObject);
        }

    }
}