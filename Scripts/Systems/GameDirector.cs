using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AI;

public class GameDirector : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Normal Enemies (4 zombie + dog)")]
    public GameObject[] normalEnemyPrefabs;

    [Header("Boss (General)")]
    public GameObject generalPrefab;
    public int totalGeneralsToSpawn = 5;
    private int generalsAlive; 

    [Header("Wave Timing")]
    public float firstWaveDelay = 120f;

    [Header("Spawn Control")]
    public float normalSpawnInterval = 1.0f;
    public int maxAliveEnemies = 12;

    
    [HideInInspector]
    public float generalSpawnInterval = 6f;
    
    [Header("Boss Progression")]
    public float timeBetweenGeneralsAfterKill = 10f;

    [Header("Level Transition")]
    public float levelClearDelay = 2f;

    private int aliveEnemies;
    private int generalsSpawned;
    private int generalsKilled;

    private Coroutine normalSpawnRoutine;
    private Coroutine bossRoutine;

  
    private bool waitingForGeneralDeath;

    private void OnEnable()
    {
        EnemyController.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EnemyController.OnEnemyDied -= HandleEnemyDied;
    }

    private void Start()
    {
        Debug.Log("GameDirector START OK");

        normalSpawnRoutine = StartCoroutine(NormalSpawnLoop());
        StartCoroutine(StartBossPhaseAfterDelay());
    }

    private IEnumerator NormalSpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(normalSpawnInterval);

            if (aliveEnemies >= maxAliveEnemies)
                continue;

            SpawnRandomNormalEnemy();
        }
    }

    private IEnumerator StartBossPhaseAfterDelay()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        
        bossRoutine = StartCoroutine(BossSequence());
    }

    
    private IEnumerator BossSequence()
    {
        while (generalsSpawned < totalGeneralsToSpawn)
        {
           
            while (aliveEnemies >= maxAliveEnemies)
                yield return null;

          
            SpawnGeneral();
            generalsSpawned++;

           
            waitingForGeneralDeath = true;
            while (waitingForGeneralDeath)
                yield return null;

            
            yield return new WaitForSeconds(timeBetweenGeneralsAfterKill);
        }
    }

    private void SpawnRandomNormalEnemy()
    {
        if (normalEnemyPrefabs == null || normalEnemyPrefabs.Length == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        Transform sp = PickSpawnPointNearPlayer(40f);
        GameObject prefab = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];

        if (prefab == null)
        {
            Debug.LogError("GameDirector: normalEnemyPrefabs NULL ");
            return;
        }

        if (prefab.scene.IsValid())
        {
            Debug.LogError("GameDirector: normalEnemyPrefabs Game Object " + prefab.name);
            return;
        }

        GameObject obj = Instantiate(prefab, sp.position, sp.rotation);

        aliveEnemies++;

        
        EnsureNavMeshSetup(obj);
    }

    private void SpawnGeneral()
    {
        if (generalPrefab == null) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        Transform sp = PickSpawnPointNearPlayer(40f);

        GameObject obj = Instantiate(generalPrefab, sp.position, sp.rotation);

       
        Debug.Log("GENERAL SPAWNED -> " + obj.name + " at " + sp.position);

        EnemyController ec = obj.GetComponent<EnemyController>();
        if (ec != null) ec.isGeneral = true;

        aliveEnemies++;

        
        generalsAlive++;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBossBoost(true);

        EnsureNavMeshSetup(obj);
    }

    
    private Transform PickSpawnPointNearPlayer(float maxDistance)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;

        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return spawnPoints[Random.Range(0, spawnPoints.Length)];

        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform sp = spawnPoints[i];
            if (sp == null) continue;

            float d = Vector3.Distance(player.transform.position, sp.position);

            if (d <= maxDistance && d < bestDist)
            {
                bestDist = d;
                best = sp;
            }
        }

        if (best == null)
            best = spawnPoints[Random.Range(0, spawnPoints.Length)];

        return best;
    }

    
    private void EnsureNavMeshSetup(GameObject obj)
    {
        if (obj == null) return;

        EnemyController ec = obj.GetComponent<EnemyController>();
        if (ec == null) return;

        if (!ec.useNavMeshMovement) return;

        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = obj.AddComponent<NavMeshAgent>();

            agent.speed = 3.5f;
            agent.angularSpeed = 360f;
            agent.acceleration = 8f;
            agent.stoppingDistance = ec.attackRange;
            agent.autoBraking = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.radius = 0.35f;
            agent.height = 2f;
        }

        EnemyNavMeshBrain brain = obj.GetComponent<EnemyNavMeshBrain>();
        if (brain == null)
        {
            obj.AddComponent<EnemyNavMeshBrain>();
        }

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void HandleEnemyDied(bool isGeneral)
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);

        if (isGeneral)
        {
            generalsKilled++;
            generalsAlive = Mathf.Max(0, generalsAlive - 1);

          
            waitingForGeneralDeath = false;

            
            if (generalsAlive <= 0 && AudioManager.Instance != null)
                AudioManager.Instance.SetBossBoost(false);

            if (generalsKilled >= totalGeneralsToSpawn)
            {
                StartCoroutine(LoadNextLevel());
            }
        }
    }

    private IEnumerator LoadNextLevel()
    {
        EnemyController.OnEnemyDied -= HandleEnemyDied;

        yield return new WaitForSeconds(levelClearDelay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}