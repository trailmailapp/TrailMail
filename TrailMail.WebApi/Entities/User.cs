namespace TrailMail.WebApi.Entities;

public class User
{
    public Guid Id { get; set; }
    
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required Uri Picture { get; set; }
    public required string LinkedInId { get; set; }

    public string Name => $"{FirstName} {LastName}";
}