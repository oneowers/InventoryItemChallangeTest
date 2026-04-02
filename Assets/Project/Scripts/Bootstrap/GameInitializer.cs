using Project.UI;
using UnityEngine;

public sealed class GameInitializer : MonoBehaviour
{
    [SerializeField]
    private GameDatabaseSO database;

    [SerializeField]
    private InventoryConfigSO config;

    [SerializeField]
    private InventoryView inventoryView;

    [SerializeField]
    private HUDView hudView;

    [SerializeField]
    private SlotMachineView slotMachineView;

    [Header("Target Shooting")]
    [SerializeField]
    private TargetShootingUI targetShootingUI;

    [SerializeField]
    private int spinCost = 50;

    private void Awake()
    {
        // В Unity Console можно вывести путь:
        Debug.Log(Application.persistentDataPath);

        JsonInventoryRepository repository = new JsonInventoryRepository();
        InventorySaveData saveData = repository.Load();
        InventoryModel model = this.CreateModelWithSlots(this.config.TotalSlots);

        if (saveData == null)
        {
            this.CreateDefaultState(model);
            Debug.Log("[Inventory] First launch: default state created");
            repository.Save(model);
        }
        else
        {
            this.RestoreState(model, saveData);
        }

        InventoryService inventoryService = new InventoryService(model, this.database, this.config, repository);
        InventoryPresenter presenter = new InventoryPresenter(
            model, inventoryService, this.inventoryView, this.hudView, this.targetShootingUI);
        presenter.Initialize();
        this.inventoryView.InitializeDragDrop(inventoryService);

        SlotMachineService slotMachineService = new SlotMachineService(inventoryService, this.database, this.spinCost);
        SlotMachinePresenter slotMachinePresenter = new SlotMachinePresenter(slotMachineService, this.slotMachineView, this.hudView);
        slotMachinePresenter.Initialize();
    }

    private InventoryModel CreateModelWithSlots(int totalSlots)
    {
        InventoryModel model = new InventoryModel();

        for (int index = 0; index < totalSlots; index++)
        {
            SlotModel slot = new SlotModel
            {
                Index = index,
                IsUnlocked = false,
                Item = null,
                Quantity = 0
            };

            model.Slots.Add(slot);
        }

        return model;
    }

    private void CreateDefaultState(InventoryModel model)
    {
        int unlockedSlots = Mathf.Min(this.config.DefaultUnlockedSlots, model.Slots.Count);

        for (int index = 0; index < unlockedSlots; index++)
        {
            SlotModel slot = model.Slots[index];
            slot.IsUnlocked = true;
            slot.Item = null;
            slot.Quantity = 0;
        }

        model.Coins = 0;
    }

    private void RestoreState(InventoryModel model, InventorySaveData saveData)
    {
        model.Coins = saveData.coins;

        if (saveData.slots == null)
        {
            return;
        }

        for (int index = 0; index < saveData.slots.Count; index++)
        {
            SlotSaveData slotSaveData = saveData.slots[index];
            if (slotSaveData == null)
            {
                continue;
            }

            if (slotSaveData.slotIndex < 0 || slotSaveData.slotIndex >= model.Slots.Count)
            {
                continue;
            }

            SlotModel slot = model.Slots[slotSaveData.slotIndex];
            slot.IsUnlocked = slotSaveData.isUnlocked;
            slot.Item = null;
            slot.Quantity = 0;

            if (string.IsNullOrWhiteSpace(slotSaveData.itemId))
            {
                continue;
            }

            BaseItemSO item = this.database.GetById(slotSaveData.itemId);
            if (item == null)
            {
                Debug.LogWarning($"[Inventory] Warning: itemId '{slotSaveData.itemId}' not found in database, slot {slotSaveData.slotIndex} will be empty");
                continue;
            }

            slot.Item = item;
            slot.Quantity = slotSaveData.quantity;
        }
    }
}
