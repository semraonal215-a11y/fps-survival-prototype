using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    [Header("Texts")]
    public TMP_Text ammoText, remainingAmmoText;
    public TMP_Text healthText;
    public TMP_Text healthKitText;

    [Header("Health Bar (Front + Back)")]
    public Slider healthBarFront; 
    public Slider healthBarBack;  

    public float frontSmoothSpeed = 12f;

    public float backCatchupDelay = 0.15f;

    
    public float backCatchupSpeed = 0.8f;

    private float healthTarget01 = 1f;
    private Coroutine backRoutine;

    [Header("Damage Effect")]
    public Image damageOverlayImage;
    public float damageFlashMaxAlpha = 0.5f;
    public float damageFadeSpeed = 3f;

    private Coroutine damageRoutine;

   
    private Inventory cachedInventory;
   

    private void Awake()
    {
        instance = this;

        
        if (healthKitText != null)
            healthKitText.text = "0/0";
      

        TryBindInventory();
    }

    private void OnEnable()
    {
        TryBindInventory();
    }

    private void OnDisable()
    {
     
        if (cachedInventory != null)
            cachedInventory.OnHealthKitsChanged -= OnHealthKitsChanged;
       
    }

    
    private void TryBindInventory()
    {
       
        if (cachedInventory != null) return;

        if (Inventory.Instance != null)
            cachedInventory = Inventory.Instance;
        else
            cachedInventory = FindFirstObjectByType<Inventory>();

        if (cachedInventory == null) return;

       
        cachedInventory.OnHealthKitsChanged -= OnHealthKitsChanged;
        cachedInventory.OnHealthKitsChanged += OnHealthKitsChanged;

       
        UpdateHealthKitText(cachedInventory.GetHealthKitCount(), cachedInventory.GetMaxHealthKits());
    }

    private void OnHealthKitsChanged(int current, int max)
    {
        UpdateHealthKitText(current, max);
    }
   

    private void Update()
    {
      
        if (healthBarFront != null)
        {
            float v = healthBarFront.value;
            v = Mathf.Lerp(v, healthTarget01, Time.deltaTime * frontSmoothSpeed);
            healthBarFront.value = v;
        }
    }

  
    public void UpdateAmmoText(int currentAmmo, int remainingAmmo)
    {
        if (ammoText != null) ammoText.text = currentAmmo.ToString();
        if (remainingAmmoText != null) remainingAmmoText.text = "/" + remainingAmmo.ToString();
    }

   
    public void UpdateHealthText(float currentHealth)
    {
        if (healthText != null)
            healthText.text = "Health:" + Mathf.RoundToInt(currentHealth);
    }

    public void UpdateHealthKitText(int current, int max)
    {
        if (healthKitText != null)
            healthKitText.text = current + "/" + max;
    }

    
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0f) maxHealth = 1f;

        float t01 = Mathf.Clamp01(currentHealth / maxHealth);
        healthTarget01 = t01;

       
        if (healthBarBack != null)
        {
            if (healthBarBack.value < t01)
            {
                healthBarBack.value = t01;

                if (backRoutine != null)
                {
                    StopCoroutine(backRoutine);
                    backRoutine = null;
                }
            }
            else
            {
                if (backRoutine != null)
                    StopCoroutine(backRoutine);

                backRoutine = StartCoroutine(BackBarCatchupRoutine(t01));
            }
        }
    }

    
    public void SetHealthBarInstant(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0f) maxHealth = 1f;

        float t01 = Mathf.Clamp01(currentHealth / maxHealth);
        healthTarget01 = t01;

        if (backRoutine != null)
        {
            StopCoroutine(backRoutine);
            backRoutine = null;
        }

        if (healthBarFront != null) healthBarFront.value = t01;
        if (healthBarBack != null) healthBarBack.value = t01;
    }

    private IEnumerator BackBarCatchupRoutine(float target01)
    {
        yield return new WaitForSeconds(backCatchupDelay);

        while (healthBarBack != null && healthBarBack.value > target01 + 0.001f)
        {
            healthBarBack.value = Mathf.MoveTowards(
                healthBarBack.value,
                target01,
                Time.deltaTime * backCatchupSpeed
            );
            yield return null;
        }

        if (healthBarBack != null)
            healthBarBack.value = target01;

        backRoutine = null;
    }

  
    public void PlayDamageFlash()
    {
        if (damageOverlayImage == null) return;

        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        damageRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        Color c = damageOverlayImage.color;
        c.a = damageFlashMaxAlpha;
        damageOverlayImage.color = c;

        while (damageOverlayImage != null && damageOverlayImage.color.a > 0f)
        {
            c = damageOverlayImage.color;
            c.a -= Time.deltaTime * damageFadeSpeed;
            damageOverlayImage.color = c;
            yield return null;
        }

        if (damageOverlayImage != null)
        {
            c = damageOverlayImage.color;
            c.a = 0f;
            damageOverlayImage.color = c;
        }

        damageRoutine = null;
    }
}