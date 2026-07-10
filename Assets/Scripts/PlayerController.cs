using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public float speed = 5.0f;
    public int kohleInInventory = 0;
    public float worth = 10.0f;

    void Awake()
    {
        Instance = this;
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);

        if (movement != Vector3.zero)
        {
            transform.forward = movement.normalized;
        }

        transform.Translate(movement.normalized * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Kohle") && kohleInInventory == 0)
        {
            kohleInInventory++;

            Kohle kohle = other.GetComponent<Kohle>();

            if (kohle != null)
            {
                kohle.Collect();
            }
        }
        else if (other.CompareTag("Offen"))
        {
            GameManager.Instance.kohle += kohleInInventory * worth;
            kohleInInventory = 0;
        }
    }
}
