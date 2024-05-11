namespace MRK.Models
{
    public class Session(string id, User user)
    {
        public string Id { get; init; } = id;
        public User User { get; init; } = user;
    }
}
