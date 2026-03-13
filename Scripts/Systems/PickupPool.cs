using System.Collections.Generic;
using UnityEngine;

public class PickupPool : MonoBehaviour
{
    public static PickupPool Instance;

    [Header("Prefabs")]
    public GameObject ammoPickupPrefab;
    public GameObject healthPickupPrefab;

    [Header("Prewarm")]
    public int ammoPrewarm = 20;
    public int healthPrewarm = 10;

    [Header("Auto Return (not picked)")]
    public float autoReturnSeconds = 30f;

    private readonly Queue<GameObject> ammoPool = new Queue<GameObject>();
    private readonly Queue<GameObject> healthPool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (ammoPickupPrefab == null) Debug.LogError("PickupPool: ammoPickupPrefab NULL");
        if (healthPickupPrefab == null) Debug.LogError("PickupPool: healthPickupPrefab NULL");

        if (ammoPickupPrefab != null) Prewarm(ammoPool, ammoPickupPrefab, ammoPrewarm);
        if (healthPickupPrefab != null) Prewarm(healthPool, healthPickupPrefab, healthPrewarm);
    }

    private void Prewarm(Queue<GameObject> q, GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var go = CreateNew(prefab);
            Return(go);
        }
    }

    private GameObject CreateNew(GameObject prefab)
    {
        var go = Instantiate(prefab);
        go.name = prefab.name + "_Pooled";
        go.SetActive(false);

        var tag = go.GetComponent<PooledPickup>();
        if (tag == null) tag = go.AddComponent<PooledPickup>();
        tag.autoReturnSeconds = autoReturnSeconds;

        return go;
    }

    public GameObject SpawnAmmo(Vector3 pos, Quaternion rot)
        => SpawnInternal(ammoPool, ammoPickupPrefab, pos, rot);

    public GameObject SpawnHealth(Vector3 pos, Quaternion rot)
        => SpawnInternal(healthPool, healthPickupPrefab, pos, rot);

    private GameObject SpawnInternal(Queue<GameObject> q, GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;

        GameObject go = q.Count > 0 ? q.Dequeue() : CreateNew(prefab);
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);

    
        var tag = go.GetComponent<PooledPickup>();
        if (tag != null) tag.BeginAutoReturn();

        return go;
    }

    public void Return(GameObject go)
    {
        if (go == null) return;

        var tag = go.GetComponent<PooledPickup>();
        if (tag == null)
        {
            Destroy(go);
            return;
        }

        go.SetActive(false);

        if (ammoPickupPrefab != null && go.name.StartsWith(ammoPickupPrefab.name))
            ammoPool.Enqueue(go);
        else if (healthPickupPrefab != null && go.name.StartsWith(healthPickupPrefab.name))
            healthPool.Enqueue(go);
        else
            ammoPool.Enqueue(go); 
    }
}
