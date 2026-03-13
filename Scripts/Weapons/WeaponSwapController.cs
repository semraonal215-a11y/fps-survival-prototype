using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WeaponSwapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Weapons (Child Objects)")]
    [SerializeField] private WeaponsController pistol;
    [SerializeField] private WeaponsController autoGun;

    [Header("Start Weapon")]
    [SerializeField] private bool startWithPistol = true;

    private WeaponsController current;

    [SerializeField] private WeaponAmmoMemory ammoMemory;

    [Header("Scroll Swap")]
    [SerializeField] private bool enableMouseScrollSwap = true;
    [SerializeField] private float scrollThreshold = 0.01f;

    [Header("Draw/Holster (Optional)")]
    [SerializeField] private bool useDrawHolster = false;
    [SerializeField] private float swapDelay = 0.18f;
    [SerializeField] private string pullOutTrigger = "PullOut";
    [SerializeField] private string pullDownTrigger = "PullDown";
    private bool isSwapping;

    [Header("Swap SFX")]
    public AudioClip swapSfx;
    [Range(0f, 2f)] public float swapSfxVolume = 1f;
    public AudioSource swapSfxSource;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        if (ammoMemory == null)
            ammoMemory = GetComponentInParent<WeaponAmmoMemory>();
    }

    private void Start()
    {
        if (ammoMemory != null)
            ammoMemory.InitFromWeapons();

        if (startWithPistol)
            EquipImmediate(pistol);
        else
            EquipImmediate(autoGun);

       
        if (useDrawHolster && current != null)
        {
            Animator a = GetWeaponAnimator(current);
            if (a != null)
                a.SetTrigger(pullOutTrigger);
        }

        
        if (swapSfxSource == null)
        {
           
            AudioManager am = FindFirstObjectByType<AudioManager>();
            if (am != null)
            {
                swapSfxSource = am.GetComponent<AudioSource>();
            }

          
            if (swapSfxSource == null && playerController != null)
            {
                swapSfxSource = playerController.GetComponent<AudioSource>();
                if (swapSfxSource == null)
                    swapSfxSource = playerController.gameObject.AddComponent<AudioSource>();
            }
        }

   
        if (swapSfxSource == null)
            swapSfxSource = gameObject.AddComponent<AudioSource>();

        swapSfxSource.playOnAwake = false;
        swapSfxSource.loop = false;
        swapSfxSource.spatialBlend = 0f; // 2D
    }

    private void Update()
    {
        if (isSwapping) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            Equip(pistol);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            Equip(autoGun);

        if (enableMouseScrollSwap && Mouse.current != null)
        {
            float scrollY = Mouse.current.scroll.ReadValue().y;

            if (scrollY > scrollThreshold)
                EquipNext();
            else if (scrollY < -scrollThreshold)
                EquipPrev();
        }
    }

    private void EquipNext()
    {
        if (current == pistol) Equip(autoGun);
        else Equip(pistol);
    }

    private void EquipPrev()
    {
        if (current == pistol) Equip(autoGun);
        else Equip(pistol);
    }

    private void Equip(WeaponsController weapon)
    {
        if (weapon == null) return;
        if (current == weapon) return;

  
        if (current != null)
        {
            if (current.IsReloading) return;

            Animator ca = GetWeaponAnimator(current);
            if (current.canAutoFire && ca != null && ca.GetBool("IsFiring"))
                return;
        }
      

        if (useDrawHolster)
            StartCoroutine(EquipWithDrawHolster(weapon));
        else
            EquipImmediate(weapon);
    }

    private IEnumerator EquipWithDrawHolster(WeaponsController weapon)
    {
        isSwapping = true;

        if (current != null) current.StopAutoFireLoop();

       
        if (current != null)
        {
            Animator a = GetWeaponAnimator(current);
            if (a != null)
            {
                a.ResetTrigger(pullOutTrigger);
                a.ResetTrigger(pullDownTrigger);
                a.SetTrigger(pullDownTrigger);
            }
        }

        yield return new WaitForSeconds(swapDelay);

        EquipImmediate(weapon);

        if (current != null)
        {
            Animator a = GetWeaponAnimator(current);
            if (a != null)
            {
                a.ResetTrigger(pullOutTrigger);
                a.ResetTrigger(pullDownTrigger);
                a.SetTrigger(pullOutTrigger);
            }
        }

        isSwapping = false;
    }

    private void EquipImmediate(WeaponsController weapon)
    {
        if (weapon == null) return;

        if (current != null) current.StopAutoFireLoop();

        if (ammoMemory != null)
            ammoMemory.SaveCurrent(current);

        if (pistol != null) pistol.gameObject.SetActive(weapon == pistol);
        if (autoGun != null) autoGun.gameObject.SetActive(weapon == autoGun);

        current = weapon;

        if (ammoMemory != null)
            ammoMemory.LoadTo(current);

        if (playerController != null)
            playerController.weaponController = current;

        var ui = UIController.instance;
        if (ui != null && Inventory.Instance != null && current != null)
            ui.UpdateAmmoText(current.currentAmmo, Inventory.Instance.GetAmmoCount());

        
        if (swapSfx != null && swapSfxSource != null && swapSfxSource.isActiveAndEnabled)
            swapSfxSource.PlayOneShot(swapSfx, swapSfxVolume);
    }

    private Animator GetWeaponAnimator(WeaponsController w)
    {
        if (w == null) return null;
        if (w.weaponAnimator != null) return w.weaponAnimator;
        return w.GetComponentInChildren<Animator>(true);
    }
}
