using UnityEngine;
using UnityEngine.EventSystems;

public sealed class SlotDropHandler : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private InventoryService inventoryService;

    private int slotIndex;

    public void Initialize(int index, InventoryService service)
    {
        this.slotIndex = index;
        this.inventoryService = service;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (this.inventoryService == null || eventData.pointerDrag == null)
        {
            return;
        }

        SlotDragHandler dragHandler = eventData.pointerDrag.GetComponent<SlotDragHandler>();
        if (dragHandler == null)
        {
            return;
        }

        this.inventoryService.MoveItem(dragHandler.SlotIndex, this.slotIndex);
    }
}
