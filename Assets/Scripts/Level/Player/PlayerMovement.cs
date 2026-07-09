using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float acceleration = 20f;
    public float airControlMultiplier = 0.4f;

    [Header("Gravity")]
    [SerializeField] private float gravity = 15f;

    [Header("Jump")]
    public float jumpForce = 6f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Look")]
    public Transform cameraPivot;
    public float mouseSensitivity = 0.1f;
    public float minY = -80f;
    public float maxY = 80f;

    private Rigidbody rb;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float currentSpeed;
    private float cameraPitch;

    private bool isGrounded;
    public bool IsGrounded => isGrounded;
    public float MaxSpeed => maxSpeed;

    private bool jumpRequested;

private CapsuleCollider capsule;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;

        capsule = GetComponent<CapsuleCollider>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /* ================= INPUT ================= */

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

   public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpRequested = true;
        }
    }
  

    void Update()
    {
        HandleLook();
    }

    void FixedUpdate()
    {
        CheckGround();
        HandleMovement();

        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        if (jumpRequested)
        {
            jumpRequested = false;

            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

  

    void HandleMovement()
    {
        Vector3 inputDirection =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        float targetSpeed = inputDirection.magnitude * maxSpeed;

        float accel = isGrounded ? acceleration : acceleration * airControlMultiplier;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * Time.fixedDeltaTime);

        Vector3 desiredVelocity = inputDirection.normalized * currentSpeed;

        rb.linearVelocity = new Vector3(
            desiredVelocity.x,
            rb.linearVelocity.y,
            desiredVelocity.z
        );
    }



    void HandleLook()
    {
        // Горизонтальный поворот игрока
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        // Вертикальный поворот камеры
        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, minY, maxY);

        cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

  

    void CheckGround()
    {
        float radius = capsule.radius * 0.95f;

        Vector3 bottom =
            transform.position +
            capsule.center -
            Vector3.up * (capsule.height / 2f - capsule.radius);

        isGrounded = Physics.CheckSphere(
            bottom + Vector3.down * 0.05f,
            radius,
            groundLayer
        );
    }

}