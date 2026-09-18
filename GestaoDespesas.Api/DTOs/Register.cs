namespace GestaoDespesas.Api.DTOs
{
    //data sent by the clinet when registering a new user
    public class RegisterDto
    {
        //the email adreess, also used as the username
        public string Email { get; set; } = string.Empty;
        // plain-text passoword chosen by user(never stored directly;
        // Identity hashes the password and stores the hash in the database)
        public string Password { get; set; } = string.Empty;
    }
}