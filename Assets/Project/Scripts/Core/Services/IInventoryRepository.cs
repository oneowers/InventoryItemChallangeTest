public interface IInventoryRepository
{
    void Save(InventoryModel model);

    InventorySaveData Load();
}
