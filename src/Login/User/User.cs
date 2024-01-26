namespace ApiSevenet;

public record User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Username { get; set; } = null;
    public string? Password { get; set; } = null;
    public bool Admin { get; set; } = false;
}