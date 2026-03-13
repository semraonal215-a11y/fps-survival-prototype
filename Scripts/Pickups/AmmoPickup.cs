using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 30;

    
    public AudioClip pickupSFX;
    [Range(0f, 1f)] public float pickupSFXVolume = 1f;

    private bool picked;

  
    private void OnEnable()
    {
        picked = false;

        
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        
        this.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (picked) return;
        if (!other.CompareTag("Player")) return;

        if (Inventory.Instance == null)
        {
            Debug.LogError($"{name}: Inventory.Instance NULL");
            return;
        }

        picked = true;

        Inventory.Instance.AddAmmo(ammoAmount);

        
        var ui = FindFirstObjectByType<UIController>();
        if (ui != null)
        {
            var weapon = FindFirstObjectByType<WeaponsController>();
            int currentAmmo = (weapon != null) ? weapon.currentAmmo : 0;
            ui.UpdateAmmoText(currentAmmo, Inventory.Instance.GetAmmoCount());
        }

        
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

       
        if (pickupSFX != null)
            AudioSource.PlayClipAtPoint(pickupSFX, transform.position, pickupSFXVolume);

        
        if (PickupPool.Instance != null)
            PickupPool.Instance.Return(gameObject);
        else
            Destroy(gameObject);
    }
}
