using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float jumpForce = 8.5f;
    [SerializeField] private float gravity = -22f;
    [SerializeField] private float turnSpeed = 10f;

    [Header("Crouch")]
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    private bool isCrouching;

    [Header("Acceleration")]
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 22f;

    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 1.5f;
    [SerializeField]
    private Vector3 cameraDistance =
        new Vector3(1f, 0f, -4f);
    [SerializeField] private float cameraSpeed = 7f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 65f;

    private CharacterController controller;

    private float verticalVelocity;
    private float yaw;
    private float pitch;

    private bool cameraOnLeft;

    private Vector3 currentHorizontalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraPivot == null || cameraTransform == null)
        {
            Debug.LogError(
                "ThirdPersonController: CameraPivot oder " +
                "CameraTransform fehlt im Inspector.",
                this
            );

            enabled = false;
            return;
        }

        yaw = cameraPivot.eulerAngles.y;
        pitch = 10f;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CameraOffset();
        Look();
        Move();
    }

    private void CameraOffset()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            cameraOnLeft = !cameraOnLeft;
        }

        Vector3 targetOffset = cameraOnLeft
            ? new Vector3(-1f, 0f, -4f)
            : new Vector3(1f, 0f, -4f);

        cameraDistance = Vector3.Lerp(
            cameraDistance,
            targetOffset,
            Time.deltaTime * cameraSpeed
        );
    }

    private void Look()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraPivot.position = transform.position
            + Vector3.up * 1.5f;

        cameraPivot.rotation = Quaternion.Euler(
            pitch,
            yaw,
            0f
        );

        cameraTransform.localPosition = cameraDistance;
    }

    private void Move()
    {
        bool isGrounded = controller.isGrounded;

        if (Input.GetKeyDown(crouchKey))
        {
            isCrouching = !isCrouching;
        }

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Springen
        if (isGrounded && !isCrouching && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = jumpForce;
        }

        // Schwerkraft
        verticalVelocity += gravity * Time.deltaTime;

        // Eingabe begrenzen, damit diagonale Bewegung
        // nicht schneller ist
        Vector3 input = Vector3.ClampMagnitude(
            new Vector3(x, 0f, z),
            1f
        );

        // Bewegungsrichtung relativ zur Kamera
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection =
            forward * input.z + right * input.x;

        // Charakter in Bewegungsrichtung drehen
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        float currentSpeed;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isSprinting)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // Zielgeschwindigkeit berechnen
        Vector3 targetVelocity = moveDirection * currentSpeed;

        // Sanft beschleunigen und abbremsen
        float changeRate = moveDirection.sqrMagnitude > 0.01f
            ? acceleration
            : deceleration;

        currentHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetVelocity,
            changeRate * Time.deltaTime
        );

        // Horizontale und vertikale Bewegung kombinieren
        Vector3 velocity = currentHorizontalVelocity;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}