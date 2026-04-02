public sealed class SlotModel
{
    public int Index { get; set; }

    public bool IsUnlocked { get; set; }

    public BaseItemSO Item { get; set; }

    public int Quantity { get; set; }

    public bool IsEmpty
    {
        get
        {
            return this.Item == null;
        }
    }

    public float TotalWeight
    {
        get
        {
            if (this.Item == null)
            {
                return 0f;
            }

            return this.Item.WeightPerUnit * this.Quantity;
        }
    }
}
