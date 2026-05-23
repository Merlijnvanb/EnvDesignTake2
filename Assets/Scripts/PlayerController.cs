using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;
    public Transform cameraTransform;

    [Header("Head Bob")]
    public float bobFrequency = 8f;
    public float bobIntensity = 0.05f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private Vector3 _moveInput;
    private float _xRotation;

    private Vector3 _defaultCameraLocalPos;
    private float _bobTimer;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform != null)
            _defaultCameraLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleHeadBob();
    }

    void HandleMovement()
    {
        bool grounded = _controller.isGrounded;
        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector3(x, 0f, z);
        Vector3 move = (transform.right * x + transform.forward * z).normalized;
        _controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && grounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -maxLookAngle, maxLookAngle);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        _controller.enabled = false;
        transform.position = position;
        float yaw = rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        _xRotation = 0f;
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.identity;
        _velocity = Vector3.zero;
        _controller.enabled = true;
    }

    void HandleHeadBob()
    {
        if (cameraTransform == null) return;

        bool moving = _controller.isGrounded && _moveInput.magnitude > 0.1f;

        if (moving)
            _bobTimer += Time.deltaTime * bobFrequency;
        else
            _bobTimer = 0f;

        Vector3 targetPos = _defaultCameraLocalPos;
        if (moving)
            targetPos += new Vector3(
                Mathf.Sin(_bobTimer * 0.5f) * bobIntensity,
                Mathf.Sin(_bobTimer) * bobIntensity,
                0f
            );

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetPos,
            Time.deltaTime * 15f
        );
    }
}
