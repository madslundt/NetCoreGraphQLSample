using System;

namespace DataModel.Models
{
    public class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Text { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
