namespace GestaoDespesas.Api.Models;

// Represents a single income entry for a user (e.g. salary, freelance payment)
public class Income
{
    public int Id { get; set; }

    // Short description of the income source (e.g. "Salary", "Freelance project")
    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    // Foreign key: links this income to the user who registered it.
    // Unlike expenses, income doesn't need a category for this project's scope.
    public string UserId { get; set; } = string.Empty;
}