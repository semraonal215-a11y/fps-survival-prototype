using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    [Header("Inventory")]
    [SerializeField] private int healthKitCount = 0;
    [SerializeField] private int ammoCount = 0;

    [Header("Limits")]
    [SerializeField] private int maxHealthKits = 5;

   
    [Header("Ammo Limit")]
    [SerializeField] private int maxAmmo = 180; 
    public int GetMaxAmmo() => maxAmmo;
   

    [Header("Health Kit")]
    public float healthKitHealAmount = 20f;
    public float GetHealthKitHealAmount() => healthKitHealAmount;

    public event Action<int, int> OnHealthKitsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("Inventory Instance set");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        OnHealthKitsChanged?.Invoke(healthKitCount, maxHealthKits);
    }

    public int GetHealthKitCount()
    {
        return healthKitCount;
    }

    public int GetMaxHealthKits()
    {
        return maxHealthKits;
    }

    public void AddHealthKit(int amount = 1)
    {
        TryAddHealthKit(amount);
    }

    public bool TryAddHealthKit(int amount = 1)
    {
        if (amount <= 0) return false;

        if (healthKitCount >= maxHealthKits)
        {
            Debug.Log("Health kit have been max value (" + maxHealthKits + ")");
            OnHealthKitsChanged?.Invoke(healthKitCount, maxHealthKits);
            return false;
        }

        int space = maxHealthKits - healthKitCount;
        int add = Mathf.Min(space, amount);

        healthKitCount += add;

        Debug.Log("Health kit added. Total: " + healthKitCount + "/" + maxHealthKits);
        OnHealthKitsChanged?.Invoke(healthKitCount, maxHealthKits);

        return true;
    }

    public bool UseHealthKit()
    {
        if (healthKitCount <= 0) return false;

        healthKitCount--;

        Debug.Log("Health kit used. Rest: " + healthKitCount + "/" + maxHealthKits);
        OnHealthKitsChanged?.Invoke(healthKitCount, maxHealthKits);

        return true;
    }

    public int GetAmmoCount()
    {
        return ammoCount;
    }

    
    public void AddAmmo(int amount)
    {
        if (amount <= 0) return;

        int before = ammoCount;

        
        ammoCount = Mathf.Clamp(ammoCount + amount, 0, maxAmmo);

        int added = ammoCount - before;

        if (added <= 0)
        {
            Debug.Log("Ammo is already full (" + ammoCount + "/" + maxAmmo + ")");
            return;
        }

        Debug.Log("Ammo added: +" + added + " Total: " + ammoCount + "/" + maxAmmo);
    }
   

    public bool UseAmmo(int amount)
    {
        if (amount <= 0) return false;
        if (ammoCount < amount) return false;

        ammoCount -= amount;
        return true;
    }
}