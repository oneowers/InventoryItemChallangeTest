using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Inventory/Items/Weapon")]
public sealed class WeaponSO : BaseItemSO
{
    [SerializeField]
    private AmmoType compatibleAmmo;

    [SerializeField]
    private int damagePerShot;

    public AmmoType CompatibleAmmo
    {
        get
        {
            return this.compatibleAmmo;
        }
    }

    public int DamagePerShot
    {
        get
        {
            return this.damagePerShot;
        }
    }
}
