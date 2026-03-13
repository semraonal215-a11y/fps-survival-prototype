using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Default Shake")]
    public float defaultDuration = 0.12f;
    public float defaultMagnitude = 0.08f;

    [Header("Optional")]
    public bool useLocalPosition = true;

    private Vector3 startLocalPos;
    private Vector3 startWorldPos;
    private Coroutine routine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        startLocalPos = transform.localPosition;
        startWorldPos = transform.position;
    }

    void OnEnable()
    {
        
        startLocalPos = transform.localPosition;
        startWorldPos = transform.position;
    }

    public void Shake(float magnitude = -1f, float duration = -1f)
    {
        if (magnitude <= 0f) magnitude = defaultMagnitude;
        if (duration <= 0f) duration = defaultDuration;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShakeRoutine(magnitude, duration));
    }

    private IEnumerator ShakeRoutine(float magnitude, float duration)
    {
        float t = 0f;

        
        while (t < duration)
        {
            t += Time.deltaTime;
            float damper = 1f - Mathf.Clamp01(t / duration); 

            Vector3 offset = Random.insideUnitSphere * (magnitude * damper);
            offset.z = 0f;

            if (useLocalPosition)
                transform.localPosition = startLocalPos + offset;
            else
                transform.position = startWorldPos + offset;

            yield return null;
        }

       
        if (useLocalPosition)
            transform.localPosition = startLocalPos;
        else
            transform.position = startWorldPos;

        routine = null;
    }
}
