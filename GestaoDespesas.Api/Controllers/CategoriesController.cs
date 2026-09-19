using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Api.Data;
using GestaoDespesas.Api.DTOs;
using GestaoDespesas.Api.Models;

namespace GestaoDespesas.Api.Controllers;

// Manages expense categories for the authenticated user.
// Route: /api/categories
// [Authorize] applied at the class level: every endpoint here requires a valid JWT.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Extracts the authenticated user's id from the JWT claims.
    // Every action below uses this to scope data to the current user.
    private string GetUserId()
    {
        return User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException();
    }

    // GET /api/categories
    // Returns all categories belonging to the authenticated user
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var userId = GetUserId();

        var categories = await _context.Categories
            .Where(c => c.UserId == userId)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync();

        return Ok(categories);
    }

    // GET /api/categories/{id}
    // Returns a single category, only if it belongs to the authenticated user
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var userId = GetUserId();

        var category = await _context.Categories
            .Where(c => c.Id == id && c.UserId == userId)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .FirstOrDefaultAsync();

        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    // POST /api/categories
    // Creates a new category owned by the authenticated user
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto request)
    {
        var userId = GetUserId();

        var category = new Category
        {
            Name = request.Name,
            UserId = userId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = new CategoryDto { Id = category.Id, Name = category.Name };

        // Returns 201 Created with a Location header pointing to GET /api/categories/{id}
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, result);
    }

    // PUT /api/categories/{id}
    // Updates an existing category, only if it belongs to the authenticated user
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, CreateCategoryDto request)
    {
        var userId = GetUserId();

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category is null)
        {
            return NotFound();
        }

        category.Name = request.Name;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/categories/{id}
    // Deletes a category, only if it belongs to the authenticated user
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var userId = GetUserId();

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category is null)
        {
            return NotFound();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}