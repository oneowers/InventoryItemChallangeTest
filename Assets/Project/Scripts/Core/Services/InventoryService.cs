using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class InventoryService
{
    private readonly InventoryModel model;
    private readonly GameDatabaseSO database;
    private readonly InventoryConfigSO config;
    private readonly IInventoryRepository repository;


    public InventoryService(
        InventoryModel model,
        GameDatabaseSO database,
        InventoryConfigSO config,
        IInventoryRepository repository)
    {
        this.model = model;
        this.database = database;
        this.config = config;
        this.repository = repository;
    }

    public void ShootRandom()
    {
        List<WeaponAmmoPair> validPairs = new List<WeaponAmmoPair>();

        for (int weaponIndex = 0; weaponIndex < this.model.Slots.Count; weaponIndex++)
        {
            SlotModel weaponSlot = this.model.Slots[weaponIndex];
            if (weaponSlot == null || weaponSlot.IsEmpty || !weaponSlot.IsUnlocked)
            {
                continue;
            }

            WeaponSO weapon = weaponSlot.Item as WeaponSO;
            if (weapon == null)
            {
                continue;
            }

            ItemType expectedAmmoType = this.GetAmmoItemType(weapon.CompatibleAmmo);

            for (int ammoIndex = 0; ammoIndex < this.model.Slots.Count; ammoIndex++)
            {
                SlotModel ammoSlot = this.model.Slots[ammoIndex];
                if (ammoSlot == null || ammoSlot.IsEmpty || !ammoSlot.IsUnlocked)
                {
                    continue;
                }

                if (ammoSlot.Item.ItemType == expectedAmmoType && ammoSlot.Quantity > 0)
                {
                    WeaponAmmoPair pair = new WeaponAmmoPair(weaponSlot, ammoSlot);
                    validPairs.Add(pair);
                    break;
                }
            }
        }

        if (validPairs.Count == 0)
        {
            Debug.LogError("[Inventory] Error: no valid weapon+ammo pair found");
            return;
        }

        WeaponAmmoPair selectedPair = validPairs[Random.Range(0, validPairs.Count)];
        SlotModel selectedAmmoSlot = selectedPair.AmmoSlot;
        selectedAmmoSlot.Quantity--;
        if (selectedAmmoSlot.Quantity == 0)
        {
            selectedAmmoSlot.Item = null;
            selectedAmmoSlot.Quantity = 0;
        }

        WeaponSO selectedWeapon = (WeaponSO)selectedPair.WeaponSlot.Item;
        Debug.Log($"[Inventory] Shot: {selectedWeapon.DisplayName} -> {selectedWeapon.CompatibleAmmo} | Damage: {selectedWeapon.DamagePerShot}");

        this.model.NotifySlotChanged(selectedAmmoSlot);
        this.repository.Save(this.model);
        this.model.NotifyWeightChanged();
    }

    public void AddAmmo()
    {
        this.AddAmmoByType(ItemType.PistolAmmo, 30);
        this.AddAmmoByType(ItemType.RifleAmmo, 30);
    }

    public void AddAmmo(AmmoType ammoType, int amount)
    {
        this.AddAmmoByType(this.GetAmmoItemType(ammoType), amount);
    }

    public void AddRandomItem()
    {
        List<BaseItemSO> availableItems = new List<BaseItemSO>();

        for (int index = 0; index < this.database.AllItems.Count; index++)
        {
            BaseItemSO item = this.database.AllItems[index];
            if (item == null)
            {
                continue;
            }

            if (item.ItemType == ItemType.PistolAmmo || item.ItemType == ItemType.RifleAmmo)
            {
                continue;
            }

            availableItems.Add(item);
        }

        if (availableItems.Count == 0)
        {
            Debug.LogError("[Inventory] Error: no items available in database");
            return;
        }

        BaseItemSO selectedItem = availableItems[Random.Range(0, availableItems.Count)];
        SlotModel emptySlot = this.FindFirstEmptyUnlockedSlot();
        if (emptySlot == null)
        {
            Debug.LogError("[Inventory] Error: no free slots for item");
            return;
        }

        emptySlot.Item = selectedItem;
        emptySlot.Quantity = 1;

        Debug.Log($"[Inventory] Added: {selectedItem.DisplayName} -> slot {emptySlot.Index}");
        this.model.NotifySlotChanged(emptySlot);
        this.repository.Save(this.model);
        this.model.NotifyWeightChanged();
    }

    public void RemoveRandomItem()
    {
        List<SlotModel> occupiedSlots = new List<SlotModel>();

        for (int index = 0; index < this.model.Slots.Count; index++)
        {
            SlotModel slot = this.model.Slots[index];
            if (slot != null && !slot.IsEmpty)
            {
                occupiedSlots.Add(slot);
            }
        }

        if (occupiedSlots.Count == 0)
        {
            Debug.LogError("[Inventory] Error: all slots are empty");
            return;
        }

        SlotModel selectedSlot = occupiedSlots[Random.Range(0, occupiedSlots.Count)];
        Debug.Log($"[Inventory] Removed: {selectedSlot.Item.DisplayName} x{selectedSlot.Quantity} from slot {selectedSlot.Index}");

        selectedSlot.Item = null;
        selectedSlot.Quantity = 0;

        this.model.NotifySlotChanged(selectedSlot);
        this.repository.Save(this.model);
        this.model.NotifyWeightChanged();
    }

    public void AddCoins(int amount)
    {
        this.model.Coins += amount;
        this.model.NotifyCoinsChanged();
        this.repository.Save(this.model);
    }

    /// <summary>
    /// Списывает 1 патрон (PistolAmmo или RifleAmmo) из инвентаря.
    /// Возвращает true если патрон найден и списан, иначе false.
    /// </summary>
    public bool TryConsumeBullet()
    {
        for (int index = 0; index < this.model.Slots.Count; index++)
        {
            SlotModel slot = this.model.Slots[index];
            if (slot == null || slot.IsEmpty || !slot.IsUnlocked)
            {
                continue;
            }

            if (slot.Item.ItemType != ItemType.PistolAmmo && slot.Item.ItemType != ItemType.RifleAmmo)
            {
                continue;
            }

            slot.Quantity--;

            if (slot.Quantity == 0)
            {
                slot.Item = null;
                slot.Quantity = 0;
            }

            Debug.Log($"[InventoryService] Consumed 1 bullet from slot {slot.Index} for target shooting");
            this.model.NotifySlotChanged(slot);
            this.repository.Save(this.model);
            this.model.NotifyWeightChanged();
            return true;
        }

        Debug.LogWarning("[InventoryService] No bullets available for target shooting");
        return false;
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (this.model.Coins < amount)
        {
            Debug.LogError("[Inventory] Error: not enough coins for spin");
            return false;
        }

        this.model.Coins -= amount;
        this.model.NotifyCoinsChanged();
        this.repository.Save(this.model);
        return true;
    }

    public void AddItem(BaseItemSO item, int quantity)
    {
        if (item == null || quantity <= 0)
        {
            Debug.LogError("[Inventory] Error: invalid item add request");
            return;
        }

        int remaining = quantity;

        for (int index = 0; index < this.model.Slots.Count; index++)
        {
            if (remaining <= 0)
            {
                break;
            }

            SlotModel slot = this.model.Slots[index];
            if (slot == null || !slot.IsUnlocked || slot.IsEmpty)
            {
                continue;
            }

            if (slot.Item.ItemId != item.ItemId || slot.Quantity >= slot.Item.MaxStackSize)
            {
                continue;
            }

            int availableSpace = slot.Item.MaxStackSize - slot.Quantity;
            int fill = Mathf.Min(remaining, availableSpace);
            slot.Quantity += fill;
            remaining -= fill;

            Debug.Log($"[Inventory] Added: {fill} {item.DisplayName} -> slot {slot.Index}");
            this.model.NotifySlotChanged(slot);
        }

        while (remaining > 0)
        {
            SlotModel emptySlot = this.FindFirstEmptyUnlockedSlot();
            if (emptySlot == null)
            {
                Debug.LogError($"[Inventory] Error: no free slots for {item.DisplayName}");
                break;
            }

            emptySlot.Item = item;
            emptySlot.Quantity = Mathf.Min(remaining, item.MaxStackSize);
            remaining -= emptySlot.Quantity;

            Debug.Log($"[Inventory] Added: {emptySlot.Quantity} {item.DisplayName} -> slot {emptySlot.Index}");
            this.model.NotifySlotChanged(emptySlot);
        }

        this.repository.Save(this.model);
        this.model.NotifyWeightChanged();
    }

    public void MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex)
        {
            return;
        }

        SlotModel sourceSlot = this.GetSlotByIndex(fromIndex);
        SlotModel targetSlot = this.GetSlotByIndex(toIndex);

        if (sourceSlot == null || targetSlot == null)
        {
            Debug.LogError("[Inventory] Error: slot move failed due to invalid index");
            return;
        }

        if (!sourceSlot.IsUnlocked || !targetSlot.IsUnlocked)
        {
            Debug.LogError("[Inventory] Error: locked slots cannot be moved");
            return;
        }

        if (sourceSlot.IsEmpty)
        {
            return;
        }

        if (targetSlot.IsEmpty)
        {
            targetSlot.Item = sourceSlot.Item;
            targetSlot.Quantity = sourceSlot.Quantity;
            sourceSlot.Item = null;
            sourceSlot.Quantity = 0;
        }
        else if (this.CanMergeAmmoStacks(sourceSlot, targetSlot))
        {
            int maxStackSize = targetSlot.Item.MaxStackSize;
            int availableSpace = maxStackSize - targetSlot.Quantity;
            int movedQuantity = Mathf.Min(sourceSlot.Quantity, availableSpace);

            targetSlot.Quantity += movedQuantity;
            sourceSlot.Quantity -= movedQuantity;

            if (sourceSlot.Quantity == 0)
            {
                sourceSlot.Item = null;
                sourceSlot.Quantity = 0;
            }
        }
        else
        {
            BaseItemSO tempItem = targetSlot.Item;
            int tempQuantity = targetSlot.Quantity;

            targetSlot.Item = sourceSlot.Item;
            targetSlot.Quantity = sourceSlot.Quantity;

            sourceSlot.Item = tempItem;
            sourceSlot.Quantity = tempQuantity;
        }

        this.model.NotifySlotChanged(sourceSlot);
        this.model.NotifySlotChanged(targetSlot);
        this.repository.Save(this.model);
        this.model.NotifyWeightChanged();
    }

    public bool TryUnlockSlot(int slotIndex)
    {
        SlotModel slot = this.GetSlotByIndex(slotIndex);
        if (slot == null)
        {
            Debug.LogError($"[Inventory] Error: slot {slotIndex} not found");
            return false;
        }

        if (this.model.Coins < this.config.SlotUnlockCost)
        {
            Debug.LogError("[Inventory] Error: not enough coins to unlock slot");
            return false;
        }

        this.model.Coins -= this.config.SlotUnlockCost;
        slot.IsUnlocked = true;

        this.model.NotifySlotChanged(slot);
        this.model.NotifyCoinsChanged();
        this.repository.Save(this.model);
        this.model.NotifyWeightChanged();
        return true;
    }

    private void AddAmmoByType(ItemType ammoType, int amount)
    {
        BaseItemSO ammoItem = this.FindItemByType(ammoType);
        if (ammoItem == null)
        {
            Debug.LogError($"[Inventory] Error: ammo item not found for {ammoType}");
            return;
        }

        int remaining = amount;

        for (int index = 0; index < this.model.Slots.Count; index++)
        {
            if (remaining <= 0)
            {
                break;
            }

            SlotModel slot = this.model.Slots[index];
            if (slot == null || !slot.IsUnlocked || slot.IsEmpty)
            {
                continue;
            }

            if (slot.Item.ItemType != ammoType || slot.Quantity >= slot.Item.MaxStackSize)
            {
                continue;
            }

            int availableSpace = slot.Item.MaxStackSize - slot.Quantity;
            int fill = Mathf.Min(remaining, availableSpace);
            slot.Quantity += fill;
            remaining -= fill;

            Debug.Log($"[Inventory] Added: {fill} {ammoType} -> slot {slot.Index}");
            this.model.NotifySlotChanged(slot);
        }

        while (remaining > 0)
        {
            SlotModel emptySlot = this.FindFirstEmptyUnlockedSlot();
            if (emptySlot == null)
            {
                Debug.LogError($"[Inventory] Error: no free slots for {ammoType}");
                break;
            }

            emptySlot.Item = ammoItem;
            emptySlot.Quantity = Mathf.Min(remaining, ammoItem.MaxStackSize);
            remaining -= emptySlot.Quantity;

            Debug.Log($"[Inventory] Added: {emptySlot.Quantity} {ammoType} -> slot {emptySlot.Index}");
            this.model.NotifySlotChanged(emptySlot);
        }

        this.repository.Save(this.model);
        this.model.NotifyWeightChanged();
    }

    private BaseItemSO FindItemByType(ItemType itemType)
    {
        for (int index = 0; index < this.database.AllItems.Count; index++)
        {
            BaseItemSO item = this.database.AllItems[index];
            if (item != null && item.ItemType == itemType)
            {
                return item;
            }
        }

        return null;
    }

    private SlotModel FindFirstEmptyUnlockedSlot()
    {
        for (int index = 0; index < this.model.Slots.Count; index++)
        {
            SlotModel slot = this.model.Slots[index];
            if (slot != null && slot.IsUnlocked && slot.IsEmpty)
            {
                return slot;
            }
        }

        return null;
    }

    private SlotModel GetSlotByIndex(int slotIndex)
    {
        for (int index = 0; index < this.model.Slots.Count; index++)
        {
            SlotModel slot = this.model.Slots[index];
            if (slot != null && slot.Index == slotIndex)
            {
                return slot;
            }
        }

        return null;
    }

    private bool CanMergeAmmoStacks(SlotModel sourceSlot, SlotModel targetSlot)
    {
        if (sourceSlot.Item == null || targetSlot.Item == null)
        {
            return false;
        }

        bool isSourceAmmo = sourceSlot.Item.ItemType == ItemType.PistolAmmo || sourceSlot.Item.ItemType == ItemType.RifleAmmo;
        bool isTargetAmmo = targetSlot.Item.ItemType == ItemType.PistolAmmo || targetSlot.Item.ItemType == ItemType.RifleAmmo;

        if (!isSourceAmmo || !isTargetAmmo)
        {
            return false;
        }

        if (sourceSlot.Item.ItemType != targetSlot.Item.ItemType)
        {
            return false;
        }

        return targetSlot.Quantity < targetSlot.Item.MaxStackSize;
    }

    private ItemType GetAmmoItemType(AmmoType ammoType)
    {
        switch (ammoType)
        {
            case AmmoType.Pistol:
                return ItemType.PistolAmmo;
            case AmmoType.Rifle:
                return ItemType.RifleAmmo;
            default:
                return ItemType.PistolAmmo;
        }
    }

    private readonly struct WeaponAmmoPair
    {
        public WeaponAmmoPair(SlotModel weaponSlot, SlotModel ammoSlot)
        {
            this.WeaponSlot = weaponSlot;
            this.AmmoSlot = ammoSlot;
        }

        public SlotModel WeaponSlot { get; }

        public SlotModel AmmoSlot { get; }
    }
}
