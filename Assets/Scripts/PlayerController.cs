using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public GameObject mainCamera;

    public float speed = 5.0f;
    public int kohleInInventory = 0;
    public float worth = 10.0f;
    public Vector3 cameraRotation = new Vector3(30, 0, 0);
    public Vector3 cameraOffset = new Vector3(0, 8, -10);

    void Awake()
    {
        Instance = this;
    }

    public void ApplyCameraSettings()
    {
        mainCamera.transform.position =
            transform.position + cameraOffset;

        mainCamera.transform.rotation =
            Quaternion.Euler(cameraRotation);
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

        ApplyCameraSettings();
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
