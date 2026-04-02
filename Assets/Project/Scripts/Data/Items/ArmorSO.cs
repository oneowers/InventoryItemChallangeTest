using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "Inventory/Items/Armor")]
public sealed class ArmorSO : BaseItemSO
{
    [SerializeField]
    private ArmorSlot armorSlot;

    [SerializeField]
    private int defenseValue;

    public ArmorSlot ArmorSlot
    {
        get
        {
            return this.armorSlot;
        }
    }

    public int DefenseValue
    {
        get
        {
            return this.defenseValue;
        }
    }
}
