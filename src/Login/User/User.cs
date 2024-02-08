using System.Text.Json.Serialization;

namespace ApiSevenet;

public record User
{
	[JsonPropertyName("id")]
    public uint Id { get; set; }

	[JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

	[JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

	[JsonPropertyName("admin")]
    public bool Admin { get; set; } = false;

	public User WithUpdatesFrom(User other) {
		return this with 
		{
			Username = string.IsNullOrEmpty(other.Username) ? Username : other.Username,
			Password = string.IsNullOrEmpty(other.Password) ? Password : other.Password
		};
	}
}