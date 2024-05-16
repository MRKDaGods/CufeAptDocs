namespace MRK.Models
{
    public class Document(string id, string name, string ownerId, DateTime creationDate, DateTime modificationDate, bool hasEditPermission)
    {
        public string Id { get; init; } = id;
        public string Name { get; init; } = name;
        public string OwnerId { get; init; } = ownerId;
        public DateTime CreationDate { get; init; } = creationDate;
        public DateTime ModificationDate { get; init; } = modificationDate;
        public bool HasEditPermission { get; init; } = hasEditPermission;
    }
}
