using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    //references
    private PlayerState playerState;
    private Animator anim;
    private ThirdPersonMovement tpm;
    private PlayerInput playerInput;
    private bool attacking;

    [Header("Health")]
    public float health, maxHealth;   
    private Healthbar hb;
    private WaveManager waveManager;

    [Header("Damage and Range")]
    public float lightDmg, heavyDmg;
    public float lightAttackRange, heavyAttackRange;
    public float slamJumpHeight;
    public Transform lightAttackPoint;
    public Transform heavyAttackPoint;
    public LayerMask whatIsEnemy;
    public ParticleSystem slam;

    [Header("Cooldowns")]
    public Slider lightCD;
    public Slider heavyCD;
    public float lightAttackCooldown, heavyAttackCooldown;
    private float lightCurrent, heavyCurrent;
    private bool canLightAttack, canHeavyAttack;
    public float deathAnimTime;


    private void Start()
    {
        anim = GetComponent<Animator>();
        tpm = GetComponent<ThirdPersonMovement>();
        playerInput = GetComponent<PlayerInput>();
        
        hb = GameObject.Find("PlayerHealthBar").GetComponent<Healthbar>();
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();
        hb.health = maxHealth;
        health = maxHealth;
      
        canLightAttack = true;
        canHeavyAttack = true;
    }

    private void Update()
    {
        if (!canLightAttack)
        {
            LightCooldown();
        }
        if (!canHeavyAttack)
        {
            HeavyCooldown();
        }

    }

    void OnAttack()
    {
        //conditions to do light melee attack
        if (!tpm.isJumping && !attacking && !tpm.isFalling && canLightAttack)
        {
            LightAttack();
        }        
    }

    void OnSlamAttack()
    {
        print("slam");
        //conditions
        if (!tpm.isGrounded || tpm.isJumping || attacking || ThirdPersonMovement.playerState != PlayerState.moving || !canHeavyAttack)
        {
            return;
        }
        tpm.DoJump(slamJumpHeight);
        anim.SetBool("Jump", true);
        ThirdPersonMovement.playerState = PlayerState.attacking;
        attacking = true;

        canHeavyAttack = false;
        heavyCurrent = 0;  //cooldown time
    }

    void LightAttack()
    {  
        if (ThirdPersonMovement.playerState != PlayerState.moving)
        {
            return;
        }
      
        attacking = true;
        ThirdPersonMovement.playerState = PlayerState.attacking;

        //selects a random animation to do
        int index = Random.Range(0, 3);
        switch (index)
        {
            case 0:
                anim.SetInteger("LightAttack", 1);
                break;
            case 1:
                anim.SetInteger("LightAttack", 2);
                break;
            case 2:
                anim.SetInteger("LightAttack", 3);
                break;
        }

        canLightAttack = false;
        lightCurrent = 0; //cooldown
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
            Vector3 dir = new Vector3(hit.transform.position.x, 0, hit.transform.position.z); 
        }
        Instantiate(slam, transform.position, Quaternion.identity);
    }

    void LightCooldown()
    {
        lightCurrent = Mathf.Clamp(lightCurrent + Time.deltaTime, 0, lightAttackCooldown);
        if (lightCurrent == lightAttackCooldown)
        {
            canLightAttack = true;
        }

        lightCD.value = lightCurrent / lightAttackCooldown; //ui slider
    }

    void HeavyCooldown()
    {
        heavyCurrent = Mathf.Clamp(heavyCurrent + Time.deltaTime, 0, heavyAttackCooldown);
        if (heavyCurrent == heavyAttackCooldown)
        {
            canHeavyAttack = true;
        }

        heavyCD.value = heavyCurrent / heavyAttackCooldown;
    }

    void EndAttackAnim()
    {      
        anim.SetInteger("LightAttack", 0);                      
        attacking = false;
        ThirdPersonMovement.playerState = PlayerState.moving;
    }

    public void TakeDamage(float damage)
    {
        health = Mathf.Clamp(health - damage, 0, maxHealth);  //stops health exceeding max health or going below zero 
        
        //changes healthbar
        hb.health = health;
        //if health reaches zero, start the game over process
        if (health <= 0)
        {
            GameOver();
        }
        else
        {
            //play one of the hit animations
            int index = Random.Range(0, 2);
            switch (index)
            {
                case 0:
                    anim.SetInteger("Hit", 1);
                    break;
                case 1:
                    anim.SetInteger("Hit", 2);
                    break;
            }
        }
    }

    public void GainHealth(float hp)
    {
        health = Mathf.Clamp(health + hp, 0, maxHealth);
    }

    void EndHitAnim()
    {
        anim.SetInteger("Hit", 0); //stops the anim repeating
    }

    void GameOver()
    {      
        ThirdPersonMovement.playerState = PlayerState.dying;
        anim.SetInteger("LightAttack", 0); //incase you die whilst attacking
        anim.SetTrigger("Dying");     
    }

    void DisplayGameOver()
    {      
        waveManager.DisplayGameOver();
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lightAttackPoint.position, lightAttackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(heavyAttackPoint.position, heavyAttackRange);
    }
}
