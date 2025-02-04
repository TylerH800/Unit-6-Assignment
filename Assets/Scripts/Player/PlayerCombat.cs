using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{

    private Animator anim;
    private ThirdPersonMovement tpm;
    private PlayerInput playerInput;


    private void Start()
    {
        anim = GetComponent<Animator>();
        tpm = new ThirdPersonMovement();
        playerInput = GetComponent<PlayerInput>();

    }

    void OnAttack(InputValue inputValue)
    {
        LightAttack();
        print("attack");

    }
    void LightAttack()
    {
        if (!tpm.isGrounded)
        {
            return;
        }
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
    void ExecuteAttack()
    {

    }
    void EndAttackAnim()
    {
        anim.SetInteger("LightAttack", 0);
    }
}
