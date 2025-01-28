using UnityEngine;
using UnityEngine.InputSystem;



public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Movement and Camera")]
    public float speed = 6;
    public float sprintSpeed = 6;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    public CharacterController characterController;
    public Transform cam;

    [Header("Jumping and Gravity")]

    private bool jumping, landing;

    public float jumpHeight;
    public float gravity = -9.81f;
    Vector3 velocity;
    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;
    bool isGrounded;

    private PlayerInput playerInput;
    private Vector2 input;

    public Animator animator;

    bool sprinting;

    private void Start()
    {        
        playerInput = GetComponent<PlayerInput>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #region Input
    void OnMove(InputValue inputValue)
    {
        input = inputValue.Get<Vector2>();
    }

    void OnJump(InputValue inputValue)
    {
        float jump = inputValue.Get<float>();
        print("A");
        print(jump);
        print(isGrounded);
        //conditions to jump
        if (jump != 0 && isGrounded)
        {
            print("b");
            animator.SetBool("Walk", false);
            animator.SetBool("Jump", true);
        }
    }

    void OnSprint(InputValue value, InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            print("sprinting");
        }
        else
        {
            print("not sprinting");
        }

    }

    #endregion
   

    void Update()
    {
        //groundcheck
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        HorizontalMovement();
        VerticalMovement();

       
    }

    
    void HorizontalMovement()
    {        
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
        
        //if there is horizontal movement
        if (direction.magnitude >= 0.1f)
        {
            animator.SetBool("Walk", true);

            //rotates based off of camera rotation
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            speed = sprinting ? sprintSpeed : speed;

            characterController.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("Walk", false);
        }
    }

    void VerticalMovement()
    {
        //conditions for landing
        if (isGrounded && velocity.y <= 0)
        {
            animator.SetBool("Land", true);
            animator.SetBool("Fall", false);
            velocity.y = -2f;

            jumping = false;
            landing = true;
        }

        //conditions for falling
        if (!isGrounded && velocity.y < 0)
        {
            animator.SetBool("Fall", true);
            animator.SetBool("Jump", false);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void DoJump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        jumping = true;
    }

    public void LandAnim()
    {
        landing = false;
    }


}
