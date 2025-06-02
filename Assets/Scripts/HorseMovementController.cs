
using UnityEngine;

public class HorseMovementController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    public float maxSpeed = 5f;
    public float acceleration = 5f;
    public float gravity = 9.81f;
    public float groundedCheckDistance = 0.1f;

    private Animator animator;
    private CharacterController controller;

    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float horizontalInput = 0f;
    private float verticalVelocity = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

    }

    void Update()
    {
        HandleInput();
        UpdateAnimator();
        HandleMovement();
    }

    
    void HandleInput()
    {
        float verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        targetSpeed = Mathf.Clamp(verticalInput, 0f, 1f) * maxSpeed;
    }

    void HandleMovement()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        Vector3 horizontalMove = transform.forward * currentSpeed;

        
        transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);

        // Gravity
        if (controller.isGrounded)
        {
            verticalVelocity = -1f; 
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

       
        Vector3 move = horizontalMove + Vector3.up * verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }

    void UpdateAnimator()
    {
        float normalizedSpeed = currentSpeed / maxSpeed;
        animator.SetFloat("Speed", normalizedSpeed);
        animator.SetFloat("Turn", horizontalInput);
    }
}
