using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;


public class MinerEnemy : MonoBehaviour
{
    private enum State
    {
        patrolling,
        chasing,
        attacking,
        dying
    }

    private Animator anim;
    private Rigidbody rb;
    private State state;
    private GameObject player;


    [Header("Attacking")]
    private int attackNumber;
    private float attackDistance = 5;    
    public float lightAttackDistance, heavyAttackDistance;
    public float lightAttackDamage, heavyAttackDamage;
    public float heavyChance;


    [Header("AI")]
    private NavMeshAgent agent;
    public float patrolSpeed, chaseSpeed;
    private bool walkPointSet;
    public float walkPointRange;
    private Vector3 walkPoint;

    public float playerFindDistance, hearRange;

    public LayerMask whatIsGround, whatIsPlayer;

    Vector3 lastPosition;
    float currentSpeed;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");

        lastPosition = transform.position;

        ChoseAttack();
    }

    // Update is called once per frame
    void Update()
    {
        StateFinder();
        Animation();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SearchWalkPoint();
        }
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
        if (state == State.patrolling)
        {
            //print("Patrolling");
            Patrolling();
            LookForPlayer();
        }
        if (state == State.chasing)
        {
            //print("CHasing");
            Chasing();
            LookForPlayer();
        }
        
    }

    void ChoseAttack()
    {
        int index = Random.Range(0, 100);
        if (index < heavyChance)
        {
            attackNumber = 1;
            attackDistance = heavyAttackDistance;
        }
        else
        {
            attackNumber = 0;
            attackDistance = lightAttackDistance;
        }
    }

    void Patrolling()
    {
        agent.speed = patrolSpeed;
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }
        else
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkpoint = transform.position - walkPoint;

        if (distanceToWalkpoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }
    
    void SearchWalkPoint()
    {
        //calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }        

    }

    void LookForPlayer()
    {
        Vector3 offset = new Vector3(0, 1, 0);
        RaycastHit hit;

        //Debug.DrawRay(transform.position + offset, this.transform.forward, Color.blue);
        if (Physics.Raycast(transform.position + offset, this.transform.forward, out hit, playerFindDistance, whatIsPlayer))
        {           
            state = State.chasing;
            walkPoint = hit.transform.position;
        }

        if (Physics.CheckSphere(transform.position, hearRange, whatIsPlayer))
        {            
            state = State.chasing;
            walkPoint = player.transform.position;
        }
    }

    void Chasing()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(walkPoint);

        if (Physics.CheckSphere(transform.position, attackDistance, whatIsPlayer))
        {
            state = State.attacking;
            AttackPlayer(attackNumber);
            //print("attack");

        }

    }

    void AttackPlayer(int attackNo)
    {
        //print("ATTACK");
        agent.SetDestination(transform.position);        

        if (attackNo == 0)
        {
            LightAttack();
        }
        else if (attackNo == 1)
        {
            HeavyAttack();
        }
    }

    void LightAttack()
    {
        anim.SetInteger("Attacking", 1);
        
    }

    void HeavyAttack()
    {
        agent.enabled = false;
        rb.isKinematic = false;
        rb.AddForce(Vector3.up * 100);
        anim.SetInteger("Attacking", 2);
        
    }

    void EndAttack()
    {
        state = State.patrolling;
        anim.SetInteger("Attacking", 0);
        SearchWalkPoint();
        ChoseAttack();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            agent.enabled = true;
            rb.isKinematic = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hearRange);
    }
}
