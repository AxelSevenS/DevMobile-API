using System.Text.Json.Serialization;

namespace Kredit;

public record Media
{
	[JsonPropertyName("id")]
    public uint Id { get; set; }

	[JsonPropertyName("authorId")]
    public uint AuthorId { get; set; }
	
	[JsonPropertyName("name")]
    public string? Name { get; set; }
	
	[JsonPropertyName("description")]
    public string? Description { get; set; }
	
	[JsonPropertyName("extension")]
    public string Extension { get; set; } = ".png";
	

	public string GetFileName()
	{
		return $"{Id}.{Extension}";
	}
}