using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;
    public static event Action OnPlayerDied;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    public bool isDead { get; private set; }

   
    [Header("SFX")]
    public AudioSource deathSfxSource;
    public AudioClip deathSfx;
    [Range(0f, 2f)] public float deathSfxVolume = 1f;
    

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (UIController.instance != null)
        {
            UIController.instance.UpdateHealthText(currentHealth);
            UIController.instance.SetHealthBarInstant(currentHealth, maxHealth);
        }

        
        if (deathSfxSource == null)
            deathSfxSource = GetComponent<AudioSource>();

        if (deathSfxSource == null)
            deathSfxSource = gameObject.AddComponent<AudioSource>();

        deathSfxSource.playOnAwake = false;
        deathSfxSource.loop = false;
        deathSfxSource.spatialBlend = 0f; // 2D
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (CameraShake.Instance != null)
        {
            float mag = Mathf.Clamp(damage / 100f, 0.03f, 0.12f);
            CameraShake.Instance.Shake(mag, 0.12f);
        }

        if (UIController.instance != null)
        {
            UIController.instance.UpdateHealthText(currentHealth);
            UIController.instance.UpdateHealthBar(currentHealth, maxHealth);
            UIController.instance.PlayDamageFlash();
        }

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (UIController.instance != null)
        {
            UIController.instance.UpdateHealthText(currentHealth);
            UIController.instance.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0f;

        if (UIController.instance != null)
        {
            UIController.instance.UpdateHealthText(currentHealth);
            UIController.instance.SetHealthBarInstant(currentHealth, maxHealth);
        }

       
        if (deathSfx != null && deathSfxSource != null)
            deathSfxSource.PlayOneShot(deathSfx, deathSfxVolume);
     

        OnPlayerDied?.Invoke();
    }
}
