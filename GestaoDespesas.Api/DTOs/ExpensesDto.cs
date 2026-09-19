namespace GestaoDespesas.Api.DTOs;

// Data returned to the client when listing/reading expenses
public class ExpenseDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }

    // Included so the client doesn't need a separate request
    // just to display the category name
    public string CategoryName { get; set; } = string.Empty;
}