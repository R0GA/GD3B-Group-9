using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float rotationSpeed = 5f;
    public float maxRotationSpeed = 180f;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float maxFallSpeed = -20f;
    public int maxJumps = 3;
    public float jumpStartToJumpDelay = 0.5f;

    [Header("Attack Settings")]
    public int attackDamage = 25;
    public float attackComboTimeout = 1.5f;
    public float attackMoveSpeed = 3f;
    public float attackInputCooldown = 0.3f;
    public LayerMask enemyLayerMask;

    [Header("References")]
    public Animator animator;
    public Transform cameraTarget;
    public Collider swordHitbox;

    [Header("Player Health")]
    public float health = 100f;
    public float maxHealth = 100f;
    public bool isDead = false;

    // Events for player damage and death
    public System.Action<float> OnDamageTaken;
    public System.Action OnPlayerDeath;

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
    private InputAction attackAction;

    // Animation state tracking
    private int currentAnimState = 0;
    private Coroutine jumpTransitionCoroutine;
    private bool isJumping = false;
    private bool isInLandingState = false;

    // Rotation smoothing
    private float targetRotation;
    private float currentRotationVelocity;

    // Attack system
    private int currentAttackCombo = 0;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private bool attackInputQueued = false;
    private bool canAcceptAttackInput = true;
    private List<GameObject> enemiesHitThisAttack = new List<GameObject>();

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Get input actions
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Dash"];
        attackAction = playerInput.actions["Attack"];

        // Initialize rotation
        targetRotation = transform.eulerAngles.y;

        // Disable sword hitbox initially
        if (swordHitbox != null)
            swordHitbox.enabled = false;

        GameObject spawn = GameObject.FindWithTag("Spawn");
        if (spawn != null)
        {
            transform.position = spawn.transform.position;
        }
    }

    private void Start()
    {
        // Subscribe to input events
        jumpAction.performed += OnJumpPerformed;
        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;
        attackAction.performed += OnAttackPerformed;
    }

    private void OnEnable()
    {
        // Enable the actions
        moveAction?.Enable();
        lookAction?.Enable();
        jumpAction?.Enable();
        sprintAction?.Enable();
        attackAction?.Enable();

        // Subscribe to sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        jumpAction.performed -= OnJumpPerformed;
        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;
        attackAction.performed -= OnAttackPerformed;

        // Disable the actions
        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
        sprintAction?.Disable();
        attackAction?.Disable();

        // Stop any running coroutines
        if (jumpTransitionCoroutine != null)
        {
            StopCoroutine(jumpTransitionCoroutine);
        }

        // Unsubscribe from sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        HandleInput();
        HandleRotation();
        HandleMovement();
        HandleGravityAndJumping();
        HandleAttackComboTimeout();
        HandleAttackInputCooldown();
        UpdateAnimations();
        Debug.Log($"Current Animation State: {currentAnimState} {IsGrounded} {isJumping} {isInLandingState} {isAttacking}");
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
        // Get mouse input for rotation - RESTORED FROM OLD CONTROLLER
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        if (Mathf.Abs(lookInput.x) > 0.1f)
        {
            // Calculate rotation with maximum limit - RESTORED FROM OLD CONTROLLER
            float rotationAmount = lookInput.x * rotationSpeed * Time.deltaTime;

            // Limit maximum rotation per frame to prevent flipping - RESTORED FROM OLD CONTROLLER
            float maxRotationPerFrame = 10f; // degrees per frame
            rotationAmount = Mathf.Clamp(rotationAmount, -maxRotationPerFrame, maxRotationPerFrame);

            // Apply rotation - RESTORED FROM OLD CONTROLLER
            transform.Rotate(0, rotationAmount, 0);
        }
    }

    private void HandleMovement()
    {
        // If attacking, limit movement speed
        float currentSpeed = isAttacking ? attackMoveSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        // Calculate movement direction relative to character's forward
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);

        // Convert to world space relative to character's rotation
        movement = transform.TransformDirection(movement);

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
                if(currentAnimState != 9 && isInLandingState)
                {
                    isInLandingState = false;
                    Debug.Log("BEEP BEEP BEEP!!!");
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
        Debug.Log("Jump performed");

        // Can't jump while attacking
        if (isAttacking) return;

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

    // Attack input handler
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {

        // Can't attack while jumping/falling
        if (!isGrounded || isJumping || isInLandingState)
        {
            return;
        }

        // Check if we can accept attack input (cooldown)
        if (!canAcceptAttackInput)
        {
            return;
        }

        // If we're already attacking, queue the next attack (unless we're on the final attack)
        if (isAttacking)
        {
            // Don't allow queuing if we're on the 4th attack (final attack in combo)
            if (currentAttackCombo >= 4)
            {
                // Ignore input during the final attack
                return;
            }

            // Only allow one queued attack at a time
            if (!attackInputQueued)
            {
                attackInputQueued = true;
                // Start cooldown to prevent multiple queued inputs
                StartCoroutine(AttackInputCooldown());
            }
            return;
        }

        // Start a new attack
        StartAttack();
    }

    private IEnumerator AttackInputCooldown()
    {
        canAcceptAttackInput = false;
        yield return new WaitForSeconds(attackInputCooldown);
        canAcceptAttackInput = true;
    }

    private void HandleAttackInputCooldown()
    {
        // If we're not attacking and have a queued attack, execute it
        if (!isAttacking && attackInputQueued && canAcceptAttackInput)
        {
            attackInputQueued = false;
            StartAttack();
        }
    }

    private void StartAttack()
    {
        // Reset combo if too much time has passed since last attack
        if (Time.time - lastAttackTime > attackComboTimeout)
        {
            currentAttackCombo = 0;
        }

        // Increment combo counter
        currentAttackCombo++;
        if (currentAttackCombo > 4) currentAttackCombo = 1;

        // Set attack state
        isAttacking = true;
        lastAttackTime = Time.time;


        // Force the animator to evaluate immediately
        animator.Update(0f);

        // Set the appropriate attack animation
        int attackAnimState = 11 + currentAttackCombo;
        SetAnimationState(attackAnimState);

        // Force another update
        animator.Update(0f);

        // Clear the list of enemies hit for this attack
        enemiesHitThisAttack.Clear();
    }

    private string GetAttackAnimationName(int combo)
    {
        switch (combo)
        {
            case 1: return "Attack1"; // Replace with your actual animation names
            case 2: return "Attack2";
            case 3: return "Attack3";
            case 4: return "Attack4";
            default: return "Attack1";
        }
    }

    // Called from animation event when attack hitbox should activate
    public void EnableAttackHitbox()
    {
        if (swordHitbox != null)
        {
            swordHitbox.enabled = true;
        }
    }

    // Called from animation event when attack hitbox should deactivate
    public void DisableAttackHitbox()
    {
        if (swordHitbox != null)
        {
            swordHitbox.enabled = false;
        }
    }

    // Called from animation event when attack is in a state where it can be chained
    public void OnAttackChainPoint()
    {
        // Don't allow chaining from the 4th attack (final attack in combo)
        if (currentAttackCombo >= 4) return;

        // If there's a queued attack, start the next attack immediately
        if (attackInputQueued && canAcceptAttackInput)
        {
            attackInputQueued = false;
            StartNextAttack();
        }
    }

    private void StartNextAttack()
    {
        // Don't start a new attack if we've reached max combo
        if (currentAttackCombo >= 4) return;

        // Increment combo counter
        currentAttackCombo++;

        // Set the appropriate attack animation
        int attackAnimState = 11 + currentAttackCombo; // 12, 13, 14, 15
        SetAnimationState(attackAnimState);

        // Reset the attack timer
        lastAttackTime = Time.time;

        // Clear the list of enemies hit for this attack
        enemiesHitThisAttack.Clear();
    }

    // Called from animation event when attack is complete
    public void OnAttackComplete()
    {
        isAttacking = false;

        // If we have a queued attack and we're still within combo timeout, execute it
        // But don't execute if we just finished the 4th attack
        if (attackInputQueued && Time.time - lastAttackTime < attackComboTimeout &&
            canAcceptAttackInput && currentAttackCombo < 4)
        {
            attackInputQueued = false;
            StartAttack();
        }
        else
        {
            // Return to idle or movement state
            UpdateMovementAnimation();

            // Reset combo after the final attack completes
            if (currentAttackCombo >= 4)
            {
                currentAttackCombo = 0;
            }
        }

    }

    // Handle detecting when attack hits an enemy
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by {other.gameObject.name}");
        // Only process hits when we're attacking and the hitbox is active
        if (!isAttacking || swordHitbox == null || !swordHitbox.enabled) return;

        // Check if the collider is on the enemy layer
        if (((1 << other.gameObject.layer) & enemyLayerMask) != 0)
        {
            // Make sure we haven't already hit this enemy in this attack
            if (!enemiesHitThisAttack.Contains(other.gameObject))
            {
                enemiesHitThisAttack.Add(other.gameObject);

                // Apply damage to the enemy
                ApplyDamageToEnemy(other.gameObject);
            }
        }
    }

    private void ApplyDamageToEnemy(GameObject enemy)
    {
        // Try to get the Enemy component and apply damage
        CreatureBase enemyComponent = enemy.GetComponent<CreatureBase>();
        if (enemyComponent != null)
        {
            if (enemyComponent.isPlayerCreature == false)
            {
                ElementType element = ElementType.None;
                enemyComponent.TakeDamage(attackDamage, element);
                Debug.Log($"Hit {enemy.name} for {attackDamage} damage!");
            }
        }
    }

    private void HandleAttackComboTimeout()
    {
        // Reset combo if too much time has passed
        if (currentAttackCombo > 0 && Time.time - lastAttackTime > attackComboTimeout)
        {
            currentAttackCombo = 0;
            attackInputQueued = false;
            OnAttackComplete();
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
        // If we're attacking, don't update movement animations
        // The attack animation is handled in StartAttack() and StartNextAttack()
        if (isAttacking)
            return;

        // Don't update movement animations if we're jumping or landing
        if (isJumping || isInLandingState)
            return;

        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        int newAnimState = 0; // Default to idle

        if (moveInput.magnitude > 0.1f)
        {
            if (isSprinting && moveInput.y > 0.1f)
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
                        newAnimState = 0; // Idle
                }
                else
                {
                    // Left/right movement is dominant
                    if (rightAmount > 0.3f)
                        newAnimState = 4; // Right
                    else if (rightAmount < -0.3f)
                        newAnimState = 3; // Left
                    else
                        newAnimState = 0; // Idle
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
            Invoke(nameof(ReturnToIdle), 0.2f);
            
        }
    }

    private void ReturnToIdle()
    {
        if (isGrounded && currentAnimState == 9)
        {
            isInLandingState = false;
            SetAnimationState(0); // Idle
        }
    }

    public void TakeDamage(float damage, ElementType elementType = ElementType.None)
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        // Trigger damage event
        OnDamageTaken?.Invoke(damage);

        Debug.Log($"Player took {damage} damage! Health: {health}");

        // Check for death
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player died!");

        // Disable movement and input
        characterController.enabled = false;
        //playerInput.enabled = false;
       
        // Notify Respawn Manager
        if (PlayerRespawnManager.Instance != null)
        {
            PlayerRespawnManager.Instance.HandlePlayerDeath(this);
            health = maxHealth;
        }
        else
        {
            // Fallback if no manager exists
            SceneManager.LoadScene("Hub");
            health = maxHealth;
        }

        EnemyTrialManager[] trials = FindObjectsOfType<EnemyTrialManager>();
        foreach (EnemyTrialManager trial in trials)
        {
            trial.StopTrialMusic();
            trial.ResetTrial();
        }
    }

    private void GameOver()
    {
        // Implement your game over logic here
        Debug.Log("Game Over!");
    }

    public void Heal(float amount)
    {
        health = Mathf.Clamp(health + amount, 0, maxHealth);
    }

    public void FullHeal()
    {
        health = maxHealth;
        isDead = false;
    }

    // Public methods for external access
    public bool IsSprinting => isSprinting;
    public bool IsGrounded => isGrounded;
    public int CurrentJumps => currentJumps;
    public bool IsAttacking => isAttacking;
    public int CurrentAttackCombo => currentAttackCombo;

    // This method will be called after a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HotbarUIManager.Instance.RefreshHotbar();
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "LevelThree")
        {
            maxHealth = 750f;
            health = maxHealth;
        }
        else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "LevelTwo")
        {
            maxHealth = 1000f;
            health = maxHealth;
        }

        GameObject spawn = GameObject.FindWithTag("LevelSpawn");
        if (spawn != null)
        {
            characterController.enabled = false; // Disable to set position
            transform.position = spawn.transform.position;
            transform.rotation = spawn.transform.rotation;
            characterController.enabled = true; // Re-enable after setting position
        }
        else if (spawn == null)
        {
            spawn = GameObject.FindWithTag("HubSpawn");
            if (spawn != null)
            {
                characterController.enabled = false; // Disable to set position
                transform.position = spawn.transform.position;
                transform.rotation = spawn.transform.rotation;
                characterController.enabled = true; // Re-enable after setting position
            }
        }
        OnAttackComplete();

        // Force reinstantiate the active creature to fix NavMeshAgent issues
        PlayerInventory.Instance.ForceReinstantiateActiveCreature();
    }

    public static bool IsInHub()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Hub";
    }
}