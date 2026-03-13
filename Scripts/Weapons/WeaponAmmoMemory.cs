using UnityEngine;

public class WeaponAmmoMemory : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private WeaponsController pistol;
    [SerializeField] private WeaponsController autoGun;

   
    private int pistolClipAmmo;
    private int autoClipAmmo;

    private bool initialized;

    public void InitFromWeapons()
    {
        if (initialized) return;
        initialized = true;

        if (pistol != null) pistolClipAmmo = pistol.currentAmmo;
        if (autoGun != null) autoClipAmmo = autoGun.currentAmmo;
    }

    public void SaveCurrent(WeaponsController current)
    {
        if (current == null) return;

        if (current == pistol) pistolClipAmmo = pistol.currentAmmo;
        if (current == autoGun) autoClipAmmo = autoGun.currentAmmo;
    }

    public void LoadTo(WeaponsController weapon)
    {
        if (weapon == null) return;

        if (weapon == pistol) weapon.currentAmmo = pistolClipAmmo;
        if (weapon == autoGun) weapon.currentAmmo = autoClipAmmo;
    }
}
