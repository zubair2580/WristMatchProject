namespace WristMatch.Domain;

public sealed class InventoryBalance
{
    private InventoryBalance() { }

    public InventoryBalance(Guid warehouseId, Guid productId)
    {
        Id = Guid.NewGuid();
        WarehouseId = warehouseId;
        ProductId = productId;
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal OnHand { get; private set; }
    public decimal Reserved { get; private set; }
    public decimal AverageCost { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public decimal Available => OnHand - Reserved;

    public void ApplyReceipt(decimal quantity, decimal unitCost, DateTimeOffset now)
    {
        if (quantity <= 0) throw new InvalidOperationException("Receipt quantity must be positive.");
        var totalCost = OnHand * AverageCost + quantity * unitCost;
        OnHand += quantity;
        AverageCost = OnHand == 0 ? 0 : Math.Round(totalCost / OnHand, 6, MidpointRounding.AwayFromZero);
        UpdatedAt = now;
    }

    public void ApplyAdjustment(decimal quantityDelta, decimal? unitCost, DateTimeOffset now)
    {
        var next = OnHand + quantityDelta;
        if (next < 0) throw new InvalidOperationException("Adjustment would make on-hand negative.");
        if (quantityDelta > 0 && unitCost.HasValue)
        {
            var totalCost = OnHand * AverageCost + quantityDelta * unitCost.Value;
            OnHand = next;
            AverageCost = OnHand == 0 ? 0 : Math.Round(totalCost / OnHand, 6, MidpointRounding.AwayFromZero);
        }
        else
        {
            OnHand = next;
            if (OnHand == 0) AverageCost = 0;
        }
        UpdatedAt = now;
    }

    public void Reserve(decimal quantity, DateTimeOffset now)
    {
        if (quantity <= 0) throw new InvalidOperationException("Reserve quantity must be positive.");
        if (Available < quantity) throw new InvalidOperationException("Insufficient available stock.");
        Reserved += quantity;
        UpdatedAt = now;
    }

    public void Release(decimal quantity, DateTimeOffset now)
    {
        if (quantity <= 0) throw new InvalidOperationException("Release quantity must be positive.");
        if (Reserved < quantity) throw new InvalidOperationException("Cannot release more than reserved.");
        Reserved -= quantity;
        UpdatedAt = now;
    }

    public void FulfilReserved(decimal quantity, DateTimeOffset now)
    {
        if (quantity <= 0) throw new InvalidOperationException("Fulfil quantity must be positive.");
        if (Reserved < quantity) throw new InvalidOperationException("Cannot fulfil more than reserved.");
        if (OnHand < quantity) throw new InvalidOperationException("Insufficient on-hand stock to fulfil.");
        Reserved -= quantity;
        OnHand -= quantity;
        if (OnHand == 0) AverageCost = 0;
        UpdatedAt = now;
    }
}

public sealed class StockMovement
{
    private StockMovement() { }

    public StockMovement(Guid warehouseId, Guid productId, string movementType, decimal quantity, decimal unitCost, string referenceNumber, string? notes)
    {
        Id = Guid.NewGuid();
        WarehouseId = warehouseId;
        ProductId = productId;
        MovementType = movementType;
        Quantity = quantity;
        UnitCost = unitCost;
        TotalCost = Math.Round(Math.Abs(quantity) * unitCost, 2, MidpointRounding.AwayFromZero);
        ReferenceNumber = referenceNumber;
        Notes = notes;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public string MovementType { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
}
