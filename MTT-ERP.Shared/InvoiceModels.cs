using System.ComponentModel.DataAnnotations;

namespace MTT_ERP.Shared;

public enum InvoiceStatus
{
    Completed,
    Pending,
    Canceled,
}

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceCode { get; set; } = $"#{Random.Shared.Next(1000, 9999)}";

    [Required(ErrorMessage = "Client name is required")]
    public string CustomerName { get; set; } = "";

    [Required(ErrorMessage = "Billing email is required")]
    [RegularExpression(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ErrorMessage = "Enter a valid email (e.g., finance@company.ae)"
    )]
    public string CustomerEmail { get; set; } = "";

    public DateTime CreateDate { get; set; } = DateTime.Today;
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

    [Range(1, 10000, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    [Range(1, 10000000, ErrorMessage = "Amount must be greater than 0")]
    public decimal Subtotal { get; set; }

    public decimal VatAmount => Math.Round(Subtotal * 0.05m, 2); // 5% UAE VAT
    public decimal TotalAmount => Subtotal + VatAmount;

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
}

public record InvoiceSummaryDto(
    decimal TotalReceivables,
    decimal PaidAmount,
    decimal UnpaidAmount,
    decimal OverdueAmount,
    int OverduePercentage
);

public record CustomerSummaryDto(
    string CustomerName,
    string CustomerEmail,
    int InvoiceCount,
    decimal TotalBilled,
    decimal PendingBalance
);
