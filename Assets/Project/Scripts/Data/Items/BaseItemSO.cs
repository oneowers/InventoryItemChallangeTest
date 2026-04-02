using UnityEngine;

[CreateAssetMenu(fileName = "BaseItem", menuName = "Inventory/Items/Base Item")]
public abstract class BaseItemSO : ScriptableObject
{
    [SerializeField]
    private string itemId = string.Empty;

    [SerializeField]
    private string displayName = string.Empty;

    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private float weightPerUnit;

    [SerializeField]
    private int maxStackSize = 1;

    [SerializeField]
    private ItemType itemType;

    [SerializeField]
    private float spinWeight;

    public string ItemId
    {
        get
        {
            return this.itemId;
        }
    }

    public string DisplayName
    {
        get
        {
            return this.displayName;
        }
    }

    public Sprite Icon
    {
        get
        {
            return this.icon;
        }
    }

    public float WeightPerUnit
    {
        get
        {
            return this.weightPerUnit;
        }
    }

    public int MaxStackSize
    {
        get
        {
            return this.maxStackSize;
        }
    }

    public ItemType ItemType
    {
        get
        {
            return this.itemType;
        }
    }

    public float SpinWeight
    {
        get
        {
            return this.spinWeight;
        }
    }

    protected virtual void OnValidate()
    {
        const float MinimumWeightPerUnit = 0f;
        const int MinimumStackSize = 1;

        if (string.IsNullOrWhiteSpace(this.itemId))
        {
            Debug.LogWarning($"Item ID is empty on asset '{this.name}'.", this);
        }

        if (this.weightPerUnit < MinimumWeightPerUnit)
        {
            this.weightPerUnit = MinimumWeightPerUnit;
        }

        if (this.maxStackSize < MinimumStackSize)
        {
            this.maxStackSize = MinimumStackSize;
        }

        if (this.spinWeight < MinimumWeightPerUnit)
        {
            this.spinWeight = MinimumWeightPerUnit;
        }
    }
}
