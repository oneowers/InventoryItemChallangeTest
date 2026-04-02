public sealed class InventoryPresenter
{
    private const int AddCoinsAmount = 100;
    private const int DefaultUnlockCost = 0;

    private readonly InventoryModel model;
    private readonly InventoryService service;
    private readonly InventoryView inventoryView;
    private readonly HUDView hudView;

    public InventoryPresenter(
        InventoryModel model,
        InventoryService service,
        InventoryView inventoryView,
        HUDView hudView)
    {
        this.model = model;
        this.service = service;
        this.inventoryView = inventoryView;
        this.hudView = hudView;
    }

    public void Initialize()
    {
        this.hudView.OnShootClicked += this.HandleShootClicked;
        this.hudView.OnRemoveItemClicked += this.HandleRemoveItemClicked;
        this.hudView.OnAddCoinsClicked += this.HandleAddCoinsClicked;

        this.inventoryView.OnUnlockRequested += this.HandleUnlockRequested;

        this.model.OnSlotChanged += this.HandleSlotChanged;
        this.model.OnCoinsChanged += this.HandleCoinsChanged;
        this.model.OnWeightChanged += this.HandleWeightChanged;

        this.inventoryView.Initialize(this.model.Slots, DefaultUnlockCost);
        this.inventoryView.RenderAll(this.model.Slots);
        this.hudView.SetCoins(this.model.Coins);
        this.hudView.SetWeight(this.model.TotalWeight);
    }

    private void HandleShootClicked()
    {
        this.service.ShootRandom();
    }

    private void HandleRemoveItemClicked()
    {
        this.service.RemoveRandomItem();
    }

    private void HandleAddCoinsClicked()
    {
        this.service.AddCoins(AddCoinsAmount);
    }

    private void HandleUnlockRequested(int slotIndex)
    {
        this.service.TryUnlockSlot(slotIndex);
    }

    private void HandleSlotChanged(SlotModel slot)
    {
        this.inventoryView.RenderSlot(slot);
    }

    private void HandleCoinsChanged(int coins)
    {
        this.hudView.SetCoins(coins);
    }

    private void HandleWeightChanged(float weight)
    {
        this.hudView.SetWeight(weight);
    }
}
