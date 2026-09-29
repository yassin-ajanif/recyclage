namespace Recyclage.ViewModels;

/// <summary>
/// Shared labels for invoice lines, including lines that carry a handed cash
/// amount instead of a product.
/// </summary>
public static class InvoiceLineLabels
{
    /// <summary>
    /// Shown in place of a product name when a line records money handed over
    /// (paid to a supplier, or received from a client) with no product.
    /// </summary>
    public const string CashOnly = "مبلغ نقدي";

    /// <summary>The product name, or <see cref="CashOnly"/> when the line has no product.</summary>
    public static string ProductOrFallback(string? productName) =>
        string.IsNullOrWhiteSpace(productName) ? CashOnly : productName.Trim();
}
