using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class FirstPersonMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public KeyCode runningKey = KeyCode.LeftShift;
    public bool IsRunning { get; private set; }
    
    [Header("Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;

    [Header("Jump")]
    public float jumpForce = 5f;
    public event System.Action Jumped;

    [Header("Crouch")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchSpeed = 2f;
    public float crouchHeight = 1f;
    private float normalHeight;
    public bool IsCrouched { get; private set; }
    public event System.Action CrouchStart;
    public event System.Action CrouchEnd;
    private CapsuleCollider capsuleCollider;
    private float headNormalY;

    [Header("Zoom")]
    public float defaultFOV = 60f;
    public float maxZoomFOV = 15f;
    public float zoomSensitivity = 1f;
    private float currentZoom = 0f;
    private Camera playerCamera;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.25f;
    public LayerMask groundMask = Physics.DefaultRaycastLayers;
    public bool isGrounded { get; private set; }
    public event System.Action Grounded;

    private Rigidbody rb;
    private Vector2 inputDirection;
    private bool jumpRequested = false;
    private bool wasGrounded = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        normalHeight = capsuleCollider.height;
        
        if (cameraTransform == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera != null) cameraTransform = playerCamera.transform;
        }
        else
        {
            playerCamera = cameraTransform.GetComponent<Camera>();
        }

        if (cameraTransform != null)
        {
            headNormalY = cameraTransform.localPosition.y;
            if (playerCamera != null) defaultFOV = playerCamera.fieldOfView;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Look
        if (cameraTransform != null)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        // 2. Zoom
        if (playerCamera != null)
        {
            currentZoom += Input.mouseScrollDelta.y * zoomSensitivity * 0.05f;
            currentZoom = Mathf.Clamp01(currentZoom);
            playerCamera.fieldOfView = Mathf.Lerp(defaultFOV, maxZoomFOV, currentZoom);
        }

        // 3. Input gathering for physics
        inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        IsRunning = Input.GetKey(runningKey) && !IsCrouched;
        
        if (Input.GetButtonDown("Jump") && isGrounded && !IsCrouched)
        {
            jumpRequested = true;
        }

        // 4. Crouch Toggle & Check
        if (Input.GetKeyDown(crouchKey))
        {
            TryToggleCrouch();
        }
        else if (Input.GetKeyUp(crouchKey) && IsCrouched)
        {
            TryToggleCrouch();
        }
    }

    void FixedUpdate()
    {
        CheckGrounded();

        // Movement
        float currentSpeed = IsCrouched ? crouchSpeed : (IsRunning ? runSpeed : walkSpeed);
        
        // Calculate target velocity based on input and current rotation
        Vector3 targetVelocity = (transform.right * inputDirection.x + transform.forward * inputDirection.y) * currentSpeed;

        // Apply forces to reach target velocity on X and Z, preserving Y velocity
        Vector3 velocityChange = targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        
        rb.AddForce(new Vector3(velocityChange.x, 0, velocityChange.z), ForceMode.VelocityChange);

        // Jump
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reset vertical velocity before jump
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpRequested = false;
            Jumped?.Invoke();
        }
    }

    private void CheckGrounded()
    {
        float radius = capsuleCollider.radius * 0.9f;
        Vector3 origin = transform.position + Vector3.up * (radius + 0.05f);
        bool currentlyGrounded = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask);

        if (currentlyGrounded && !wasGrounded)
        {
            Grounded?.Invoke();
        }
        
        isGrounded = currentlyGrounded;
        wasGrounded = currentlyGrounded;
    }

    private void TryToggleCrouch()
    {
        if (IsCrouched)
        {
            // Try to stand up - check headroom
            Vector3 castOrigin = transform.position + Vector3.up * crouchHeight;
            float castDistance = normalHeight - crouchHeight;
            float radius = capsuleCollider.radius * 0.9f;
            
            if (Physics.SphereCast(castOrigin, radius, Vector3.up, out RaycastHit hit, castDistance, groundMask))
            {
                // Headroom blocked, cannot stand up
                return;
            }

            // Stand up
            capsuleCollider.height = normalHeight;
            capsuleCollider.center = Vector3.up * (normalHeight / 2f);
            if (cameraTransform != null) cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, headNormalY, cameraTransform.localPosition.z);
            IsCrouched = false;
            CrouchEnd?.Invoke();
        }
        else
        {
            // Crouch down
            capsuleCollider.height = crouchHeight;
            capsuleCollider.center = Vector3.up * (crouchHeight / 2f);
            if (cameraTransform != null) cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, crouchHeight * 0.8f, cameraTransform.localPosition.z);
            IsCrouched = true;
            CrouchStart?.Invoke();
        }
    }
}