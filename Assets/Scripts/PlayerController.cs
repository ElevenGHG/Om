using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public GameObject mainCamera;
    private CharacterController controller;

    public float speed = 5.0f;
    public int kohleInInventory = 0;
    public float worth = 10.0f;
    public Vector3 cameraRotation = new Vector3(30, 0, 0);
    public Vector3 cameraOffset = new Vector3(0, 8, -10);

    void Awake()
    {
        Instance = this;
        controller = GetComponent<CharacterController>();
    }

    public void ApplyCameraSettings()
    {
        mainCamera.transform.position =
            transform.position + cameraOffset;

        mainCamera.transform.rotation =
            Quaternion.Euler(cameraRotation);
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(
            horizontalInput,
            0f,
            verticalInput
        );

        movement = Vector3.ClampMagnitude(movement, 1f);

        if (movement.sqrMagnitude > 0.001f)
        {
            transform.forward = movement.normalized;
        }

        controller.Move(movement * speed * Time.deltaTime);

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
