namespace GestaoDespesas.Api.DTOs;

//Data sent by the client when creating (or updating) a category
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
}