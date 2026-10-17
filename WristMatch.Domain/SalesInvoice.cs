namespace WristMatch.Domain;

public enum SalesInvoiceStatus
{
    Draft = 1,
    Issued = 2,
    Paid = 3,
    Cancelled = 4
}

public enum PaymentRecordStatus
{
    Pending = 1,
    Confirmed = 2,
    Voided = 3
}

public sealed class SalesInvoice
{
    private SalesInvoice() { }

    public SalesInvoice(string invoiceNumber, Guid salesOrderId, Guid customerId, string currency, DateTimeOffset invoiceDate, DateTimeOffset dueDate)
    {
        Id = Guid.NewGuid();
        InvoiceNumber = invoiceNumber;
        SalesOrderId = salesOrderId;
        CustomerId = customerId;
        Currency = currency.ToUpperInvariant();
        Status = SalesInvoiceStatus.Draft;
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid SalesOrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public SalesInvoiceStatus Status { get; private set; }
    public string Currency { get; private set; } = "PKR";
    public DateTimeOffset InvoiceDate { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public decimal OutstandingAmount { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public List<SalesInvoiceLine> Lines { get; private set; } = [];
    public List<Payment> Payments { get; private set; } = [];

    public void ReplaceLines(IEnumerable<(Guid ProductId, decimal Quantity, decimal UnitPrice, decimal TaxAmount, decimal LineTotal)> lines)
    {
        if (Status is not SalesInvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can change lines.");
        Lines.Clear();
        foreach (var line in lines)
        {
            if (line.Quantity <= 0) throw new InvalidOperationException("Line quantity must be positive.");
            if (line.UnitPrice < 0) throw new InvalidOperationException("Unit price cannot be negative.");
            Lines.Add(new SalesInvoiceLine(Id, line.ProductId, line.Quantity, line.UnitPrice, line.TaxAmount, line.LineTotal));
        }
        RecalculateTotals();
    }

    public void SetNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Issue()
    {
        if (Status != SalesInvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be issued.");
        if (Lines.Count == 0)
            throw new InvalidOperationException("Issue requires at least one line.");
        Status = SalesInvoiceStatus.Issued;
        OutstandingAmount = GrandTotal;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Payment RecordPayment(string paymentNumber, string method, decimal amount, DateTimeOffset receivedAt)
    {
        if (Status is not (SalesInvoiceStatus.Issued or SalesInvoiceStatus.Paid))
            throw new InvalidOperationException("Only issued invoices can receive payments.");
        if (amount <= 0)
            throw new InvalidOperationException("Payment amount must be positive.");
        if (amount > OutstandingAmount)
            throw new InvalidOperationException("Payment cannot exceed outstanding amount.");

        var payment = new Payment(paymentNumber, CustomerId, Id, method, amount, Currency, receivedAt);
        OutstandingAmount = Math.Round(OutstandingAmount - amount, 2, MidpointRounding.AwayFromZero);
        Status = OutstandingAmount <= 0 ? SalesInvoiceStatus.Paid : SalesInvoiceStatus.Issued;
        if (OutstandingAmount < 0) OutstandingAmount = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
        return payment;
    }

    public void Cancel(bool hasConfirmedPayments)
    {
        if (Status is SalesInvoiceStatus.Paid or SalesInvoiceStatus.Cancelled)
            throw new InvalidOperationException("Paid or cancelled invoices cannot be cancelled.");
        if (hasConfirmedPayments)
            throw new InvalidOperationException("Cannot cancel an invoice with confirmed payments.");
        Status = SalesInvoiceStatus.Cancelled;
        OutstandingAmount = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void RecalculateTotals()
    {
        Subtotal = Math.Round(Lines.Sum(line => line.Quantity * line.UnitPrice), 2, MidpointRounding.AwayFromZero);
        TaxTotal = Math.Round(Lines.Sum(line => line.TaxAmount), 2, MidpointRounding.AwayFromZero);
        GrandTotal = Math.Round(Subtotal + TaxTotal, 2, MidpointRounding.AwayFromZero);
        if (Status == SalesInvoiceStatus.Draft)
            OutstandingAmount = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public sealed class SalesInvoiceLine
{
    private SalesInvoiceLine() { }

    public SalesInvoiceLine(Guid salesInvoiceId, Guid productId, decimal quantity, decimal unitPrice, decimal taxAmount, decimal lineTotal)
    {
        Id = Guid.NewGuid();
        SalesInvoiceId = salesInvoiceId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxAmount = taxAmount;
        LineTotal = lineTotal;
    }

    public Guid Id { get; private set; }
    public Guid SalesInvoiceId { get; private set; }
    public SalesInvoice SalesInvoice { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal LineTotal { get; private set; }
}

public sealed class Payment
{
    private Payment() { }

    public Payment(string paymentNumber, Guid customerId, Guid salesInvoiceId, string method, decimal amount, string currency, DateTimeOffset receivedAt)
    {
        Id = Guid.NewGuid();
        PaymentNumber = paymentNumber;
        CustomerId = customerId;
        SalesInvoiceId = salesInvoiceId;
        Method = string.IsNullOrWhiteSpace(method) ? "BankTransfer" : method.Trim();
        Status = PaymentRecordStatus.Confirmed;
        Currency = currency.ToUpperInvariant();
        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        ReceivedAt = receivedAt;
    }

    public Guid Id { get; private set; }
    public string PaymentNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Guid SalesInvoiceId { get; private set; }
    public SalesInvoice SalesInvoice { get; private set; } = null!;
    public string Method { get; private set; } = "BankTransfer";
    public PaymentRecordStatus Status { get; private set; }
    public string Currency { get; private set; } = "PKR";
    public decimal Amount { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }
}
