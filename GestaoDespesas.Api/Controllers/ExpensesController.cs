using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Api.Data;
using GestaoDespesas.Api.DTOs;
using GestaoDespesas.Api.Models;

namespace GestaoDespesas.Api.Controllers;

// Manages expenses for the authenticated user.
// Route: /api/expenses
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ExpensesController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetUserId()
    {
        return User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException();
    }

    // GET /api/expenses
    // Returns all expenses belonging to the authenticated user,
    // including the related category name
    // GET /api/expenses
// GET /api/expenses?startDate=2026-09-01&endDate=2026-09-30&categoryId=2
// Returns all expenses belonging to the authenticated user,
// optionally filtered by date range and/or category
[HttpGet]
public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses(
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    [FromQuery] int? categoryId)
{
    var userId = GetUserId();

    // Start with the base query: all expenses for this user
    var query = _context.Expenses
        .Where(e => e.UserId == userId);

    // Apply filters only when the client actually provided them
    if (startDate.HasValue)
    {
        query = query.Where(e => e.Date >= startDate.Value);
    }

    if (endDate.HasValue)
    {
        query = query.Where(e => e.Date <= endDate.Value);
    }

    if (categoryId.HasValue)
    {
        query = query.Where(e => e.CategoryId == categoryId.Value);
    }

    var expenses = await query
        .Include(e => e.Category)
        .OrderByDescending(e => e.Date)
        .Select(e => new ExpenseDto
        {
            Id = e.Id,
            Description = e.Description,
            Amount = e.Amount,
            Date = e.Date,
            CategoryId = e.CategoryId,
            CategoryName = e.Category != null ? e.Category.Name : string.Empty
        })
        .ToListAsync();

    return Ok(expenses);
}

    // GET /api/expenses/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseDto>> GetExpense(int id)
    {
        var userId = GetUserId();

        var expense = await _context.Expenses
            .Where(e => e.Id == id && e.UserId == userId)
            .Include(e => e.Category)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount,
                Date = e.Date,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : string.Empty
            })
            .FirstOrDefaultAsync();

        if (expense is null)
        {
            return NotFound();
        }

        return Ok(expense);
    }

    // POST /api/expenses
    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> CreateExpense(CreateExpenseDto request)
    {
        var userId = GetUserId();

        // Validate that the category exists AND belongs to the authenticated user.
        // Without this check, a user could create an expense linked to
        // someone else's category by guessing its id.
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);

        if (!categoryExists)
        {
            return BadRequest("Invalid category.");
        }

        var expense = new Expense
        {
            Description = request.Description,
            Amount = request.Amount,
            Date = request.Date,
            CategoryId = request.CategoryId,
            UserId = userId
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        // Reload with category name for the response
        var category = await _context.Categories.FindAsync(request.CategoryId);

        var result = new ExpenseDto
        {
            Id = expense.Id,
            Description = expense.Description,
            Amount = expense.Amount,
            Date = expense.Date,
            CategoryId = expense.CategoryId,
            CategoryName = category?.Name ?? string.Empty
        };

        return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, result);
    }

    // PUT /api/expenses/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, CreateExpenseDto request)
    {
        var userId = GetUserId();

        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (expense is null)
        {
            return NotFound();
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);

        if (!categoryExists)
        {
            return BadRequest("Invalid category.");
        }

        expense.Description = request.Description;
        expense.Amount = request.Amount;
        expense.Date = request.Date;
        expense.CategoryId = request.CategoryId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/expenses/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var userId = GetUserId();

        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (expense is null)
        {
            return NotFound();
        }

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}