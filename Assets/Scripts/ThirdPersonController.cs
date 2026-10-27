using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

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
    [SerializeField] private Vector3 cameraDistance = new Vector3(0f, 0f, 0f);
    [SerializeField] private float cameraSpeed = 7f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 65f;

    [Header("Player Rotation")]
    [SerializeField] private bool rotateWithCamera = true;
    [SerializeField] private float bodyTurnSpeed = 15f;

    [Header("Health")]
    public int health = 100;
    [SerializeField] private int maxHealth = 100;

    [Header("Health Regeneration")]
    [SerializeField] private float regenerationDelay = 4f;
    [SerializeField] private float regenerationPerSecond = 10f;

    [Header("Hit Flash")]
    [SerializeField] private Renderer[] playerRenderers;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashCoroutine;

    private static readonly int BaseColorID =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorID =
        Shader.PropertyToID("_Color");

    private float lastDamageTime;
    private float regenerationProgress;

    [Header("UI")]
    [SerializeField] private TMP_Text healthBarUI;
    [SerializeField] private TMP_Text gameOverText;

    public GameObject mainCamera;

    private CharacterController controller;

    private float verticalVelocity;
    private float yaw;
    private float pitch;

    private bool cameraOnLeft;

    private Vector3 currentHorizontalVelocity;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            playerRenderers = GetComponentsInChildren<Renderer>();
        }

        controller = GetComponent<CharacterController>();

        health = Mathf.Clamp(health, 0, maxHealth);
        lastDamageTime = Time.time;

        UpdateHealthUI();

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

    private void OnEnable()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void UpdateHealthUI()
    {
        if (healthBarUI == null)
            return;

        healthBarUI.text = $"Health: {health}";
    }

    private void Update()
    {
        CameraOffset();
        Look();
        Move();
        RegenerateHealth();
    }

    private void RegenerateHealth()
    {
        // Bei 0 Leben oder vollen Lebenspunkten nicht regenerieren.
        if (health <= 0 || health >= maxHealth)
        {
            regenerationProgress = 0f;
            return;
        }

        // Warten, bis der Spieler lange genug keinen Schaden
        // mehr erhalten hat.
        if (Time.time - lastDamageTime < regenerationDelay)
        {
            return;
        }

        // Regenerationsfortschritt sammeln.
        regenerationProgress +=
            regenerationPerSecond * Time.deltaTime;

        // Ganze Lebenspunkte wiederherstellen.
        int healAmount = Mathf.FloorToInt(regenerationProgress);

        if (healAmount > 0)
        {
            int oldHealth = health;

            health = Mathf.Min(maxHealth, health + healAmount);

            regenerationProgress -= healAmount;

            // Nur aktualisieren, wenn sich das Leben verändert hat.
            if (health != oldHealth)
            {
                UpdateHealthUI();
            }
        }
    }

    private void CameraOffset()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            cameraOnLeft = !cameraOnLeft;
        }

        Vector3 targetOffset = cameraOnLeft
            ? new Vector3(-1f, -0.5f, -3f)
            : new Vector3(1f, -0.5f, -3f);

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

        if (rotateWithCamera)
        {
            Quaternion targetRotation =
                Quaternion.Euler(0f, yaw, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                bodyTurnSpeed * Time.deltaTime
            );
        }
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

        // Diagonale Bewegung begrenzen
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

        // Zielgeschwindigkeit
        Vector3 targetVelocity = moveDirection * currentSpeed;

        // Beschleunigen und abbremsen
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

    public void TakeDamage(int damage)
    {
        // Nach dem Tod keinen weiteren Schaden verarbeiten.
        if (health <= 0 || damage <= 0)
            return;

        health = Mathf.Max(0, health - damage);

        // Timer zurücksetzen, weil der Spieler Schaden erhalten hat.
        lastDamageTime = Time.time;
        regenerationProgress = 0f;

        UpdateHealthUI();

        if (health <= 0)
        {
            Die();
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        foreach (Renderer rend in playerRenderers)
        {
            if (rend == null) continue;

            rend.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor(BaseColorID, flashColor);
            propertyBlock.SetColor(ColorID, flashColor);

            rend.SetPropertyBlock(propertyBlock);
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (Renderer rend in playerRenderers)
        {
            if (rend == null) continue;

            rend.SetPropertyBlock(null);
        }

        flashCoroutine = null;
    }

    private void Die()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void ResetCameraView()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.rotation =
                Quaternion.Euler(0f, 0f, 0f);
        }

        cameraTransform.localRotation = Quaternion.identity;

        yaw = transform.eulerAngles.y;
        cameraDistance = Vector3.zero;
        pitch = 10f;

        cameraPivot.position = transform.position
            + Vector3.up * 1.5f;

        cameraPivot.rotation = Quaternion.Euler(
            pitch,
            yaw,
            0f
        );

        cameraTransform.localPosition = cameraDistance;
    }

    private void OnDisable()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }
}