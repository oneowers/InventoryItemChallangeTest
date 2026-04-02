using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Image itemIcon;

    [SerializeField]
    private Canvas dragCanvas;

    [SerializeField]
    private InventoryService inventoryService;

    private GameObject ghostObject;
    private RectTransform ghostRectTransform;
    private int slotIndex;

    public int SlotIndex
    {
        get
        {
            return this.slotIndex;
        }
    }

    public void Initialize(int index, InventoryService service)
    {
        this.slotIndex = index;
        this.inventoryService = service;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (this.itemIcon == null || this.itemIcon.sprite == null || this.itemIcon.color.a == 0f)
        {
            return;
        }

        Canvas targetCanvas = this.dragCanvas;
        if (targetCanvas == null)
        {
            targetCanvas = this.GetComponentInParent<Canvas>();
        }

        if (targetCanvas == null)
        {
            return;
        }

        this.ghostObject = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        this.ghostObject.transform.SetParent(targetCanvas.transform, false);

        Image ghostImage = this.ghostObject.GetComponent<Image>();
        ghostImage.sprite = this.itemIcon.sprite;
        ghostImage.raycastTarget = false;
        ghostImage.color = new Color(1f, 1f, 1f, 0.8f);
        ghostImage.preserveAspect = true;

        CanvasGroup ghostCanvasGroup = this.ghostObject.GetComponent<CanvasGroup>();
        ghostCanvasGroup.blocksRaycasts = false;

        this.ghostRectTransform = this.ghostObject.GetComponent<RectTransform>();
        this.ghostRectTransform.sizeDelta = this.itemIcon.rectTransform.rect.size;

        this.UpdateGhostPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.ghostObject != null)
        {
            Destroy(this.ghostObject);
            this.ghostObject = null;
            this.ghostRectTransform = null;
        }
    }

    private void UpdateGhostPosition(PointerEventData eventData)
    {
        if (this.ghostRectTransform == null)
        {
            return;
        }

        this.ghostRectTransform.position = eventData.position;
    }
}
