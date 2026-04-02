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
    private Button addAmmoButton;

    [SerializeField]
    private Button addItemButton;

    [SerializeField]
    private Button removeItemButton;

    [SerializeField]
    private Button addCoinsButton;

    public event Action OnShootClicked;

    public event Action OnAddAmmoClicked;

    public event Action OnAddItemClicked;

    public event Action OnRemoveItemClicked;

    public event Action OnAddCoinsClicked;

    private void Awake()
    {
        this.shootButton.onClick.AddListener(this.HandleShootClicked);
        this.addAmmoButton.onClick.AddListener(this.HandleAddAmmoClicked);
        this.addItemButton.onClick.AddListener(this.HandleAddItemClicked);
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

    private void HandleShootClicked()
    {
        this.OnShootClicked?.Invoke();
    }

    private void HandleAddAmmoClicked()
    {
        this.OnAddAmmoClicked?.Invoke();
    }

    private void HandleAddItemClicked()
    {
        this.OnAddItemClicked?.Invoke();
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
