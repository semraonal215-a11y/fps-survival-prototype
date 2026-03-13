using UnityEngine;
using System;

public class EnemyController : MonoBehaviour
{
    private PlayerController player;
    private PlayerHealth playerHealth;

    public static event Action<bool> OnEnemyDied;

    [Header("Enemy Type")]
    public bool isGeneral = false;

    private bool deathNotified;

    [Header("NavMesh Movement")]
    public bool useNavMeshMovement = true;

    public bool IsDead => isDead;

    [Header("Movement")]
    public float moveSpeed;
    public Rigidbody theRB;

    [Header("Ranges")]
    public float chaseRange = 15f;
    public float stopCloseRange = 2f;
    public float attackRange = 1.6f;

    [Header("Attack")]
    public float attackCooldown = 1.2f;
    private float attackTimer;

    private float strafeAmount;
    public Animator anim;

    private bool wasInAttackRange;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int currentPatrolPoint;
    public Transform pointsHolder;
    public float waitAtPointTime = 2f;
    private float waitCounter;

    private bool ownsPointsHolder;

    [Header("Health")]
    public float currentHealth = 25f;
    public float waitToDisappear = 4f;
    private bool isDead;

    [Header("Damage")]
    public float enemyDamage = 10f;

    [Header("Loot Drop")]
    public bool enableLootDrop = true;
    public GameObject ammoPickupPrefab;
    public GameObject healthPickupPrefab;

    [Range(0f, 1f)] public float dropChanceAmmo = 0.6f;
    [Range(0f, 1f)] public float dropChanceHealthKit = 0.2f;

    public int ammoDropAmount = 30;
    public int healthKitDropAmount = 1;

    public float dropYOffset = 0.1f;
    public float dropRandomOffsetRange = 0.5f;

    public float lootUpwardForce = 4f;
    public float lootSideForce = 2f;
    public float lootSpinForce = 10f;

    private bool lootDropped;

   
    [Header("SFX (OneShots)")]
    public AudioSource sfxSource;
    public AudioClip damageSfx;
    public AudioClip deathSfx;
    [Range(0f, 2f)] public float sfxVolume = 1f;

    public float damageSfxCooldown = 0.08f;
    private float nextDamageSfxTime;

    [Header("SFX (Idle Loop)")]
    public AudioSource idleLoopSource;
    public AudioClip idleLoopSfx;
    [Range(0f, 2f)] public float idleLoopVolume = 0.5f;
    public float idleHearDistance = 12f;   
    public float idleMinDistance = 2f;    

    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<Animator>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>(true);

        if (theRB == null)
            theRB = GetComponent<Rigidbody>();

        if (useNavMeshMovement && theRB != null)
        {
            theRB.useGravity = false;
            theRB.isKinematic = true;
            theRB.linearVelocity = Vector3.zero;
            theRB.angularVelocity = Vector3.zero;
        }

        if (anim == null)
            Debug.LogError($"{name}: EnemyController.anim NULL! Prefab içinde Animator bulunamadý.");

        if (useNavMeshMovement && GetComponent<UnityEngine.AI.NavMeshAgent>() == null)
            Debug.LogError($"{name}: useNavMeshMovement açýk ama NavMeshAgent yok!");

        
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 1f;

        
        if (idleLoopSource == null)
        {
           
            idleLoopSource = gameObject.AddComponent<AudioSource>();
        }

        idleLoopSource.playOnAwake = false;
        idleLoopSource.loop = true;
        idleLoopSource.spatialBlend = 1f;
        idleLoopSource.rolloffMode = AudioRolloffMode.Linear;
        idleLoopSource.minDistance = idleMinDistance;
        idleLoopSource.maxDistance = idleHearDistance;
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        strafeAmount = UnityEngine.Random.Range(-.75f, .75f);

        if (pointsHolder != null)
            ownsPointsHolder = pointsHolder.IsChildOf(transform);

        if (pointsHolder != null)
            pointsHolder.SetParent(null);

        waitCounter = waitAtPointTime;
        attackTimer = 0f;

        
        TryStartIdleLoop();
    }

    void FixedUpdate()
    {
        if (player == null || playerHealth == null)
        {
            StopIdleLoop();
            return;
        }

        if (playerHealth.isDead)
        {
            StopIdleLoop();

            if (theRB != null) theRB.linearVelocity = Vector3.zero;

            if (anim != null)
            {
                anim.SetBool("Moving", false);
                anim.ResetTrigger("Attack");
            }

            return;
        }

        if (isDead)
        {
            StopIdleLoop();

            waitCounter -= Time.fixedDeltaTime;
            if (waitCounter <= 0f)
            {
                Destroy(gameObject);

                if (pointsHolder != null && ownsPointsHolder)
                    Destroy(pointsHolder.gameObject);
            }
            return;
        }

        
        UpdateIdleLoopVolume();

        if (useNavMeshMovement)
        {
            return;
        }

        Vector3 targetPos = player.transform.position;
        targetPos.y = transform.position.y;

        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= chaseRange)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            theRB.MoveRotation(Quaternion.LookRotation(direction));

            bool inAttackRange = distance <= attackRange;

            if (wasInAttackRange && !inAttackRange)
            {
                if (anim != null) anim.ResetTrigger("Attack");
            }
            wasInAttackRange = inAttackRange;

            if (inAttackRange)
            {
                theRB.linearVelocity = Vector3.zero;

                attackTimer -= Time.fixedDeltaTime;
                if (attackTimer <= 0f)
                {
                    if (anim != null) anim.SetTrigger("Attack");
                    attackTimer = attackCooldown;
                }

                if (anim != null) anim.SetBool("Moving", false);
            }
            else
            {
                Vector3 vel = theRB.linearVelocity;
                vel.x = transform.forward.x * moveSpeed;
                vel.z = transform.forward.z * moveSpeed;
                theRB.linearVelocity = vel;

                if (anim != null) anim.SetBool("Moving", true);
            }

            return;
        }

       
        if (patrolPoints.Length > 0)
        {
            Vector3 patrolTarget = patrolPoints[currentPatrolPoint].position;
            patrolTarget.y = transform.position.y;

            float pointDistance = Vector3.Distance(transform.position, patrolTarget);

            if (pointDistance > 1f)
            {
                transform.LookAt(patrolTarget);

                Vector3 vel = theRB.linearVelocity;
                vel.x = transform.forward.x * moveSpeed;
                vel.z = transform.forward.z * moveSpeed;
                theRB.linearVelocity = vel;

                if (anim != null) anim.SetBool("Moving", true);
            }
            else
            {
                theRB.linearVelocity = Vector3.zero;
                if (anim != null) anim.SetBool("Moving", false);

                waitCounter -= Time.fixedDeltaTime;
                if (waitCounter <= 0f)
                {
                    currentPatrolPoint++;
                    if (currentPatrolPoint >= patrolPoints.Length)
                        currentPatrolPoint = 0;

                    waitCounter = waitAtPointTime;
                }
            }
        }
        else
        {
            theRB.linearVelocity = Vector3.zero;
            if (anim != null) anim.SetBool("Moving", false);
        }
    }

    public void TakeDamage(float damageToTake)
    {
        if (isDead) return;

        currentHealth -= damageToTake;

        
        if (damageSfx != null && sfxSource != null && Time.time >= nextDamageSfxTime)
        {
            sfxSource.PlayOneShot(damageSfx, sfxVolume);
            nextDamageSfxTime = Time.time + damageSfxCooldown;
        }

        if (currentHealth <= 0f)
        {
            
            if (deathSfx != null && sfxSource != null)
                sfxSource.PlayOneShot(deathSfx, sfxVolume);

            if (anim != null) anim.SetTrigger("Dead");
            isDead = true;

            StopIdleLoop(); 

            if (!deathNotified)
            {
                deathNotified = true;
                OnEnemyDied?.Invoke(isGeneral);
            }

            TryDropLoot();

            waitCounter = waitToDisappear;

            if (theRB != null)
            {
                theRB.linearVelocity = Vector3.zero;
                theRB.isKinematic = true;
            }

            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    public void DealDamage()
    {
        if (playerHealth == null || playerHealth.isDead) return;

        if (anim != null)
        {
            var st = anim.GetCurrentAnimatorStateInfo(0);
            if (!st.IsName("Attack"))
                return;
        }

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist > attackRange + 0.35f) return;

        playerHealth.TakeDamage(enemyDamage);
    }

    private void TryDropLoot()
    {
        if (!enableLootDrop) return;
        if (lootDropped) return;

        lootDropped = true;

        Vector3 dropPos = transform.position;
        dropPos.y += dropYOffset;
        dropPos.x += UnityEngine.Random.Range(-dropRandomOffsetRange, dropRandomOffsetRange);
        dropPos.z += UnityEngine.Random.Range(-dropRandomOffsetRange, dropRandomOffsetRange);

        if (ammoPickupPrefab != null && UnityEngine.Random.value < dropChanceAmmo)
        {
            GameObject ammoObj = null;

            if (PickupPool.Instance != null)
                ammoObj = PickupPool.Instance.SpawnAmmo(dropPos, Quaternion.identity);
            else
                ammoObj = Instantiate(ammoPickupPrefab, dropPos, Quaternion.identity);

            if (ammoObj != null)
            {
                AmmoPickup ammoPickup = ammoObj.GetComponent<AmmoPickup>();
                if (ammoPickup != null)
                    ammoPickup.ammoAmount = ammoDropAmount;

                ApplyLootForce(ammoObj);
            }
        }

        if (healthPickupPrefab != null && UnityEngine.Random.value < dropChanceHealthKit)
        {
            GameObject healthObj = null;

            if (PickupPool.Instance != null)
                healthObj = PickupPool.Instance.SpawnHealth(dropPos, Quaternion.identity);
            else
                healthObj = Instantiate(healthPickupPrefab, dropPos, Quaternion.identity);

            if (healthObj != null)
            {
                HealthPickup healthPickup = healthObj.GetComponent<HealthPickup>();
                if (healthPickup != null)
                    healthPickup.kitAmount = healthKitDropAmount;

                ApplyLootForce(healthObj);
            }
        }
    }

    private void ApplyLootForce(GameObject lootObj)
    {
        Rigidbody rb = lootObj.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 force = Vector3.up * lootUpwardForce;
        force.x += UnityEngine.Random.Range(-lootSideForce, lootSideForce);
        force.z += UnityEngine.Random.Range(-lootSideForce, lootSideForce);

        rb.AddForce(force, ForceMode.Impulse);

        Vector3 spin = new Vector3(
            UnityEngine.Random.Range(-lootSpinForce, lootSpinForce),
            UnityEngine.Random.Range(-lootSpinForce, lootSpinForce),
            UnityEngine.Random.Range(-lootSpinForce, lootSpinForce)
        );

        rb.AddTorque(spin, ForceMode.Impulse);
    }

    public void NavAttackTick(float dt)
    {
        attackTimer -= dt;
        if (attackTimer <= 0f)
        {
            if (anim != null) anim.SetTrigger("Attack");
            attackTimer = attackCooldown;
        }
    }

    
    private void TryStartIdleLoop()
    {
        if (idleLoopSource == null) return;
        if (idleLoopSfx == null) return;

        idleLoopSource.clip = idleLoopSfx;
        idleLoopSource.volume = idleLoopVolume;
        if (!idleLoopSource.isPlaying)
            idleLoopSource.Play();
    }

    private void StopIdleLoop()
    {
        if (idleLoopSource == null) return;
        if (idleLoopSource.isPlaying)
            idleLoopSource.Stop();
    }

    private void UpdateIdleLoopVolume()
    {
        
        if (player == null) { StopIdleLoop(); return; }

        
        if (idleLoopSfx == null) { StopIdleLoop(); return; }

        float dist = Vector3.Distance(transform.position, player.transform.position);

        
        if (dist > idleHearDistance)
        {
            StopIdleLoop();
            return;
        }

        
        TryStartIdleLoop();

       
        float t = Mathf.InverseLerp(idleHearDistance, idleMinDistance, dist); 
        idleLoopSource.volume = idleLoopVolume * Mathf.Clamp01(t);
    }

    private void OnDisable()
    {
        StopIdleLoop();
    }
    
}
