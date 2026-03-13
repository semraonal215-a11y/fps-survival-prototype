using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [Header("References")]
    public Transform target;         
    public Camera miniMapCam;         

    [Header("Follow")]
    public float height = 25f;       
    public Vector3 offset = Vector3.zero;

    [Header("Rotation")]
    public bool followPlayerYaw = true;   

    [Header("Update Mode")]
    public bool useLateUpdate = true;     

    private void Awake()
    {
        if (miniMapCam == null)
            miniMapCam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (useLateUpdate) Tick();
    }

    private void FixedUpdate()
    {
        if (!useLateUpdate) Tick();
    }

    private void Tick()
    {
        if (target == null) return;

       
        Vector3 pos = target.position + offset;
        pos.y = target.position.y + height;
        transform.position = pos;

        
        if (followPlayerYaw)
        {
            float y = target.eulerAngles.y;
            transform.rotation = Quaternion.Euler(90f, y, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
