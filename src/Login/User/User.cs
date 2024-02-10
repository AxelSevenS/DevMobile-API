using System.Text.Json.Serialization;

namespace Kredit;

public record User
{
	[JsonPropertyName("id")]
    public uint Id { get; set; }

	[JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

	[JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

	[JsonPropertyName("roles")]
    public string Roles { get; set; } = "Client";

	public User WithUpdatesFrom(User other, bool editAuths) {
		return this with 
		{
			Username = string.IsNullOrEmpty(other.Username) ? Username : other.Username,
			Password = string.IsNullOrEmpty(other.Password) ? Password : other.Password,
			Roles = editAuths ? other.Roles : Roles
		};
	}
}