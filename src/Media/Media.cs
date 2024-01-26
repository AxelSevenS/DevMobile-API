namespace ApiSevenet;

public record Media
{
    public Guid Id { get; set; }
    public Guid Author { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string Extension { get; set; } = ".png";
	

	public string GetFileName()
	{
		return $"{Id}{Extension}";
	}
}