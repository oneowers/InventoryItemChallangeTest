using System.IO;
using UnityEngine;

public sealed class JsonInventoryRepository : IInventoryRepository
{
    private const string FileName = "inventory.json";

    private readonly string savePath;

    public JsonInventoryRepository()
    {
        this.savePath = Path.Combine(Application.persistentDataPath, FileName);
    }

    public void Save(InventoryModel model)
    {
        InventorySaveData saveData = new InventorySaveData();
        saveData.coins = model.Coins;

        for (int index = 0; index < model.Slots.Count; index++)
        {
            SlotModel slot = model.Slots[index];
            if (slot == null)
            {
                continue;
            }

            SlotSaveData slotSaveData = new SlotSaveData();
            slotSaveData.slotIndex = slot.Index;
            slotSaveData.isUnlocked = slot.IsUnlocked;
            slotSaveData.itemId = slot.Item == null ? string.Empty : slot.Item.ItemId;
            slotSaveData.quantity = slot.Item == null ? 0 : slot.Quantity;
            saveData.slots.Add(slotSaveData);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(this.savePath, json);
    }

    public InventorySaveData Load()
    {
        if (!File.Exists(this.savePath))
        {
            return null;
        }

        string json = File.ReadAllText(this.savePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonUtility.FromJson<InventorySaveData>(json);
    }
}
