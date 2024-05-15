namespace MRK.Models
{
    public class DocumentUser(User user, Document document, bool hasEditPerm)
    {
        public User User { get; init; } = user;
        public Document Document { get; init; } = document;
        public bool HasEditPermission { get; init; } = hasEditPerm;
    }
}
