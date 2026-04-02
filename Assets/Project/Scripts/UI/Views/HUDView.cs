using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HUDView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI coinsText;

    [SerializeField]
    private TextMeshProUGUI weightText;

    [SerializeField]
    private Button shootButton;

    [SerializeField]
    private Button spinButton;

    [SerializeField]
    private Button removeItemButton;

    [SerializeField]
    private Button addCoinsButton;

    public event Action OnShootClicked;

    public event Action OnSpinClicked;

    public event Action OnRemoveItemClicked;

    public event Action OnAddCoinsClicked;

    private void Awake()
    {
        this.shootButton.onClick.AddListener(this.HandleShootClicked);
        this.spinButton.onClick.AddListener(this.HandleSpinClicked);
        this.removeItemButton.onClick.AddListener(this.HandleRemoveItemClicked);
        this.addCoinsButton.onClick.AddListener(this.HandleAddCoinsClicked);
    }

    public void SetCoins(int amount)
    {
        this.coinsText.text = $"Coins: {amount}";
    }

    public void SetWeight(float kg)
    {
        this.weightText.text = $"Weight: {kg:F2} kg";
    }

    public void SetSpinInteractable(bool isInteractable)
    {
        this.spinButton.interactable = isInteractable;
    }

    private void HandleShootClicked()
    {
        this.OnShootClicked?.Invoke();
    }

    private void HandleSpinClicked()
    {
        this.OnSpinClicked?.Invoke();
    }

    private void HandleRemoveItemClicked()
    {
        this.OnRemoveItemClicked?.Invoke();
    }

    private void HandleAddCoinsClicked()
    {
        this.OnAddCoinsClicked?.Invoke();
    }
}
