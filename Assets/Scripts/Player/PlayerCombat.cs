using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private PlayerState playerState;
    private Animator anim;
    private ThirdPersonMovement tpm;
    private PlayerInput playerInput;
    private bool attacking;


    private void Start()
    {
        anim = GetComponent<Animator>();
        tpm = GetComponent<ThirdPersonMovement>();
        playerInput = GetComponent<PlayerInput>();
    }

    void OnAttack(InputValue inputValue)
    {
        //conditions to do light melee attack
        if (!tpm.isJumping && !attacking && !tpm.isFalling)
        {
            LightAttack();
        }
        //conditions to do heavy slam attack
        else if (tpm.isJumping && !attacking && !tpm.isGrounded)
        {
            SlamAttack();
        }
    }

    void LightAttack()
    {        
        attacking = true;
        ThirdPersonMovement.playerState = PlayerState.attacking;

        //selects a random animation to do
        int index = Random.Range(0, 3);
        switch (index)
        {
            case 0:
                anim.SetInteger("LightAttack", 1);
                return;
            case 1:
                anim.SetInteger("LightAttack", 2);
                return;
            case 2:
                anim.SetInteger("LightAttack", 3);
                return;
        }
    }

    void SlamAttack()
    {
        ThirdPersonMovement.playerState = PlayerState.attacking;
        attacking = true;

        anim.SetBool("SlamAttack", true);
    }
    void ExecuteAttack()
    {

    }
    void EndAttackAnim()
    {
        anim.SetInteger("LightAttack", 0);
        anim.SetBool("SlamAttack", false);
        anim.SetBool("Jump", false);
        
        attacking = false;
        ThirdPersonMovement.playerState = PlayerState.moving;
    }
}
