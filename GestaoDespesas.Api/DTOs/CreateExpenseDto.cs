

namespace GestaoDespesas.Api.DTOs;

// Data sent by the client when creating (or updating) an expense
public class CreateExpenseDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
}