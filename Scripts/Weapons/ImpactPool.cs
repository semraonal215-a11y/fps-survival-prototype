using System.Collections.Generic;
using UnityEngine;

public class ImpactPool : MonoBehaviour
{
    public static ImpactPool Instance;

    [Header("Pool Settings")]
    public GameObject impactPrefab;
    public int prewarmCount = 30;
    public float defaultLifeTime = 2f;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

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
        if (impactPrefab == null)
        {
            Debug.LogError("ImpactPool: impactPrefab is NULL.");
            return;
        }

        Prewarm(prewarmCount);
    }

    private void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var go = CreateNew();
            Return(go);
        }
    }

    private GameObject CreateNew()
    {
        var go = Instantiate(impactPrefab);
        go.name = impactPrefab.name + "_Pooled";
        go.SetActive(false);

        
        var ret = go.GetComponent<PooledReturnToImpactPool>();
        if (ret == null) ret = go.AddComponent<PooledReturnToImpactPool>();
        ret.lifeTime = defaultLifeTime;

        return go;
    }

    public GameObject Spawn(Vector3 pos, Quaternion rot, float? lifeTimeOverride = null)
    {
        if (impactPrefab == null)
        {
            Debug.LogError("ImpactPool: impactPrefab is NULL.");
            return null;
        }

        GameObject go = pool.Count > 0 ? pool.Dequeue() : CreateNew();

        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);

        var ret = go.GetComponent<PooledReturnToImpactPool>();
        if (ret != null)
            ret.Begin(lifeTimeOverride ?? defaultLifeTime);

        return go;
    }

    public void Return(GameObject go)
    {
        if (go == null) return;

        go.SetActive(false);
        pool.Enqueue(go);
    }
}
