namespace backend.Models
{
    public enum Role
    {
        Viewer = 1,
        Editor = 2,
        Author = 3,
        Admin = 4
    }

    public enum Status
    {
        Pending = 1,
        Completed = 2,
        Removed = 3,
    }
}