using System.Collections;
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
    public float playerFindDistance, aggroDistance;
    private Vector3 walkPoint;

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

        if (agent.remainingDistance <= agent.stoppingDistance) //done with path
        {
            Vector3 point;
            if (RandomNavmeshLocation(walkPointRange, out point)) //pass in our centre point and radius of area
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
                agent.SetDestination(point);
            }
        }

        //if the enemy is at the walkpoint, begin waiting
        Vector3 distanceToWalkpoint = transform.position - walkPoint;

        if (distanceToWalkpoint.magnitude < 3f)
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
            print("waiting");
        }
    }

    bool RandomNavmeshLocation(float radius, out Vector3 result)
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * radius; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
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

        if (Physics.CheckSphere(transform.position, aggroDistance, whatIsPlayer))
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
