using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshBrain : MonoBehaviour
{
    [Header("Roam (Random Walk)")]
    public float roamRadius = 12f;
    public float roamRepathTime = 2.5f;

    [Header("Chase (Player)")]
    public float chaseRepathTime = 0.25f;
    public float chaseRandomOffset = 1.2f;

    
    [Header("Animation")]
    public string movingParam = "Moving";
    public string attackTrigger = "Attack";

    private EnemyController ec;
    private NavMeshAgent agent;
    private Transform playerTr;
    private PlayerHealth playerHealth;

    private float roamTimer;
    private float chaseTimer;

    private Vector3 currentChaseOffset;

    
    private float attackTimer;

    private void Awake()
    {
        ec = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();

        
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void Start()
    {
        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            playerTr = pc.transform;
            playerHealth = pc.GetComponent<PlayerHealth>();
        }

       
        if (agent != null)
            agent.avoidancePriority = Random.Range(20, 80);

        
        if (agent != null)
        {
            agent.updatePosition = true;
            agent.updateRotation = true;

            
            if (ec != null)
                agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, ec.attackRange);
        }

        
        attackTimer = 0f;

        
        PickNewRoamPoint();
    }

    private void Update()
    {
        if (ec == null || agent == null) return;
        if (playerTr == null || playerHealth == null) return;

        
        if (!ec.useNavMeshMovement) return;

       
        if (playerHealth.isDead)
        {
            agent.isStopped = true;
            SetMoving(false);
            return;
        }

        
        if (ec.IsDead)
        {
            agent.isStopped = true;
            SetMoving(false);
            return;
        }

        float dist = Vector3.Distance(transform.position, playerTr.position);

       
        if (dist <= ec.attackRange)
        {
            agent.isStopped = true;

            
            Vector3 look = playerTr.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 10f);

            SetMoving(false);

            
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                if (ec.anim != null)
                    ec.anim.SetTrigger(attackTrigger);

                attackTimer = ec.attackCooldown;
            }

            return;
        }

       
        if (dist <= ec.chaseRange)
        {
            agent.isStopped = false;

            chaseTimer -= Time.deltaTime;
            if (chaseTimer <= 0f)
            {
                chaseTimer = chaseRepathTime;

                
                currentChaseOffset = Random.insideUnitSphere * chaseRandomOffset;
                currentChaseOffset.y = 0f;

                Vector3 target = playerTr.position + currentChaseOffset;

                if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
                else
                    agent.SetDestination(playerTr.position);
            }

            
            SetMoving(ShouldShowMoving());

            return;
        }

      
        agent.isStopped = false;

        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f || ReachedDestination() || IsStuck())
        {
            PickNewRoamPoint();
        }

        SetMoving(ShouldShowMoving());
    }

    private void PickNewRoamPoint()
    {
        roamTimer = roamRepathTime;

        Vector3 random = Random.insideUnitSphere * roamRadius;
        random.y = 0f;

        Vector3 origin = transform.position + random;

        if (NavMesh.SamplePosition(origin, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private bool ReachedDestination()
    {
        if (!agent.hasPath) return true;
        if (agent.pathPending) return false;
        return agent.remainingDistance <= agent.stoppingDistance + 0.2f;
    }

    private bool IsStuck()
    {
      
        if (!agent.hasPath) return false;
        if (agent.pathPending) return false;

        if (agent.remainingDistance > agent.stoppingDistance + 0.5f && agent.velocity.sqrMagnitude < 0.02f)
            return true;

        return false;
    }

   
    private void SetMoving(bool on)
    {
        if (ec.anim != null)
            ec.anim.SetBool(movingParam, on);
    }

    
    private bool ShouldShowMoving()
    {
        if (agent.isStopped) return false;
        if (agent.pathPending) return false;

        
        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance + 0.25f)
            return true;

        
        if (agent.velocity.sqrMagnitude > 0.05f)
            return true;

        return false;
    }
}
