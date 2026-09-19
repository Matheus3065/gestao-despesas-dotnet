namespace GestaoDespesas.Api.DTOs;  

//Data returned to the client when listing categories
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

}