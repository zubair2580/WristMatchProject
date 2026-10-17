namespace WristMatch.Domain;

public enum SalesOrderStatus
{
    Draft = 1,
    Confirmed = 2,
    Processing = 3,
    Completed = 4,
    Cancelled = 5
}

public enum SalesPaymentStatus
{
    Pending = 1,
    Paid = 2,
    Refunded = 3
}

public enum SalesFulfilmentStatus
{
    Unfulfilled = 1,
    Reserved = 2,
    Fulfilled = 3,
    Cancelled = 4
}

public sealed class SalesOrder
{
    private SalesOrder() { }

    public SalesOrder(string orderNumber, Guid customerId, Guid warehouseId, string currency, decimal taxRate)
    {
        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        CustomerId = customerId;
        WarehouseId = warehouseId;
        Currency = currency.ToUpperInvariant();
        TaxRate = taxRate;
        Status = SalesOrderStatus.Draft;
        PaymentStatus = SalesPaymentStatus.Pending;
        FulfilmentStatus = SalesFulfilmentStatus.Unfulfilled;
        PlacedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public SalesPaymentStatus PaymentStatus { get; private set; }
    public SalesFulfilmentStatus FulfilmentStatus { get; private set; }
    public string Currency { get; private set; } = "PKR";
    public decimal TaxRate { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public List<SalesOrderLine> Lines { get; private set; } = [];

    public void ReplaceLines(IEnumerable<(Guid ProductId, decimal Quantity, decimal UnitPrice)> lines)
    {
        if (Status is not SalesOrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can change lines.");
        Lines.Clear();
        foreach (var line in lines)
        {
            if (line.Quantity <= 0) throw new InvalidOperationException("Line quantity must be positive.");
            if (line.UnitPrice < 0) throw new InvalidOperationException("Unit price cannot be negative.");
            var tax = Math.Round(line.Quantity * line.UnitPrice * TaxRate, 2, MidpointRounding.AwayFromZero);
            var total = Math.Round(line.Quantity * line.UnitPrice + tax, 2, MidpointRounding.AwayFromZero);
            Lines.Add(new SalesOrderLine(Id, line.ProductId, line.Quantity, line.UnitPrice, tax, total));
        }
        RecalculateTotals();
    }

    public void SetNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Confirm()
    {
        EnsureTransition(SalesOrderStatus.Draft, SalesOrderStatus.Confirmed);
        if (Lines.Count == 0) throw new InvalidOperationException("Confirm requires at least one line.");
        Status = SalesOrderStatus.Confirmed;
        FulfilmentStatus = SalesFulfilmentStatus.Reserved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void StartProcessing()
    {
        EnsureTransition(SalesOrderStatus.Confirmed, SalesOrderStatus.Processing);
        Status = SalesOrderStatus.Processing;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        if (Status is not (SalesOrderStatus.Confirmed or SalesOrderStatus.Processing))
            throw new InvalidOperationException("Only confirmed or processing orders can be completed.");
        Status = SalesOrderStatus.Completed;
        FulfilmentStatus = SalesFulfilmentStatus.Fulfilled;
        if (PaymentStatus == SalesPaymentStatus.Pending)
            PaymentStatus = SalesPaymentStatus.Paid;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status is SalesOrderStatus.Completed or SalesOrderStatus.Cancelled)
            throw new InvalidOperationException("Completed or cancelled orders cannot be cancelled.");
        Status = SalesOrderStatus.Cancelled;
        FulfilmentStatus = SalesFulfilmentStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPaid()
    {
        if (Status == SalesOrderStatus.Cancelled)
            throw new InvalidOperationException("Cancelled orders cannot be paid.");
        PaymentStatus = SalesPaymentStatus.Paid;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool HasStockReservation => Status is SalesOrderStatus.Confirmed or SalesOrderStatus.Processing;

    private void RecalculateTotals()
    {
        Subtotal = Math.Round(Lines.Sum(line => line.Quantity * line.UnitPrice), 2, MidpointRounding.AwayFromZero);
        TaxTotal = Math.Round(Lines.Sum(line => line.TaxAmount), 2, MidpointRounding.AwayFromZero);
        GrandTotal = Math.Round(Subtotal + TaxTotal, 2, MidpointRounding.AwayFromZero);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureTransition(SalesOrderStatus from, SalesOrderStatus to)
    {
        if (Status != from)
            throw new InvalidOperationException($"Cannot move order from {Status} to {to}.");
    }
}

public sealed class SalesOrderLine
{
    private SalesOrderLine() { }

    public SalesOrderLine(Guid salesOrderId, Guid productId, decimal quantity, decimal unitPrice, decimal taxAmount, decimal lineTotal)
    {
        Id = Guid.NewGuid();
        SalesOrderId = salesOrderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxAmount = taxAmount;
        LineTotal = lineTotal;
    }

    public Guid Id { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public SalesOrder SalesOrder { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal LineTotal { get; private set; }
}
