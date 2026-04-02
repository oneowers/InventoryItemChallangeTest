using System;
using System.Collections.Generic;

public sealed class InventoryModel
{
    public InventoryModel()
    {
        this.Slots = new List<SlotModel>();
    }

    public List<SlotModel> Slots { get; }

    public int Coins { get; set; }

    public float TotalWeight
    {
        get
        {
            float totalWeight = 0f;

            for (int index = 0; index < this.Slots.Count; index++)
            {
                SlotModel slot = this.Slots[index];
                if (slot != null)
                {
                    totalWeight += slot.TotalWeight;
                }
            }

            return totalWeight;
        }
    }

    public event Action<SlotModel> OnSlotChanged;

    public event Action<int> OnCoinsChanged;

    public event Action<float> OnWeightChanged;

    public void NotifySlotChanged(SlotModel slot)
    {
        this.OnSlotChanged?.Invoke(slot);
    }

    public void NotifyCoinsChanged()
    {
        this.OnCoinsChanged?.Invoke(this.Coins);
    }

    public void NotifyWeightChanged()
    {
        this.OnWeightChanged?.Invoke(this.TotalWeight);
    }
}
