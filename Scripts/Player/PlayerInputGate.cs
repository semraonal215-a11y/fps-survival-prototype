using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputGate : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Optional: Disable these scripts too ")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable; 

    [Header("Optional: Disable active weapon controller ")]
    [SerializeField] private WeaponsController currentWeapon;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
            Debug.LogError("PlayerInputGate: PlayerInput component bulunamadý! Player objesinde PlayerInput var mý?");

        
        if (currentWeapon == null)
            currentWeapon = FindFirstObjectByType<WeaponsController>();
    }

    public void SetInputEnabled(bool enabled)
    {
       
        if (playerInput != null)
        {
            if (enabled) playerInput.ActivateInput();
            else playerInput.DeactivateInput();
        }

       
        if (scriptsToDisable != null)
        {
            for (int i = 0; i < scriptsToDisable.Length; i++)
            {
                if (scriptsToDisable[i] == null) continue;
                scriptsToDisable[i].enabled = enabled;
            }
        }

     
        if (currentWeapon != null)
            currentWeapon.enabled = enabled;
    }

   
    public void SetCurrentWeapon(WeaponsController wc)
    {
        currentWeapon = wc;
    }
}
