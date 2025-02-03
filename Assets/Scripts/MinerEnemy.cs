using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;


public class MinerEnemy : MonoBehaviour
{
    private enum State
    {
        patrolling,
        waiting,
        chasing,
        attacking,
        dying
    }

    //references
    private Animator anim;
    private Rigidbody rb;
    private State state;
    private GameObject player;


    [Header("Attacking")]    
    private float attackDistance = 5;
    public float attackDamage;
    private bool canAttack = true;
    public float attackCooldown = 1.5f;
    


    [Header("AI")]
    private NavMeshAgent agent;
    public float patrolSpeed, chaseSpeed;
    private bool walkPointSet;
    public float walkPointRange;
    public float minWaitTime, maxWaitTime;
    private Vector3 walkPoint;

    public float playerFindDistance, hearRange;

    public LayerMask whatIsGround, whatIsPlayer;

    //animation
    Vector3 lastPosition;
    private float currentSpeed;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");

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

        //if the player is within attack range, attack them
        if (Physics.CheckSphere(transform.position, attackDistance, whatIsPlayer))
        {
            state = State.attacking;            
        }
        
        switch (state)
        {
            case State.patrolling:
                Patrolling();
                return;

            case State.chasing:
                Chasing();
                return;

            case State.attacking:
                Attacking();
                return;
        }

    }    

    void Patrolling()
    {
        agent.speed = patrolSpeed;
        
        if (!walkPointSet)
        {
            agent.SetDestination(RandomNavmeshLocation(walkPointRange));
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

    
    public Vector3 RandomNavmeshLocation(float radius)
    {
        //generates random position and adds current position on to it
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;

        //if the point is on the navmesh, set it to the walkpoint
        //if not, find the closest point on the navmesh
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        walkPointSet = true;
        walkPoint = finalPosition;
        return finalPosition;
    }

    void LookForPlayer()
    {
        Vector3 offset = new Vector3(0, 1, 0);
        RaycastHit hit;

        //if the player is within range, or is directly in front of the ai, it will begin to chase the player
        if (Physics.Raycast(transform.position + offset, this.transform.forward, out hit, playerFindDistance, whatIsPlayer))
        {           
            state = State.chasing;            
        }

        if (Physics.CheckSphere(transform.position, hearRange, whatIsPlayer))
        {            
            state = State.chasing;   
        }
    }

    void Chasing() //chases the player until it reaches them
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.transform.position);
    }

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

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hearRange);
    }
}
