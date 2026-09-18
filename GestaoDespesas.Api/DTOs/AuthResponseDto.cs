namespace GestaoDespesas.Api.DTOs
{
    public class AuthResponseDto
    {
       public string Token { get; set; } = string.Empty;
       //when the token, so the client knows when to log in again
       public DateTime Expiration { get; set; }

    }
}