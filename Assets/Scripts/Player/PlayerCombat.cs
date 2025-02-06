using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private PlayerState playerState;
    private Animator anim;
    private ThirdPersonMovement tpm;
    private PlayerInput playerInput;
    private bool attacking;

    public float health, maxHealth;
    public GameObject healthBarGO;
    private Healthbar hb;
    public float lightDmg, heavyDmg;
    public float lightAttackRange, heavyAttackRange;
    public float slamJumpHeight;
    public Transform lightAttackPoint;
    public Transform heavyAttackPoint;
    public LayerMask whatIsEnemy;
    public float deathAnimTime;


    private void Start()
    {
        anim = GetComponent<Animator>();
        tpm = GetComponent<ThirdPersonMovement>();
        playerInput = GetComponent<PlayerInput>();
        
        hb = healthBarGO.GetComponent<Healthbar>();
        hb.health = maxHealth;
        health = maxHealth;
    }

    void OnAttack()
    {
        //conditions to do light melee attack
        if (!tpm.isJumping && !attacking && !tpm.isFalling)
        {
            LightAttack();
        }        
    }

    void OnSlamAttack()
    {
        print("slam");
        if (!tpm.isGrounded || tpm.isJumping || attacking)
        {
            return;
        }
        tpm.DoJump(slamJumpHeight);
        anim.SetBool("Jump", true);
        ThirdPersonMovement.playerState = PlayerState.attacking;
        attacking = true;
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

    void ExecuteLightAttack()
    {        
        Collider[] hits = Physics.OverlapSphere(lightAttackPoint.position, lightAttackRange, whatIsEnemy);
        foreach (Collider hit in hits)
        {
            print(hit.gameObject.name);
            hit.gameObject.GetComponent<MinerEnemy>().TakeDamage(lightDmg);
        }
    }    

    void ExecuteHeavyAttack()
    {
        Collider[] hits = Physics.OverlapSphere(heavyAttackPoint.position, heavyAttackRange, whatIsEnemy);
        foreach (Collider hit in hits)
        {
            hit.gameObject.GetComponent<MinerEnemy>().TakeDamage(heavyDmg);

        }
    }
    
    void EndAttackAnim()
    {
        anim.SetInteger("LightAttack", 0);        
        //anim.SetBool("Jump", false);
        
        attacking = false;
        ThirdPersonMovement.playerState = PlayerState.moving;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;      
        
        //changes healthbar
        hb.health = health;
        //if health reaches zero, start the game over process
        if (health <= 0)
        {
            StartCoroutine(GameOver());
        }
        else
        {
            //play one of the hit animations
            int index = Random.Range(0, 2);
            switch (index)
            {
                case 0:
                    anim.SetInteger("Hit", 1);
                    return;
                case 1:
                    anim.SetInteger("Hit", 2);
                    return;
            }
        }
    }

    IEnumerator GameOver()
    {
        anim.SetTrigger("Dying");
        yield return new WaitForSeconds(deathAnimTime);
        //display game over
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lightAttackPoint.position, lightAttackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(heavyAttackPoint.position, heavyAttackRange);
    }
}
