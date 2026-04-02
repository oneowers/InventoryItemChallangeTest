using UnityEngine;

[CreateAssetMenu(fileName = "InventoryConfig", menuName = "Inventory/Items/Inventory Config")]
public sealed class InventoryConfigSO : ScriptableObject
{
    [SerializeField]
    private int totalSlots = 30;

    [SerializeField]
    private int defaultUnlockedSlots = 15;

    [SerializeField]
    private int slotUnlockCost = 100;

    public int TotalSlots
    {
        get
        {
            return this.totalSlots;
        }
    }

    public int DefaultUnlockedSlots
    {
        get
        {
            return this.defaultUnlockedSlots;
        }
    }

    public int SlotUnlockCost
    {
        get
        {
            return this.slotUnlockCost;
        }
    }
}
