using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SlotView : MonoBehaviour
{
    [SerializeField]
    private Image itemIcon;

    [SerializeField]
    private TextMeshProUGUI stackCountText;

    [SerializeField]
    private GameObject lockedOverlay;

    [SerializeField]
    private Button unlockButton;

    [SerializeField]
    private TextMeshProUGUI unlockPriceText;

    private int slotIndex;

    public event Action<int> OnUnlockRequested;

    private void Awake()
    {
        Image rootImage = this.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.raycastTarget = true;
        }
    }

    public void Initialize(int index, int unlockCost)
    {
        this.slotIndex = index;
        this.unlockPriceText.text = unlockCost.ToString();
        this.unlockButton.onClick.RemoveListener(this.HandleUnlockClicked);
        this.unlockButton.onClick.AddListener(this.HandleUnlockClicked);
    }

    public void Render(SlotModel slot)
    {
        if (slot.IsUnlocked == false)
        {
            this.lockedOverlay.SetActive(true);
            this.unlockButton.gameObject.SetActive(true);
            this.itemIcon.color = new Color(1f, 1f, 1f, 0f);
            this.stackCountText.color = new Color(1f, 1f, 1f, 0f);
            return;
        }

        this.lockedOverlay.SetActive(false);
        this.unlockButton.gameObject.SetActive(false);

        if (slot.IsEmpty)
        {
            this.itemIcon.color = new Color(1f, 1f, 1f, 0f);
            this.stackCountText.color = new Color(1f, 1f, 1f, 0f);
            return;
        }

        this.itemIcon.sprite = slot.Item.Icon;
        this.itemIcon.color = Color.white;

        if (slot.Quantity > 1)
        {
            this.stackCountText.text = slot.Quantity.ToString();
            this.stackCountText.color = Color.white;
        }
        else
        {
            this.stackCountText.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    private void HandleUnlockClicked()
    {
        this.OnUnlockRequested?.Invoke(this.slotIndex);
    }
}
