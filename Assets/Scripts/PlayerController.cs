using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravityStrength = 20f;
    public float jumpHeight = 1.5f;

    [Header("Step Climbing")]
    public float stepHeight = 0.4f;
    public float stepSearchOvershoot = 0.01f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;
    public Transform cameraTransform;

    [Header("Head Bob")]
    public float bobFrequency = 8f;
    public float bobIntensity = 0.05f;

    private Rigidbody _rb;
    private Vector3 _gravityDir = Vector3.down;
    private Vector3 _moveInput;
    private float _xRotation;
    private Quaternion _yawRotation;
    private bool _grounded;
    private bool _jumpQueued;

    private Vector3 _defaultCameraLocalPos;
    private float _bobTimer;

    private List<ContactPoint> _contactPoints = new List<ContactPoint>();
    private Vector3 _lastVelocity;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform != null)
            _defaultCameraLocalPos = cameraTransform.localPosition;

        _yawRotation = transform.rotation;
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector3(x, 0f, z);

        if (Input.GetButtonDown("Jump"))
            _jumpQueued = true;

        HandleLook();
        HandleHeadBob();
    }

    void FixedUpdate()
    {
        _lastVelocity = _rb.linearVelocity;
        CheckGrounded();
        HandleMovement();
        HandleStepClimb();
        _contactPoints.Clear();
    }

    void OnCollisionEnter(Collision col) => _contactPoints.AddRange(col.contacts);
    void OnCollisionStay(Collision col) => _contactPoints.AddRange(col.contacts);

    void CheckGrounded()
    {
        // Start sphere above player base so it doesn't overlap the floor at cast start
        _grounded = Physics.SphereCast(
            _rb.position - _gravityDir * 0.5f,
            0.3f,
            _gravityDir,
            out _,
            0.3f
        );
    }

    void HandleMovement()
    {
        Vector3 gravityUp = -_gravityDir;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, gravityUp).normalized;
        Vector3 right   = Vector3.ProjectOnPlane(transform.right,   gravityUp).normalized;
        Vector3 moveDir = (right * _moveInput.x + forward * _moveInput.z).normalized;

        float gravitySpeed = Vector3.Dot(_rb.linearVelocity, _gravityDir);
        _rb.linearVelocity = moveDir * moveSpeed + _gravityDir * gravitySpeed;

        if (_grounded && gravitySpeed > 0f)
            _rb.linearVelocity -= _gravityDir * gravitySpeed;
        else if (!_grounded)
            _rb.AddForce(_gravityDir * gravityStrength, ForceMode.Acceleration);

        if (_jumpQueued && _grounded)
            _rb.linearVelocity += gravityUp * Mathf.Sqrt(2f * gravityStrength * jumpHeight);

        _jumpQueued = false;
    }

    void HandleStepClimb()
    {
        if (!_grounded) return;

        Vector3 gravityUp = -_gravityDir;

        // Find the most upward-facing contact point (ground)
        ContactPoint groundCP = default;
        bool foundGround = false;
        foreach (var cp in _contactPoints)
        {
            float upDot = Vector3.Dot(cp.normal, gravityUp);
            if (upDot > 0.0001f && (!foundGround || upDot > Vector3.Dot(groundCP.normal, gravityUp)))
            {
                groundCP = cp;
                foundGround = true;
            }
        }
        if (!foundGround) return;

        // Only step when moving laterally
        if (Vector3.ProjectOnPlane(_lastVelocity, _gravityDir).sqrMagnitude < 0.0001f) return;

        foreach (var cp in _contactPoints)
        {
            if (TryResolveStep(out Vector3 stepOffset, cp, groundCP, gravityUp))
            {
                _rb.position += stepOffset;
                _rb.linearVelocity = _lastVelocity;
                break;
            }
        }
    }

    bool TryResolveStep(out Vector3 stepOffset, ContactPoint stepCP, ContactPoint groundCP, Vector3 gravityUp)
    {
        stepOffset = Vector3.zero;

        // Must be a wall-like surface (normal not pointing upward)
        if (Vector3.Dot(stepCP.normal, gravityUp) >= 0.4f) return false;

        // Step contact must be within max step height of the ground contact
        float heightDiff = Vector3.Dot(stepCP.point - groundCP.point, gravityUp);
        if (heightDiff >= stepHeight) return false;

        // Direction into the step along the gravity plane
        Vector3 stepInDir = Vector3.ProjectOnPlane(-stepCP.normal, gravityUp).normalized;

        // Origin: at step contact lateral position, raised to max step height above ground contact
        float groundHeight = Vector3.Dot(groundCP.point, gravityUp);
        Vector3 lateralPos  = stepCP.point - gravityUp * Vector3.Dot(stepCP.point, gravityUp);
        Vector3 origin      = lateralPos + gravityUp * (groundHeight + stepHeight + 0.0001f) + stepInDir * stepSearchOvershoot;

        // Raycast downward onto the specific step collider (avoids hitting anything else)
        if (!stepCP.otherCollider.Raycast(new Ray(origin, _gravityDir), out RaycastHit hit, stepHeight))
            return false;

        float heightGain = Vector3.Dot(hit.point - groundCP.point, gravityUp) + 0.0001f;
        if (heightGain <= 0f) return false;

        stepOffset = gravityUp * heightGain + stepInDir * stepSearchOvershoot;
        return true;
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -maxLookAngle, maxLookAngle);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        _yawRotation *= Quaternion.Euler(0f, mouseX, 0f);
        transform.rotation = _yawRotation;
    }

    void HandleHeadBob()
    {
        if (cameraTransform == null) return;

        bool moving = _grounded && _moveInput.magnitude > 0.1f;

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

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        _rb.linearVelocity  = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.position        = position;
        transform.position  = position;
        transform.rotation  = rotation;
        _yawRotation        = rotation;
        _gravityDir         = -(rotation * Vector3.up);
        _xRotation          = 0f;
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.identity;
    }
}
