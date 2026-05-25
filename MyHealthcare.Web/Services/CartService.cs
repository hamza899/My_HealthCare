using MyHealthcare.Shared.DTOs;

namespace MyHealthcare.Web.Services;

public class CartService
{
    private const string StorageKey = "myhealthcare_cart";
    private readonly LocalStorageService _storage;
    private List<CartItemDto>? _items;

    public event Action? OnChange;

    public CartService(LocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task<List<CartItemDto>> GetItemsAsync()
    {
        if (_items is null)
        {
            _items = await _storage.GetAsync<List<CartItemDto>>(StorageKey) ?? new();
        }
        return _items;
    }

    public async Task<int> GetItemCountAsync()
    {
        var items = await GetItemsAsync();
        return items.Sum(i => i.Quantity);
    }

    public async Task<decimal> GetTotalAsync()
    {
        var items = await GetItemsAsync();
        return items.Sum(i => i.Subtotal);
    }

    public async Task AddAsync(MedicineDto medicine, int quantity = 1)
    {
        var items = await GetItemsAsync();
        var existing = items.FirstOrDefault(i => i.MedicineId == medicine.Id);

        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            items.Add(new CartItemDto
            {
                MedicineId = medicine.Id,
                MedicineName = medicine.Name,
                ImageUrl = medicine.ImageUrl,
                UnitPrice = medicine.EffectivePrice,
                Quantity = quantity,
                PrescriptionRequired = medicine.PrescriptionRequired
            });
        }

        await PersistAsync();
    }

    public async Task UpdateQuantityAsync(string medicineId, int quantity)
    {
        var items = await GetItemsAsync();
        var item = items.FirstOrDefault(i => i.MedicineId == medicineId);
        if (item is null) return;

        if (quantity <= 0)
        {
            items.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        await PersistAsync();
    }

    public async Task RemoveAsync(string medicineId)
    {
        var items = await GetItemsAsync();
        items.RemoveAll(i => i.MedicineId == medicineId);
        await PersistAsync();
    }

    public async Task ClearAsync()
    {
        _items = new();
        await _storage.RemoveAsync(StorageKey);
        OnChange?.Invoke();
    }

    private async Task PersistAsync()
    {
        if (_items is null) return;
        await _storage.SetAsync(StorageKey, _items);
        OnChange?.Invoke();
    }
}
