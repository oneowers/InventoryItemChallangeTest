using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "Inventory/Items/Game Database")]
public sealed class GameDatabaseSO : ScriptableObject
{
    [SerializeField]
    private List<BaseItemSO> allItems = new List<BaseItemSO>();

    private Dictionary<string, BaseItemSO> _cache = new Dictionary<string, BaseItemSO>();

    public IReadOnlyList<BaseItemSO> AllItems
    {
        get
        {
            return this.allItems;
        }
    }

    private void OnEnable()
    {
        this._cache = this.allItems
            .Where(item => item != null && !string.IsNullOrWhiteSpace(item.ItemId))
            .GroupBy(item => item.ItemId)
            .ToDictionary(group => group.Key, group => group.First());
    }

    public BaseItemSO GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        BaseItemSO item;
        if (this._cache != null && this._cache.TryGetValue(id, out item))
        {
            return item;
        }

        return null;
    }

    private void OnValidate()
    {
        HashSet<string> uniqueIds = new HashSet<string>();

        for (int index = 0; index < this.allItems.Count; index++)
        {
            BaseItemSO item = this.allItems[index];
            if (item == null || string.IsNullOrWhiteSpace(item.ItemId))
            {
                continue;
            }

            if (!uniqueIds.Add(item.ItemId))
            {
                Debug.LogWarning($"Duplicate itemId '{item.ItemId}' found in '{this.name}'.", this);
            }
        }
    }
}
