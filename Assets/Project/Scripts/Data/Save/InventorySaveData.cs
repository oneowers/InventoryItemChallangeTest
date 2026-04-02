using System;
using System.Collections.Generic;

[Serializable]
public sealed class InventorySaveData
{
    public int coins;
    public List<SlotSaveData> slots = new List<SlotSaveData>();
}
