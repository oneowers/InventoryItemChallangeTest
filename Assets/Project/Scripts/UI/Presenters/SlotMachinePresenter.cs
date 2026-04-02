public sealed class SlotMachinePresenter
{
    private readonly SlotMachineService service;
    private readonly SlotMachineView view;
    private readonly HUDView hudView;

    private bool isSpinning;

    public SlotMachinePresenter(
        SlotMachineService service,
        SlotMachineView view,
        HUDView hudView)
    {
        this.service = service;
        this.view = view;
        this.hudView = hudView;
    }

    public void Initialize()
    {
        this.view.Initialize(this.service.GetSpinSymbols());
        this.hudView.OnSpinClicked += this.HandleSpinClicked;
        this.hudView.SetSpinInteractable(true);
    }

    private void HandleSpinClicked()
    {
        if (this.isSpinning)
        {
            return;
        }

        if (!this.service.TryStartSpin())
        {
            return;
        }

        this.isSpinning = true;
        this.hudView.SetSpinInteractable(false);

        SpinResult result = this.service.GetSpinResult();
        this.view.Spin(result, () =>
        {
            this.service.ApplyResult(result);
            this.isSpinning = false;
            this.hudView.SetSpinInteractable(true);
        });
    }
}
