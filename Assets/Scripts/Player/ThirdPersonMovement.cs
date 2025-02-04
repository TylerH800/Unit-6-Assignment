using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Movement and Camera")]
    public float speed = 6;
    public float acceleration = 8;
    public float sprintSpeed = 6;
    public float slowSpeed = 2.5f;
    public float targetSpeed;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    private bool sprinting;

    public CharacterController characterController;
    public Transform cam;

    [Header("Jumping and Gravity")]

    private bool falling, landing, slowed;

    public float jumpHeight;
    public float gravity = -9.81f;
    Vector3 vertVelocity; //used for vertical movement
    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;
    public bool isGrounded;

    private PlayerInput playerInput;
    private Vector2 input;

    private Animator animator;
    private CharacterController cc;

    private float currentSpeed;
    private Vector3 lastPosition;



    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lastPosition = transform.position;
    }

    #region Input
    void OnMove(InputValue inputValue)
    {
        input = inputValue.Get<Vector2>();
    }

    void OnJump(InputValue inputValue)
    {
        float jump = inputValue.Get<float>();
        if (jump != 0 && isGrounded)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Jump", true);
        }
    }

    void GetSprint()
    {
        var sprint = playerInput.actions["Sprint"];
        if (sprint.IsPressed())
        {
            sprinting = true;
            animator.SetBool("Run", true);
        }
        else
        {
            sprinting = false;
            animator.SetBool("Run", false);
        }
    }

    

    #endregion

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        HorizontalMovement();
        VerticalMovement();

        GetSprint();


    }



    void HorizontalMovement()
    {
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

        if (animator.GetBool("Jump") || !isGrounded)
        {
            targetSpeed = Mathf.MoveTowards(targetSpeed, slowSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            targetSpeed = Mathf.MoveTowards(targetSpeed, sprinting ? sprintSpeed : speed, acceleration * Time.deltaTime);
        }

        // If there's horizontal movement
        if (direction.magnitude >= 0.1f)
        {
            animator.SetBool("Walk", true);
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Calculate the movement direction
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            characterController.Move(moveDir.normalized * targetSpeed * Time.deltaTime);

        }
        else
        {
            animator.SetBool("Walk", false);
        }


    }

    void VerticalMovement()
    {
        vertVelocity.y += gravity * Time.deltaTime;
        characterController.Move(vertVelocity * Time.deltaTime);

        // Conditions for landing
        if (isGrounded && vertVelocity.y <= 0)
        {
            vertVelocity.y = -2f;
        }

        // Conditions for falling
        if (!isGrounded && vertVelocity.y < 0)
        {
            print("fall");
            animator.SetBool("Jump", false);
            animator.SetBool("Fall", true);
        }
        if (isGrounded)
        {
            animator.SetBool("Fall", false);
        }

    }

    public void DoJump()
    {
        vertVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    


}
