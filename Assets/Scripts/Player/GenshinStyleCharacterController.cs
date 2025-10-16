using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections;

public class GenshinStyleCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float rotationSpeed = 10f;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float maxFallSpeed = -20f;
    public int maxJumps = 3;
    public float jumpStartToJumpDelay = 0.5f;

    [Header("References")]
    public Animator animator;
    public Transform cameraTarget;

    // Private variables
    private CharacterController characterController;
    private PlayerInput playerInput;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isSprinting;
    private int currentJumps;
    private bool isGrounded;
    private float verticalVelocity;

    // Input actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    // Animation state tracking
    private int currentAnimState = 0;
    private Coroutine jumpTransitionCoroutine;
    private bool isJumping = false;
    private bool isInLandingState = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Get input actions
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Dash"];
    }

    private void Start()
    {
        // Subscribe to input events
        jumpAction.performed += OnJumpPerformed;
        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;
    }

    private void OnEnable()
    {
        // Enable the actions
        moveAction?.Enable();
        lookAction?.Enable();
        jumpAction?.Enable();
        sprintAction?.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        jumpAction.performed -= OnJumpPerformed;
        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;

        // Disable the actions
        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
        sprintAction?.Disable();

        // Stop any running coroutines
        if (jumpTransitionCoroutine != null)
        {
            StopCoroutine(jumpTransitionCoroutine);
        }
    }

    private void Update()
    {
        HandleInput();
        HandleRotation();
        HandleMovement();
        HandleGravityAndJumping();
        UpdateAnimations();
    }

    private void HandleInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        // Check if we can sprint (only when moving forward)
        if (isSprinting && moveInput.y <= 0)
        {
            isSprinting = false;
        }
    }

    private void HandleRotation()
    {
        // Get mouse input for rotation
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        if (lookInput.magnitude > 0.1f)
        {
            // Rotate character based on mouse movement
            transform.Rotate(0, lookInput.x * rotationSpeed * Time.deltaTime, 0);
        }

        // Update camera target position to follow player
        if (cameraTarget != null)
        {
            cameraTarget.position = transform.position;
        }
    }

    private void HandleMovement()
    {
        // Calculate movement direction relative to character's forward
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);

        // Convert to world space relative to character's rotation
        movement = transform.TransformDirection(movement);

        // Apply movement speed
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 horizontalMovement = movement * currentSpeed;

        // Apply horizontal movement while preserving vertical velocity
        velocity = new Vector3(horizontalMovement.x, velocity.y, horizontalMovement.z);

        // Move the character
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleGravityAndJumping()
    {
        // Store previous grounded state
        bool wasGrounded = isGrounded;

        // Ground check
        isGrounded = characterController.isGrounded;

        if (isGrounded)
        {
            // Reset jumps when grounded
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;

                // Only reset jumps and landing state when we were previously in the air
                if (!wasGrounded && isJumping)
                {
                    currentJumps = 0;
                    isJumping = false;
                    OnJumpLand();
                }
            }
        }
        else
        {
            // We're in the air, so we're jumping
            isJumping = true;
        }

        // Apply gravity
        verticalVelocity += gravity * Time.deltaTime;

        // Clamp fall speed
        verticalVelocity = Mathf.Max(verticalVelocity, maxFallSpeed);

        // Update velocity with vertical component
        velocity.y = verticalVelocity;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (currentJumps < maxJumps && !isInLandingState)
        {
            // Calculate jump velocity based on height and gravity
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            currentJumps++;
            isJumping = true;
            isInLandingState = false;

            // Trigger appropriate jump animation
            StartJumpAnimation();
        }
    }

    private void StartJumpAnimation()
    {
        // Set initial jump animation based on current jump count
        int initialJumpState = 5; // jump1

        switch (currentJumps)
        {
            case 1:
                initialJumpState = 5; // jump1
                break;
            case 2:
                initialJumpState = 6; // jump2
                break;
            case 3:
                initialJumpState = 7; // jump3
                break;
        }

        // Set the initial jump state
        SetAnimationState(initialJumpState);
    }

    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        // Only allow sprinting when moving forward
        if (moveInput.y > 0)
        {
            isSprinting = true;
        }
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        isSprinting = false;
    }

    private void UpdateAnimations()
    {
        // Don't update movement animations if we're jumping or landing
        if (isJumping || isInLandingState)
            return;

        // Handle movement animations
        int newAnimState = 0; // Default to idle

        if (moveInput.magnitude > 0.1f)
        {
            if (isSprinting && moveInput.y > 0.1f) // Only sprint when moving forward with significant input
            {
                newAnimState = 10; // Sprint
            }
            else
            {
                // Convert world movement direction to local space relative to character
                Vector3 worldDirection = new Vector3(velocity.x, 0, velocity.z).normalized;
                Vector3 localDirection = transform.InverseTransformDirection(worldDirection);

                // Determine the dominant movement direction
                float forwardAmount = localDirection.z;
                float rightAmount = localDirection.x;

                // Use thresholds to determine the animation state
                if (Mathf.Abs(forwardAmount) > Mathf.Abs(rightAmount))
                {
                    // Forward/backward movement is dominant
                    if (forwardAmount > 0.3f)
                        newAnimState = 1; // Forward
                    else if (forwardAmount < -0.3f)
                        newAnimState = 2; // Backward
                    else
                        newAnimState = 0; // Idle (no significant movement)
                }
                else
                {
                    // Left/right movement is dominant
                    if (rightAmount > 0.3f)
                        newAnimState = 4; // Right
                    else if (rightAmount < -0.3f)
                        newAnimState = 3; // Left
                    else
                        newAnimState = 0; // Idle (no significant movement)
                }
            }
        }

        // Only update animation if state changed
        if (newAnimState != currentAnimState)
        {
            SetAnimationState(newAnimState);
        }
    }

    private void SetAnimationState(int newState)
    {
        currentAnimState = newState;
        animator.SetInteger("AnimState", currentAnimState);

        // Debug log to see what animation state is being set
        Debug.Log($"Setting animation state to: {newState}");
    }

    // Called when the character lands from a jump
    public void OnJumpLand()
    {
        if (isGrounded)
        {
            // Stop any running jump transitions
            if (jumpTransitionCoroutine != null)
            {
                StopCoroutine(jumpTransitionCoroutine);
                jumpTransitionCoroutine = null;
            }

            isInLandingState = true;
            SetAnimationState(9); // Land

            // Return to idle after landing animation
            Invoke(nameof(ReturnToIdle), 0.35f);
        }
    }

    private void ReturnToIdle()
    {
        if (isGrounded && currentAnimState == 9)
        {
            //SetAnimationState(0); // Idle
            isInLandingState = false;
        }
    }

    // Public methods for external access
    public bool IsSprinting => isSprinting;
    public bool IsGrounded => isGrounded;
    public int CurrentJumps => currentJumps;
}