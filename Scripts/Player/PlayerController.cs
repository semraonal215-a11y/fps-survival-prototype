using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController playerController;
    public Camera playerCam;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -20f;
    public float jumpForce = 8f;

    [Header("Look")]
    public float lookSpeed = 100f;
    public float minViewAngle = -80f;
    public float maxViewAngle = 80f;

    [Header("Camera FOV")]
    public float camZoomNormal = 60f;
    public float camZoomOut = 70f;
    public float camZoomSpeed = 8f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;
    public InputActionReference shootAction;
    public InputActionReference reloadAction;

    private float yVelocity;
    private float yaw;
    private float pitch;
    private bool isSprinting;

    [Header("Weapons")]
    public WeaponsController weaponController;

    [Header("Animation")]
    [SerializeField] private Animator anim;
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimJump = Animator.StringToHash("Jump");

    [Header("Game Feel")]
    public CameraBobbing cameraBobbing;

   
    [Header("SFX - Footsteps")]
    public AudioSource footstepSource;
    public AudioClip[] walkFootsteps;
    public AudioClip[] runFootsteps;

    [Range(0f, 2f)] public float footstepVolume = 0.9f;
    public float walkStepInterval = 0.55f;
    public float runStepInterval = 0.35f;

    private float footstepTimer;
    
    [Header("SFX - Jump")]
    public AudioSource jumpSource;
    public AudioClip jumpClip;
    [Range(0f, 2f)] public float jumpVolume = 1f;

    private bool IsAnimatorPlayable(Animator a)
    {
        return a != null
            && a.isActiveAndEnabled
            && a.gameObject.activeInHierarchy
            && a.runtimeAnimatorController != null;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = 0f;

        playerCam.fieldOfView = camZoomNormal;

        if (anim == null)
            anim = GetComponentInChildren<Animator>(true);

        if (cameraBobbing == null && playerCam != null)
            cameraBobbing = playerCam.GetComponent<CameraBobbing>();

    
        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>();

        if (footstepSource == null)
            footstepSource = gameObject.AddComponent<AudioSource>();

        footstepSource.playOnAwake = false;
        footstepSource.loop = false;
        footstepSource.spatialBlend = 0f; // 2D
        footstepTimer = 0f;

      
        if (jumpSource == null)
            jumpSource = footstepSource;

        if (jumpSource != null)
        {
            jumpSource.playOnAwake = false;
            jumpSource.loop = false;
          
            jumpSource.spatialBlend = 0f;
        }
        
    }

    void Update()
    {
        // INPUT
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        if (sprintAction.action.WasPressedThisFrame())
            isSprinting = true;

        if (sprintAction.action.WasReleasedThisFrame())
            isSprinting = false;

        // MOVE
        float speed = isSprinting ? runSpeed : moveSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        bool groundedNow = playerController.isGrounded;

        if (groundedNow)
        {
            if (yVelocity < 0)
                yVelocity = -2f;

            if (jumpAction.action.WasPressedThisFrame())
            {
                yVelocity = jumpForce;

                if (anim != null)
                    anim.SetTrigger(AnimJump);

                
                if (jumpClip != null && jumpSource != null)
                    jumpSource.PlayOneShot(jumpClip, jumpVolume);
                
            }
        }

        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = yVelocity;

        playerController.Move(velocity * Time.deltaTime);

      
        HandleFootsteps(moveInput, groundedNow);
       

        if (cameraBobbing != null)
        {
            bool moving = moveInput.sqrMagnitude > 0.01f && groundedNow;
            cameraBobbing.SetMoveState(moving, isSprinting);
        }

        if (anim != null)
        {
            float inputMag = moveInput.magnitude;
            float animSpeed = inputMag * (isSprinting ? 1f : 0.5f);
            anim.SetFloat(AnimSpeed, animSpeed);
            anim.SetBool(AnimIsGrounded, groundedNow);
        }

        // LOOK
        yaw += lookInput.x * lookSpeed * Time.deltaTime;
        pitch -= lookInput.y * lookSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minViewAngle, maxViewAngle);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        playerCam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // CAMERA FOV
        float targetFov = isSprinting ? camZoomOut : camZoomNormal;
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFov, camZoomSpeed * Time.deltaTime);

        // SHOOT 
        if (weaponController != null)
        {
            if (shootAction.action.WasPressedThisFrame())
            {
                weaponController.Shoot();

                if (weaponController != null && !weaponController.canAutoFire && weaponController.currentAmmo > 0)
                {
                    Animator a = weaponController.weaponAnimator;
                    if (IsAnimatorPlayable(a))
                        a.SetTrigger("Fire");
                }
            }

            if (shootAction.action.IsPressed())
            {
                weaponController.ShootHeld();
            }

            
            if (shootAction.action.WasReleasedThisFrame())
            {
                weaponController.StopAutoFireLoop();
            }

            if (weaponController.canAutoFire)
            {
                Animator a = weaponController.weaponAnimator;
                if (a != null)
                {
                    bool hasAmmoInClip = weaponController.currentAmmo > 0;
                    a.SetBool("IsFiring", shootAction.action.IsPressed() && hasAmmoInClip);
                }
            }
        }

        // RELOAD
        if (reloadAction.action.WasPressedThisFrame())
        {
            if (weaponController == null)
            {
                Debug.LogWarning("Reload pressed but weaponController is NULL");
                return;
            }

            weaponController.RequestReload();
        }

        // HEALTH KIT
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (PlayerHealth.Instance.currentHealth >= PlayerHealth.Instance.maxHealth)
                return;

            if (Inventory.Instance.UseHealthKit())
            {
                float heal = Inventory.Instance.GetHealthKitHealAmount();
                PlayerHealth.Instance.Heal(heal);
            }
        }
    }

  
    private void HandleFootsteps(Vector2 moveInput, bool groundedNow)
    {
        
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.isDead)
            return;

        bool moving = moveInput.sqrMagnitude > 0.01f;
        if (!groundedNow || !moving)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer > 0f) return;

        AudioClip clip = GetRandomFootstepClip();
        if (clip != null && footstepSource != null)
            footstepSource.PlayOneShot(clip, footstepVolume);

        footstepTimer = isSprinting ? runStepInterval : walkStepInterval;
    }

    private AudioClip GetRandomFootstepClip()
    {
        AudioClip[] list = isSprinting ? runFootsteps : walkFootsteps;
        if (list == null || list.Length == 0) return null;

        int idx = Random.Range(0, list.Length);
        return list[idx];
    }
    
}