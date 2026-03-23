namespace backend.Models
{
    public class Domain
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // public IEnumerable<Course> Courses { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}

