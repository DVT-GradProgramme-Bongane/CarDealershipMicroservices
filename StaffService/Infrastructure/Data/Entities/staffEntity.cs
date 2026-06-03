public class StaffEntitiy
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Role StaffRole {get; set; }

    public string Email {get; set;} = string.Empty;
    public string Phone {get; set;} = string.Empty;

    public DateTime CreatedAt {get; set; } 

    public StaffEntitiy(string firstName, string lastName, Role role, string email, string phone)
    {
        Id = new Guid();
        FirstName = firstName;
        LastName = lastName;
        StaffRole = role;
        Email = email;
        Phone = phone;
        CreatedAt = DateTime.UtcNow; // utc vs normal DateNow
    }
}

