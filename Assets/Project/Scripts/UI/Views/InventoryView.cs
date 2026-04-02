using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryView : MonoBehaviour
{
    [SerializeField]
    private List<SlotView> slotViews = new List<SlotView>();

    public event Action<int> OnUnlockRequested;

    public void Initialize(List<SlotModel> slots, int unlockCost)
    {
        int slotViewCount = this.slotViews.Count;
        int slotCount = slots.Count;
        int count = slotViewCount < slotCount ? slotViewCount : slotCount;

        for (int index = 0; index < count; index++)
        {
            SlotView slotView = this.slotViews[index];
            slotView.Initialize(index, unlockCost);
            slotView.OnUnlockRequested -= this.HandleUnlockRequested;
            slotView.OnUnlockRequested += this.HandleUnlockRequested;
        }
    }

    public void InitializeDragDrop(InventoryService service)
    {
        for (int index = 0; index < this.slotViews.Count; index++)
        {
            SlotDragHandler drag = this.slotViews[index].GetComponent<SlotDragHandler>();
            SlotDropHandler drop = this.slotViews[index].GetComponent<SlotDropHandler>();

            if (drag != null)
            {
                drag.Initialize(index, service);
            }

            if (drop != null)
            {
                drop.Initialize(index, service);
            }
        }
    }

    public void RenderSlot(SlotModel slot)
    {
        this.slotViews[slot.Index].Render(slot);
    }

    public void RenderAll(List<SlotModel> slots)
    {
        for (int index = 0; index < slots.Count; index++)
        {
            this.RenderSlot(slots[index]);
        }
    }

    private void HandleUnlockRequested(int slotIndex)
    {
        this.OnUnlockRequested?.Invoke(slotIndex);
    }
}
