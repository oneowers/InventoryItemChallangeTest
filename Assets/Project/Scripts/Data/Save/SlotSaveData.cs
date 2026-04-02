using System;

[Serializable]
public sealed class SlotSaveData
{
    public int slotIndex;
    public bool isUnlocked;
    public string itemId = string.Empty;
    public int quantity;
}
