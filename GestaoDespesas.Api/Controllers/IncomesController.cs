using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Api.Data;
using GestaoDespesas.Api.DTOs;
using GestaoDespesas.Api.Models;

namespace GestaoDespesas.Api.Controllers;

// Manages income entries for the authenticated user.
// Route: /api/incomes
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IncomesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public IncomesController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetUserId()
    {
        return User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException();
    }

    // GET /api/incomes
    // GET /api/incomes?startDate=2026-09-01&endDate=2026-09-30
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IncomeDto>>> GetIncomes(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var userId = GetUserId();

        var query = _context.Incomes
            .Where(i => i.UserId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(i => i.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(i => i.Date <= endDate.Value);
        }

        var incomes = await query
            .OrderByDescending(i => i.Date)
            .Select(i => new IncomeDto
            {
                Id = i.Id,
                Description = i.Description,
                Amount = i.Amount,
                Date = i.Date
            })
            .ToListAsync();

        return Ok(incomes);
    }

    // GET /api/incomes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<IncomeDto>> GetIncome(int id)
    {
        var userId = GetUserId();

        var income = await _context.Incomes
            .Where(i => i.Id == id && i.UserId == userId)
            .Select(i => new IncomeDto
            {
                Id = i.Id,
                Description = i.Description,
                Amount = i.Amount,
                Date = i.Date
            })
            .FirstOrDefaultAsync();

        if (income is null)
        {
            return NotFound();
        }

        return Ok(income);
    }

    // POST /api/incomes
    [HttpPost]
    public async Task<ActionResult<IncomeDto>> CreateIncome(CreateIncomeDto request)
    {
        var userId = GetUserId();

        var income = new Income
        {
            Description = request.Description,
            Amount = request.Amount,
            Date = request.Date,
            UserId = userId
        };

        _context.Incomes.Add(income);
        await _context.SaveChangesAsync();

        var result = new IncomeDto
        {
            Id = income.Id,
            Description = income.Description,
            Amount = income.Amount,
            Date = income.Date
        };

        return CreatedAtAction(nameof(GetIncome), new { id = income.Id }, result);
    }

    // PUT /api/incomes/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIncome(int id, CreateIncomeDto request)
    {
        var userId = GetUserId();

        var income = await _context.Incomes
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (income is null)
        {
            return NotFound();
        }

        income.Description = request.Description;
        income.Amount = request.Amount;
        income.Date = request.Date;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/incomes/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIncome(int id)
    {
        var userId = GetUserId();

        var income = await _context.Incomes
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (income is null)
        {
            return NotFound();
        }

        _context.Incomes.Remove(income);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}