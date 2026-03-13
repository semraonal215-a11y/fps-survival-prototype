using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    [Header("References (Optional)")]
    public CharacterController characterController; 

    [Header("Bobbing")]
    public float walkFrequency = 1.6f;
    public float walkAmplitude = 0.05f;

    public float sprintFrequency = 2.2f;
    public float sprintAmplitude = 0.08f;

    [Header("Smoothing")]
    public float returnSpeed = 10f;

    private Vector3 startLocalPos;
    private float bobTimer;

    
    private bool isMoving;
    private bool isSprinting;

    void Awake()
    {
        startLocalPos = transform.localPosition;
    }

    void OnEnable()
    {
        startLocalPos = transform.localPosition;
        bobTimer = 0f;
    }

    
    public void SetMoveState(bool moving, bool sprinting)
    {
        isMoving = moving;
        isSprinting = sprinting;
    }

    void Update()
    {
        
        if (characterController != null)
        {
            Vector3 horizontalVel = characterController.velocity;
            horizontalVel.y = 0f;

           
            if (!isMoving)
                isMoving = horizontalVel.magnitude > 0.1f;
        }

        if (!isMoving)
        {
            
            transform.localPosition = Vector3.Lerp(transform.localPosition, startLocalPos, Time.deltaTime * returnSpeed);
            bobTimer = 0f;
            return;
        }

        float freq = isSprinting ? sprintFrequency : walkFrequency;
        float amp = isSprinting ? sprintAmplitude : walkAmplitude;

        bobTimer += Time.deltaTime * freq;

        float yOffset = Mathf.Sin(bobTimer * Mathf.PI * 2f) * amp;
        float xOffset = Mathf.Cos(bobTimer * Mathf.PI * 2f) * (amp * 0.5f);

        Vector3 target = startLocalPos + new Vector3(xOffset, yOffset, 0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * returnSpeed);
    }
}
