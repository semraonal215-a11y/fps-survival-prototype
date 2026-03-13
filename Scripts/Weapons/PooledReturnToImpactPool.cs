using System.Collections;
using UnityEngine;

public class PooledReturnToImpactPool : MonoBehaviour
{
    public float lifeTime = 2f;

    private Coroutine routine;

    public void Begin(float newLifeTime)
    {
        lifeTime = newLifeTime;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ReturnAfter());
    }

    private IEnumerator ReturnAfter()
    {
        yield return new WaitForSeconds(lifeTime);

        if (ImpactPool.Instance != null)
            ImpactPool.Instance.Return(gameObject);
        else
            Destroy(gameObject); 
    }

    void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
}
