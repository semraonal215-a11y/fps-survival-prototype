using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float healAmount = 25f; 
    public int kitAmount = 1;


    public AudioClip pickupSFX;
    [Range(0f, 1f)] public float pickupSFXVolume = 1f;

    private bool picked;
    private Collider pickupCollider;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider>();
    }

 
    private void OnEnable()
    {
        picked = false;

        if (pickupCollider == null)
            pickupCollider = GetComponent<Collider>();

        if (pickupCollider != null) pickupCollider.enabled = true;

        
        this.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (picked) return;

        Debug.Log("Trigger girdi: " + other.name);

        if (other.tag == "Player")
        {
            Debug.Log("PLAYER ALGILANDI");

            if (Inventory.Instance == null)
            {
                Debug.LogError($"{name}: Inventory.Instance NULL (Player üzerinde Inventory var mý?)");
                return;
            }

          
            bool added = Inventory.Instance.TryAddHealthKit(kitAmount);

           
            var ui = UIController.instance;
            if (ui != null)
                ui.UpdateHealthKitText(Inventory.Instance.GetHealthKitCount(),
                                       Inventory.Instance.GetMaxHealthKits());

            
            if (!added) return;

            picked = true;

            if (pickupCollider != null) pickupCollider.enabled = false;

           
            if (pickupSFX != null)
                AudioSource.PlayClipAtPoint(pickupSFX, transform.position, pickupSFXVolume);

           
            if (PickupPool.Instance != null)
                PickupPool.Instance.Return(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
