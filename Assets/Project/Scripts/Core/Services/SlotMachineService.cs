using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class SlotMachineService
{
    private const int ReelCount = 3;
    private const int JackpotCoinsReward = 200;
    private const int AmmoRewardAmount = 30;
    private const int JackpotChanceDivisor = 20;

    private readonly InventoryService inventoryService;
    private readonly GameDatabaseSO database;
    private readonly int spinCost;

    public SlotMachineService(InventoryService inventoryService, GameDatabaseSO database, int spinCost)
    {
        this.inventoryService = inventoryService;
        this.database = database;
        this.spinCost = spinCost;
    }

    public int SpinCost
    {
        get
        {
            return this.spinCost;
        }
    }

    public List<BaseItemSO> GetSpinSymbols()
    {
        List<BaseItemSO> symbols = new List<BaseItemSO>();

        for (int index = 0; index < this.database.AllItems.Count; index++)
        {
            BaseItemSO item = this.database.AllItems[index];
            if (item != null)
            {
                symbols.Add(item);
            }
        }

        if (this.database.JackpotSymbol != null)
        {
            symbols.Add(this.database.JackpotSymbol);
        }

        return symbols;
    }

    public SpinResult GetSpinResult()
    {
        BaseItemSO first = this.RollSymbol();
        BaseItemSO second = this.RollSymbol();
        BaseItemSO third = this.RollSymbol();
        return new SpinResult(first, second, third);
    }

    public bool TryStartSpin()
    {
        return this.inventoryService.TrySpendCoins(this.spinCost);
    }

    public void ApplyResult(SpinResult result)
    {
        if (result == null)
        {
            return;
        }

        if (result.Contains(this.database.JackpotSymbol))
        {
            this.inventoryService.AddCoins(JackpotCoinsReward);
            Debug.Log($"[Inventory] Spin: jackpot -> {JackpotCoinsReward} coins");
            return;
        }

        BaseItemSO tripleMatch = result.GetTripleMatch();
        if (tripleMatch != null)
        {
            this.ApplyMatchedReward(tripleMatch);
            return;
        }

        BaseItemSO pairMatch = result.GetPairMatch();
        if (pairMatch != null)
        {
            this.ApplyPairReward(pairMatch);
            return;
        }

        AmmoType randomAmmoType = Random.Range(0, ReelCount) % 2 == 0 ? AmmoType.Pistol : AmmoType.Rifle;
        this.inventoryService.AddAmmo(randomAmmoType, AmmoRewardAmount);
        Debug.Log($"[Inventory] Spin: all different -> {AmmoRewardAmount} {randomAmmoType} ammo");
    }

    private void ApplyMatchedReward(BaseItemSO matchedItem)
    {
        if (matchedItem.ItemType == ItemType.PistolAmmo)
        {
            this.inventoryService.AddAmmo(AmmoType.Pistol, AmmoRewardAmount);
            Debug.Log($"[Inventory] Spin: triple match -> {AmmoRewardAmount} {AmmoType.Pistol} ammo");
            return;
        }

        if (matchedItem.ItemType == ItemType.RifleAmmo)
        {
            this.inventoryService.AddAmmo(AmmoType.Rifle, AmmoRewardAmount);
            Debug.Log($"[Inventory] Spin: triple match -> {AmmoRewardAmount} {AmmoType.Rifle} ammo");
            return;
        }

        this.inventoryService.AddItem(matchedItem, 1);
        Debug.Log($"[Inventory] Spin: triple match -> {matchedItem.DisplayName}");
    }

    private void ApplyPairReward(BaseItemSO matchedItem)
    {
        if (matchedItem.ItemType == ItemType.PistolAmmo)
        {
            this.inventoryService.AddAmmo(AmmoType.Pistol, AmmoRewardAmount);
            Debug.Log($"[Inventory] Spin: pair match -> {AmmoRewardAmount} {AmmoType.Pistol} ammo");
            return;
        }

        if (matchedItem.ItemType == ItemType.RifleAmmo)
        {
            this.inventoryService.AddAmmo(AmmoType.Rifle, AmmoRewardAmount);
            Debug.Log($"[Inventory] Spin: pair match -> {AmmoRewardAmount} {AmmoType.Rifle} ammo");
            return;
        }

        BaseItemSO rewardItem = this.GetRandomItemByType(matchedItem.ItemType);
        if (rewardItem == null)
        {
            return;
        }

        this.inventoryService.AddItem(rewardItem, 1);
        Debug.Log($"[Inventory] Spin: pair match -> {rewardItem.DisplayName}");
    }

    private BaseItemSO GetRandomItemByType(ItemType itemType)
    {
        List<BaseItemSO> items = new List<BaseItemSO>();

        for (int index = 0; index < this.database.AllItems.Count; index++)
        {
            BaseItemSO item = this.database.AllItems[index];
            if (item != null && item.ItemType == itemType)
            {
                items.Add(item);
            }
        }

        if (items.Count == 0)
        {
            Debug.LogError($"[Inventory] Error: no items found for type {itemType}");
            return null;
        }

        return items[Random.Range(0, items.Count)];
    }

    private BaseItemSO RollSymbol()
    {
        if (this.database.JackpotSymbol != null && Random.Range(0, JackpotChanceDivisor) == 0)
        {
            return this.database.JackpotSymbol;
        }

        float totalWeight = 0f;
        for (int index = 0; index < this.database.AllItems.Count; index++)
        {
            BaseItemSO item = this.database.AllItems[index];
            if (item != null && item.SpinWeight > 0f)
            {
                totalWeight += item.SpinWeight;
            }
        }

        if (totalWeight <= 0f)
        {
            return this.database.AllItems.Count > 0 ? this.database.AllItems[0] : null;
        }

        float pick = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int index = 0; index < this.database.AllItems.Count; index++)
        {
            BaseItemSO item = this.database.AllItems[index];
            if (item == null || item.SpinWeight <= 0f)
            {
                continue;
            }

            cumulative += item.SpinWeight;
            if (pick <= cumulative)
            {
                return item;
            }
        }

        return this.database.AllItems.Count > 0 ? this.database.AllItems[this.database.AllItems.Count - 1] : null;
    }
}

public sealed class SpinResult
{
    private readonly BaseItemSO[] items;

    public SpinResult(BaseItemSO first, BaseItemSO second, BaseItemSO third)
    {
        this.items = new BaseItemSO[ReelCount];
        this.items[0] = first;
        this.items[1] = second;
        this.items[2] = third;
    }

    private const int ReelCount = 3;

    public BaseItemSO GetItem(int index)
    {
        return this.items[index];
    }

    public bool Contains(BaseItemSO item)
    {
        if (item == null)
        {
            return false;
        }

        for (int index = 0; index < this.items.Length; index++)
        {
            if (this.items[index] != null && this.items[index].ItemId == item.ItemId)
            {
                return true;
            }
        }

        return false;
    }

    public BaseItemSO GetTripleMatch()
    {
        if (this.items[0] == null || this.items[1] == null || this.items[2] == null)
        {
            return null;
        }

        if (this.items[0].ItemId == this.items[1].ItemId && this.items[1].ItemId == this.items[2].ItemId)
        {
            return this.items[0];
        }

        return null;
    }

    public BaseItemSO GetPairMatch()
    {
        if (this.items[0] != null && this.items[1] != null && this.items[0].ItemId == this.items[1].ItemId)
        {
            return this.items[0];
        }

        if (this.items[0] != null && this.items[2] != null && this.items[0].ItemId == this.items[2].ItemId)
        {
            return this.items[0];
        }

        if (this.items[1] != null && this.items[2] != null && this.items[1].ItemId == this.items[2].ItemId)
        {
            return this.items[1];
        }

        return null;
    }
}
