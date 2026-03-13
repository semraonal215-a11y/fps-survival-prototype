using UnityEngine;
using System.Collections;

public class WeaponsController : MonoBehaviour
{
    public float range;
    public Transform cam;
    public LayerMask validLayers;

    public GameObject impactEffect, damageEffect;

    public bool canAutoFire;
    public float timeBetweenShoots = .1f;
    private float shotCounter;

    [Header("Ammo")]
    public int currentAmmo = 15;
    public int clipSize = 15;

    [Header("Damage")]
    public float damageAmount = 15f;

    private UIController UIcontrol;

    private int RemainingAmmo => (Inventory.Instance != null) ? Inventory.Instance.GetAmmoCount() : 0;

    [Header("Animation")]
    public Animator weaponAnimator;

    private bool isReloading;
    public bool IsReloading => isReloading;

    [Header("Reload State ")]
    public string reloadStateName = "Reload";
    public int reloadLayerIndex = 0;
    private int reloadStateHash;

    [Header("SFX ")]
    public AudioSource sfxSource;
    public AudioClip fireSfx;
    public AudioClip reloadSfx;
    public AudioClip emptyClickSfx;


    [Header("Pull Out SFX")]
    public AudioClip chamberSfx;
    public float chamberCooldown = 0.25f;
    private float nextChamberTime;


    [Header("AUTO Fire Loop")]
    public AudioClip fireLoopSfx;
    private bool isAutoLoopPlaying;

    [Header("SFX Settings")]
    public float sfxVolume = 1f;
    public float emptyClickCooldown = 0.15f;
    private float nextEmptySfxTime;

    void Awake()
    {
        reloadStateHash = Animator.StringToHash(reloadStateName);
    }

    void Start()
    {
        UIcontrol = FindFirstObjectByType<UIController>();
        UpdateAmmoUI();

        EnsureSfxSource();
    }

    private void EnsureSfxSource()
    {
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2D
    }

    private void OnEnable()
    {
        EnsureSfxSource();
    }

    public void PlayChamberSfx()
    {
        if (Time.time < nextChamberTime) return;
        nextChamberTime = Time.time + chamberCooldown;

        EnsureSfxSource();

        if (chamberSfx == null || sfxSource == null) return;


        StopAutoFireLoop();

        sfxSource.PlayOneShot(chamberSfx, sfxVolume);
    }


    public void Shoot()
    {
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            StopAutoFireLoop();

            if (emptyClickSfx != null && Time.time >= nextEmptySfxTime)
            {
                if (sfxSource != null)
                    sfxSource.PlayOneShot(emptyClickSfx, sfxVolume);

                nextEmptySfxTime = Time.time + emptyClickCooldown;
            }

            UpdateAmmoUI();
            return;
        }

        RaycastHit hit;

        if (Physics.Raycast(cam.position, cam.forward, out hit, range, validLayers))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Instantiate(damageEffect, hit.point, Quaternion.identity);

                EnemyController enemy = hit.transform.GetComponent<EnemyController>();
                if (enemy != null)
                    enemy.TakeDamage(damageAmount);
            }
            else
            {
                if (ImpactPool.Instance != null)
                    ImpactPool.Instance.Spawn(hit.point, Quaternion.identity);
                else
                    Instantiate(impactEffect, hit.point, Quaternion.identity);
            }
        }

        if (!(canAutoFire && fireLoopSfx != null))
        {
            if (fireSfx != null && sfxSource != null)
                sfxSource.PlayOneShot(fireSfx, sfxVolume);
        }

        shotCounter = timeBetweenShoots;
        currentAmmo--;
        UpdateAmmoUI();
    }

    public void ShootHeld()
    {
        if (isReloading) { StopAutoFireLoop(); return; }
        if (!canAutoFire) return;

        if (fireLoopSfx != null && currentAmmo > 0)
            StartAutoFireLoop();
        else
            StopAutoFireLoop();

        shotCounter -= Time.deltaTime;
        if (shotCounter <= 0) Shoot();
    }

    public void StopAutoFireLoop()
    {
        if (!isAutoLoopPlaying)
            return;

        if (sfxSource != null)
        {
            if (sfxSource.isPlaying && sfxSource.clip == fireLoopSfx)
                sfxSource.Stop();

            sfxSource.loop = false;
            sfxSource.clip = null;
        }

        isAutoLoopPlaying = false;
    }

    private void StartAutoFireLoop()
    {
        EnsureSfxSource();

        if (isAutoLoopPlaying) return;
        if (sfxSource == null || fireLoopSfx == null) return;

        sfxSource.Stop();
        sfxSource.clip = fireLoopSfx;
        sfxSource.volume = sfxVolume;
        sfxSource.loop = true;
        sfxSource.Play();

        isAutoLoopPlaying = true;
    }

    public void RequestReload()
    {
        if (isReloading) return;
        if (Inventory.Instance == null) return;

        int need = clipSize - currentAmmo;
        if (need <= 0) return;

        int available = RemainingAmmo;
        if (available <= 0) return;

        isReloading = true;

        StopAutoFireLoop();

        if (reloadSfx != null && sfxSource != null)
            sfxSource.PlayOneShot(reloadSfx, sfxVolume);

        Animator a = weaponAnimator != null ? weaponAnimator : GetComponentInChildren<Animator>();
        if (a == null)
        {
            ReloadAmmoLogic();
            isReloading = false;
            return;
        }

        a.SetBool("IsReloading", true);
        a.ResetTrigger("Reload");
        a.SetTrigger("Reload");

        StopAllCoroutines();
        StartCoroutine(EnsureReloadStateStarted(a));
    }

    private IEnumerator EnsureReloadStateStarted(Animator a)
    {
        yield return null;

        if (a == null) yield break;

        var st = a.GetCurrentAnimatorStateInfo(reloadLayerIndex);

        if (st.shortNameHash != reloadStateHash)
        {
            a.CrossFade(reloadStateHash, 0.05f, reloadLayerIndex, 0f);
        }
    }

    private void ReloadAmmoLogic()
    {
        if (Inventory.Instance == null) return;

        int need = clipSize - currentAmmo;
        if (need <= 0) return;

        int available = RemainingAmmo;
        int take = Mathf.Min(need, available);

        if (take > 0 && Inventory.Instance.UseAmmo(take))
            currentAmmo += take;

        UpdateAmmoUI();
    }

    public void ReloadFromAnimEvent()
    {
        ReloadAmmoLogic();
    }

    public void ReloadEndFromAnimEvent()
    {
        isReloading = false;

        Animator a = weaponAnimator != null ? weaponAnimator : GetComponentInChildren<Animator>();
        if (a != null) a.SetBool("IsReloading", false);

        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (UIcontrol != null)
            UIcontrol.UpdateAmmoText(currentAmmo, RemainingAmmo);
    }

    private void OnDisable()
    {
        StopAutoFireLoop();

        isAutoLoopPlaying = false;

        if (sfxSource != null)
            sfxSource.Stop();
    }
}