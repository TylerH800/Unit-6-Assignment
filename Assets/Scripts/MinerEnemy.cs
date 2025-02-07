using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.AI;


public class MinerEnemy : MonoBehaviour
{
    private enum State
    {
        patrolling,
        waiting,
        chasing,
        attacking,
        hit,
        dying
    }

    //references
    private Animator anim;
    private Rigidbody rb;
    private BoxCollider bc;
    private State state;
    private GameObject player;


    [Header("Combat")]
    public float health, maxHealth;
    public GameObject healthBarGO;
    private Healthbar hb;

    public float attackDamage;
    private bool canAttack = true;
    public float attackCooldown = 1.5f;
    public Transform attackPosition;
    public float attackHitRange = 1.5f;


    [Header("AI")]
    private NavMeshAgent agent;
    public float patrolSpeed, chaseSpeed;
    private bool walkPointSet;
    public float walkPointRange;
    public float minWaitTime, maxWaitTime;
    private Vector3 walkPoint;

    public float playerFindDistance;

    public LayerMask whatIsGround, whatIsPlayer;

    //animation
    Vector3 lastPosition;
    private float currentSpeed;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");

        hb = healthBarGO.GetComponent<Healthbar>();
        hb.health = maxHealth;
        hb.maxHealth = maxHealth;

        health = maxHealth;
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        StateFinder();
        Animation();
    }

    void Animation()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, (transform.position - lastPosition).magnitude / Time.deltaTime, 0.75f);
        lastPosition = transform.position;

        //print(currentSpeed);        
        anim.SetFloat("Velocity", currentSpeed);
    }

    void StateFinder()
    {
        //if not attacking or dying, check for the player
        if (state != State.attacking && state != State.dying)
        {
            LookForPlayer();
        }

        switch (state)
        {
            case State.patrolling:
                Patrolling();
                break;

            case State.chasing:
                Chasing();
                break;

            case State.attacking:
                Attacking();
                break;
        }

    }

    #region Movement and player finding
    void Patrolling()
    {
        agent.speed = patrolSpeed;

        if (!walkPointSet)
        {
            
            if (RandomNavmeshLocation(walkPointRange)) //returns true if a point on the navmesh is found
            {
                agent.SetDestination(walkPoint);
            }
            else
            {
                return; //prevents waiting from happening over and over if a new walkpoint isnt set
            }                
        }

        //if the enemy is at the walkpoint, begin waiting
        Vector3 distanceToWalkpoint = transform.position - walkPoint;

        if (distanceToWalkpoint.magnitude < 1f)
        {
            state = State.waiting;
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            StartCoroutine(Wait(waitTime));
        }
    }

    IEnumerator Wait(float seconds) //used when the ai gets to its walk point to give it a short stand still
    {
        yield return new WaitForSeconds(seconds);
        if (state == State.waiting)
        {
            walkPointSet = false;
            state = State.patrolling;
        }
    }

    bool RandomNavmeshLocation(float radius)
    {
        //generates random position and adds current position on to it
        Vector3 randomPosition = Random.insideUnitSphere * radius;
        randomPosition += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPosition, out hit, 1, NavMesh.AllAreas))
        {
            //if the random point is on the nav mesh, the walk point is set to it
            walkPoint = hit.position;
            walkPointSet = true;
            
            return true;
        }        
        return false;   

        //this whole walk point system could probably be done in less code but it works well for my project
    }

    void LookForPlayer()
    {
        Vector3 offset = new Vector3(0, 1, 0);
        RaycastHit hit;

        //if the player is directly in front of the ai, it will begin to chase the player (also if it gets hit)
        if (Physics.Raycast(transform.position + offset, this.transform.forward, out hit, playerFindDistance, whatIsPlayer))
        {
            state = State.chasing;
        }
    }

    void Chasing() //chases the player until it reaches them
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.transform.position);

        //if the player is within attack range, attack them
        if (Physics.CheckSphere(transform.position, attackHitRange - 0.2f, whatIsPlayer))
        {
            state = State.attacking;
        }
    }

    #endregion

    #region Combat
    void Attacking() //method called to perform and loop attacks
    {
        agent.SetDestination(transform.position);

        if (canAttack) //if not currently attacking or in cooldown, start attack animation
        {
            anim.SetInteger("Attacking", 1);
            //integers are used as I had multiple attacks with multiple methods before,
            //so I used different integers for different attacks, similar to the death animations

            canAttack = false;
        }
    }

    void ExecuteAttack()
    {
        Collider[] hits = Physics.OverlapSphere(attackPosition.position, attackHitRange, whatIsPlayer);
        foreach (Collider hit in hits)
        {
            print(hit.gameObject.name);
            hit.gameObject.GetComponent<PlayerCombat>().TakeDamage(attackDamage);
        }
    }

    void EndAttack() //called via an event at the end of each attack animation
    {
        //stops attack anim, starts cooldown and sets ai back to chasing
        anim.SetInteger("Attacking", 0);
        StartCoroutine(AttackCooldown(attackCooldown));

        state = State.chasing;
    }

    IEnumerator AttackCooldown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        canAttack = true;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        hb.health = health;
        if (health <= 0)
        {
            EnemyDies();
        }
        else
        {
            anim.SetTrigger("Hit");
            anim.SetInteger("Attacking", 0);

            state = State.hit; //state used to prevent the chasing method from attacking again
            print(state);

        }
    }

    void EndHitAnim() //called at end of hit anim to allow chasing and attacking
    {
        state = State.chasing;
    }

    void EnemyDies()
    {
        state = State.dying;
        bc.isTrigger = true; //used incase the player is on top of them
        int index = Random.Range(0, 2);
        print(index);
        if (index == 0)
        {
            anim.SetInteger("Dying", 1);
        }
        if (index == 1)
        {
            anim.SetInteger("Dying", 2);
        }
    }

    void Despawn() //called at end of death anims
    {
        Destroy(gameObject);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPosition.position, attackHitRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPosition.position, attackHitRange - 0.2f);
    }
}
