namespace MRK.Models
{
    public class Document(string id, string name, User owner, DateTime creationDate, DateTime modificationDate, bool hasEditPermission)
    {
        public string Id { get; init; } = id;
        public string Name { get; init; } = name;
        public User Owner { get; init; } = owner;
        public DateTime CreationDate { get; init; } = creationDate;
        public DateTime ModificationDate { get; init; } = modificationDate;
        public bool HasEditPermission { get; init; } = hasEditPermission;
    }
}
