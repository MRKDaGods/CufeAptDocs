namespace MRK.Models
{
    public class User(string id, string username, string email, string pwdHash)
    {
        public string Id { get; init; } = id;
        public string Username { get; init; } = username;
        public string Email { get; init; } = email;
        public string PasswordHash { get; init; } = pwdHash;
    }
}
