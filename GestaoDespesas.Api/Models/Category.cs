namespace GestaoDespesas.Api.Models
{
    // Represents an expense category (e.g. "Food", "Transport", "Entertainment")
    public class Category
    {
        public int Id { get; set; }
        // Category name, shown to the user (e.g. "Food")
        public string Name { get; set; } = string.Empty;
        // Each user manages their own set of categories
        // Gets or sets the ID of the user who owns this category.
        public string UserId { get; set; } = string.Empty;
    }
}