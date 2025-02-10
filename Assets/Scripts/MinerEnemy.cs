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
    public int level;
    public float health, maxHealth;    
    private Healthbar hb;

    public float attackDamage;
    private bool canAttack = true;
    public float attackCooldown = 1.5f;
    public Transform attackPosition;
    public float attackHitRange = 1.5f;

    private WaveManager waveManager;


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
        hb = GetComponentInChildren<Healthbar>();

        player = GameObject.Find("Player");
        waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();

        //level is determined at the start of each wave
        level = WaveManager.enemyLevel;
        attackDamage += (level * 5) - 5; //+5 damage for each level
        maxHealth += (level * 10) - 10; //+10 health for each level

        //this enemy's healthbar settings/variables
        hb.health = maxHealth;
        hb.maxHealth = maxHealth;

        health = maxHealth;

        //used for calculating velocity for blend tree
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
        //manually finds the velocity as the nav mesh velocity wasn't working properly
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
            if (RandomNavmeshLocation(walkPointRange, out point) && agent != null)
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

        //im aware that both if statements do basically the same thing but merging it didnt work and im not bothered to do it
        //"if it aint broken dont fix it"
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

    bool RandomNavmeshLocation(float radius, out Vector3 result)
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * radius; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            //if the random point is on or close enough to the mesh, the position is returned and destination is set
            //if not, the destinatin isn't set and it will try again next frame
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }


    void LookForPlayer()
    {
        Vector3 offset1 = new Vector3(0.5f, 1, 0);
        Vector3 offset2 = new Vector3(-0.5f, 1, 0);
        Vector3 offset3 = new Vector3(0.5f, 1, 0);
        RaycastHit hit;

        
        //three raycasts are done at differnet ofsets so if the player is generally in front of the enemy, it should give chase
        if (Physics.Raycast(transform.position + offset1, this.transform.forward, out hit, playerFindDistance, whatIsPlayer))
        {
            state = State.chasing;
        }
        if (Physics.Raycast(transform.position + offset2, this.transform.forward, out hit, playerFindDistance, whatIsPlayer))
        {
            state = State.chasing;
        }
        if (Physics.Raycast(transform.position + offset3, this.transform.forward, out hit, playerFindDistance, whatIsPlayer))
        {
            state = State.chasing;
        }

        //also if the player is too close
        if (Physics.CheckSphere(transform.position, aggroDistance, whatIsPlayer))
        {
            state = State.chasing;
        }
    }

    void Chasing() //chases the player until it reaches them
    {
        agent.speed = chaseSpeed;

        if (agent != null)
        {
            agent.SetDestination(player.transform.position);
        }

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
        if (agent != null)
        {
            agent.SetDestination(transform.position);
        }

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
            anim.SetInteger("Attacking", 0); //if hit during the atack animation, it would try and attack straight after

            state = State.hit; //state used to prevent the chasing method from attacking again
            Invoke("EndHitAnim", 1);

        }
        
    }

    void EndHitAnim() //called at end of hit anim to allow chasing and attacking
    {
        state = State.chasing;
        canAttack = true;
    }

    void EnemyDies()
    {
        state = State.dying;       

        int index = Random.Range(0, 2);
      
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
        WaveManager.enemiesLeft--;
        waveManager.UpdateUI();
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
