namespace GestaoDespesas.Api.DTOs;

// Data sent by the client when creating (or updating) an income entry
public class CreateIncomeDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}