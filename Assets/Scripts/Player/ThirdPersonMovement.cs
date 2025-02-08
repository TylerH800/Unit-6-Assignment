using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    moving, //default
    jumping,
    falling,
    attacking,
    dying
}

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Movement and Camera")]

    public static PlayerState playerState = PlayerState.moving;

    public float speed = 6;
    public float acceleration = 8;
    public float sprintSpeed = 6;    
    private float targetSpeed;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    private bool sprinting;

    private Animator animator;
    private CharacterController cc;
    public Transform cam;

    [Header("Jumping and Gravity")]

    [HideInInspector] public bool isFalling, isJumping, isGrounded, isOnSlope;

    public float jumpHeight;
    public float gravity = -9.81f;
    Vector3 vertVelocity; //used for vertical movement
    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;
    public LayerMask slopeMask;

    private PlayerInput playerInput;
    private Vector2 input;


    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();

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
         animator.SetBool("Walk", false);
         animator.SetBool("Jump", true);     
         isJumping = true;
    }

    #endregion

    void Update()
    {
        VerticalMovement();

        if (playerState == PlayerState.moving || playerState == PlayerState.jumping)
        {            
            HorizontalMovement();
            GetSprint();
        }

        print(isGrounded);
    }


    #region movement
    void HorizontalMovement()
    {
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

        //when starting or finishing sprinting, the speed change is done smoothly. not neccesary but i prefer the feel of it
        targetSpeed = Mathf.MoveTowards(targetSpeed, sprinting ? sprintSpeed : speed, acceleration * Time.deltaTime);
        
        // If there's horizontal movement
        if (direction.magnitude >= 0.1f)
        {
            animator.SetBool("Walk", true);
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Calculate the movement direction
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            cc.Move(moveDir.normalized * targetSpeed * Time.deltaTime);

        }
        else
        {
            animator.SetBool("Walk", false);
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

    //called from an animation event, giving a delay for the character to prepare to jump
    //I did this as I wanted the rock golem to feel heavy, like in some other animations 
    public void DoJump(float height)
    {
        if (isGrounded)
        {       
            vertVelocity.y = Mathf.Sqrt(height * -2f * gravity);
        }
    }

    void VerticalMovement()
    {    
        // Ground check
        
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        isOnSlope = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance * 3, slopeMask);

        //gravity
        vertVelocity.y += gravity * Time.deltaTime;
        cc.Move(vertVelocity * Time.deltaTime);

        // Conditions for landing
        if (isGrounded && vertVelocity.y <= 0)
        {
            vertVelocity.y = -2f;
        }
     
        // Conditions for falling
        if (!isGrounded && vertVelocity.y < 5 && playerState != PlayerState.attacking && !isOnSlope)
        {            
            //if you are in the attacking state whilst jumping that means you are doing the slam attack,
            //so this prevents the fall animation from playing in that scenario
            animator.SetBool("Fall", true);
            isFalling = true;
        }
        //rather than falling, slam
        else if (!isGrounded && vertVelocity.y < 5 && playerState == PlayerState.attacking)
        {
            animator.SetBool("SlamAttack", true);
            gravity = -30f;
        }

        //conditions to stop the jumping bool
        //this cant be done when grounded as the jump is delayed
        if (!isGrounded && vertVelocity.y < 5)
        {
            animator.SetBool("Jump", false);
            isJumping = false;
        } 
        
        //conditions to stop the falling bool
        if (isGrounded)
        {
            gravity = -20f;
            animator.SetBool("Fall", false);           
            isFalling = false;
            animator.SetBool("SlamAttack", false);
        }

        //these animator bools are done in a more complicated way than I normally would,
        //but that was needed to for the slam attack to work consistently and as intended

    }
    #endregion


}

