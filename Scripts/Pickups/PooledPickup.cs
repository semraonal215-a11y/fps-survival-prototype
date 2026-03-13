using System.Collections;
using UnityEngine;

public class PooledPickup : MonoBehaviour
{
    public float autoReturnSeconds = 30f;
    private Coroutine routine;

    public void BeginAutoReturn()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(AutoReturn());
    }

    private IEnumerator AutoReturn()
    {
        yield return new WaitForSeconds(autoReturnSeconds);

        if (PickupPool.Instance != null)
            PickupPool.Instance.Return(gameObject);
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
