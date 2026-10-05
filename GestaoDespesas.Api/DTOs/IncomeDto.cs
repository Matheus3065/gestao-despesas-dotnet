namespace GestaoDespesas.Api.DTOs;

// Data returned to the client when listing/reading income entries
public class IncomeDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}