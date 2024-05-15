namespace MRK
{
    public abstract class Operation
    {
        public int Position { get; set; }
        public string UserId { get; set; }
        public DateTime Timestamp { get; set; }

        protected Operation(int position, string userId)
        {
            Position = position;
            UserId = userId;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class InsertOperation : Operation
    {
        public string Text { get; set; }

        public InsertOperation(int position, string text, string userId) : base(position, userId)
        {
            Text = text;
        }
    }

    public class DeleteOperation : Operation
    {
        public int Length { get; set; }

        public DeleteOperation(int position, int length, string userId) : base(position, userId)
        {
            Length = length;
        }
    }
}
