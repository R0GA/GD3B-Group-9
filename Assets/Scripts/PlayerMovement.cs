using System.Collections;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float lookSpeed;
    public float gravity = -9.81f;
    public float jumpHeight = 1.0f;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth;
    public GameObject gameOverScreen;

    [Header("Weapon Settings")]
    [SerializeField] private float weaponHitRadius;
    [SerializeField] private Transform weaponHitPoint;
    [SerializeField] private int damage = 2;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Animator animator;

    private float currentHealth;
    public HealthManager healthManager;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private Vector2 _velocity;
    private CharacterController _characterController;

    public Rigidbody rb;

    public bool canDodge = true;
    public bool isPaused = false;

    public void Start()
    {
        currentHealth = maxHealth;

        healthManager.SetSlider(maxHealth);
    }
    private void OnEnable()
    {
        var playerInput = new Controls();

        playerInput.Player.Enable();

        playerInput.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        playerInput.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        playerInput.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        playerInput.Player.Look.canceled += ctx => _lookInput = Vector2.zero;

        playerInput.Player.Jump.performed += ctx => Jump();

        playerInput.Player.Dash.performed += ctx => Dash(); //Dodge

        playerInput.Player.Attack.performed += ctx => Attack();
    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public void Update()
    {
        
        Movement();
        Looking();
        
        ApplyGravity();

        if(currentHealth <= 0)
        {
            gameOverScreen.SetActive(true);
        }

        

    }

    public void Movement()
    {
        if (isPaused == false)
        {
            // Create a movement vector based on the input
            Vector3 move = new Vector3(-_moveInput.x, 0, -_moveInput.y);

            // Transform direction from local to world space
            move = transform.TransformDirection(move);

            

            // Move the character controller based on the movement vector and speed
            _characterController.Move(move * moveSpeed *  Time.deltaTime);
        }
    }

    public void Looking()
    {
        if (isPaused == false)
        {
            // Only use horizontal input (left/right)
            float lookX = _lookInput.x * lookSpeed;

            // Rotate the player left/right around the Y-axis
            transform.Rotate(0f, lookX, 0f);
        }
    }

    public void ApplyGravity()
    {
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -0.5f; // Small value to keep the player grounded
        }

        _velocity.y += gravity * Time.deltaTime; // Apply gravity to the velocity
        _characterController.Move(_velocity * Time.deltaTime); // Apply the velocity to the character
    }

    public void Jump()
    {
        if (_characterController.isGrounded && isPaused == false) { }
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Dash()
    {
        if(canDodge == true)
        {
            StartCoroutine(TheDodge());
        }
    }

    public void Attack()
    {
        animator.SetTrigger("attack");

        Collider[] hit = Physics.OverlapSphere(weaponHitPoint.position, weaponHitRadius, targetLayer);
        if (hit.Length > 0)
        {
            hit[0].GetComponent<EnemyHealth>().TakeDamage(damage);
            Debug.Log("Enemy Damaged");

        }
    }

    private IEnumerator TheDodge()
    {
        yield return new WaitForSeconds(0f);
        canDodge = false;
        moveSpeed = moveSpeed + 8f;
        yield return new WaitForSeconds(0.3f);
        moveSpeed = moveSpeed - 8f;
        yield return new WaitForSeconds(2f);
        canDodge = true;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        healthManager.SetSlider(currentHealth);
    }
}
