namespace WristMatch.Domain;

public enum PurchaseOrderStatus
{
    Draft = 1,
    Approved = 2,
    Received = 3,
    Cancelled = 4
}

public sealed class PurchaseOrder
{
    private PurchaseOrder() { }

    public PurchaseOrder(string purchaseOrderNumber, Guid supplierId, Guid warehouseId, string currency, decimal taxRate, DateTimeOffset? expectedAt)
    {
        Id = Guid.NewGuid();
        PurchaseOrderNumber = purchaseOrderNumber;
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        Currency = currency.ToUpperInvariant();
        TaxRate = taxRate;
        Status = PurchaseOrderStatus.Draft;
        OrderedAt = UpdatedAt = DateTimeOffset.UtcNow;
        ExpectedAt = expectedAt;
    }

    public Guid Id { get; private set; }
    public string PurchaseOrderNumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public string Currency { get; private set; } = "PKR";
    public decimal TaxRate { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset OrderedAt { get; private set; }
    public DateTimeOffset? ExpectedAt { get; private set; }
    public DateTimeOffset? ReceivedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public List<PurchaseOrderLine> Lines { get; private set; } = [];

    public void ReplaceLines(IEnumerable<(Guid ProductId, decimal Quantity, decimal UnitCost)> lines)
    {
        if (Status is not PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Only draft purchase orders can change lines.");
        Lines.Clear();
        foreach (var line in lines)
        {
            if (line.Quantity <= 0) throw new InvalidOperationException("Line quantity must be positive.");
            if (line.UnitCost < 0) throw new InvalidOperationException("Unit cost cannot be negative.");
            var lineTotal = Math.Round(line.Quantity * line.UnitCost, 2, MidpointRounding.AwayFromZero);
            Lines.Add(new PurchaseOrderLine(Id, line.ProductId, line.Quantity, line.UnitCost, lineTotal));
        }
        RecalculateTotals();
    }

    public void SetNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetExpectedAt(DateTimeOffset? expectedAt)
    {
        ExpectedAt = expectedAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Approve()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Only draft purchase orders can be approved.");
        if (Lines.Count == 0)
            throw new InvalidOperationException("Approve requires at least one line.");
        Status = PurchaseOrderStatus.Approved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Receive(IReadOnlyDictionary<Guid, decimal> quantitiesByLineId, DateTimeOffset now)
    {
        if (Status is not (PurchaseOrderStatus.Approved or PurchaseOrderStatus.Received))
            throw new InvalidOperationException("Only approved or partially received purchase orders can receive goods.");

        var receivedAny = false;
        foreach (var line in Lines)
        {
            if (!quantitiesByLineId.TryGetValue(line.Id, out var qty) || qty <= 0)
                continue;
            line.Receive(qty);
            receivedAny = true;
        }
        if (!receivedAny)
            throw new InvalidOperationException("Provide a positive receive quantity for at least one line.");

        Status = Lines.All(line => line.ReceivedQuantity >= line.Quantity)
            ? PurchaseOrderStatus.Received
            : PurchaseOrderStatus.Approved;
        if (Status == PurchaseOrderStatus.Received)
            ReceivedAt = now;
        UpdatedAt = now;
    }

    public void Cancel()
    {
        if (Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled)
            throw new InvalidOperationException("Received or cancelled purchase orders cannot be cancelled.");
        if (Lines.Any(line => line.ReceivedQuantity > 0))
            throw new InvalidOperationException("Cannot cancel a purchase order with received quantities.");
        Status = PurchaseOrderStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void RecalculateTotals()
    {
        Subtotal = Math.Round(Lines.Sum(line => line.Quantity * line.UnitCost), 2, MidpointRounding.AwayFromZero);
        TaxTotal = Math.Round(Subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);
        GrandTotal = Math.Round(Subtotal + TaxTotal, 2, MidpointRounding.AwayFromZero);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public sealed class PurchaseOrderLine
{
    private PurchaseOrderLine() { }

    public PurchaseOrderLine(Guid purchaseOrderId, Guid productId, decimal quantity, decimal unitCost, decimal lineTotal)
    {
        Id = Guid.NewGuid();
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
        LineTotal = lineTotal;
    }

    public Guid Id { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public PurchaseOrder PurchaseOrder { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal LineTotal { get; private set; }

    public decimal OutstandingQuantity => Quantity - ReceivedQuantity;

    public void Receive(decimal quantity)
    {
        if (quantity <= 0) throw new InvalidOperationException("Receive quantity must be positive.");
        if (ReceivedQuantity + quantity > Quantity)
            throw new InvalidOperationException("Cannot receive more than the ordered quantity.");
        ReceivedQuantity += quantity;
    }
}
