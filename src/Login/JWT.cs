using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ApiSevenet;

public record JWT {

    public static readonly string Secret = "Nawmn47obf1NiJD/+/BNDKD55iOY8ct695aa8J1smZgADdLurcGnSoWhbEjejG0tWvgtGCyJpyebTwI6EA1u8A==";

	public const uint durationDays = 1;


    public string Header { get; set; }
    public string Payload { get; set; }
    public string Signature { get; set; }

    private JWTHeader? decodedHeader = null;
    private JWTPayload? decodedPayload = null;


    public JWT(string header, string payload, string signature)
    {
        Header = header;
        Payload = payload;
        Signature = signature;
    }


    private static string Base64UrlEncode(string input)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input))
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }

    private static string Base64UrlDecode(string input)
    {
        input = input.Replace('-', '+').Replace('_', '/');
        switch (input.Length % 4)
        {
            case 0:
                break;
            case 2:
                input += "==";
                break;
            case 3:
                input += "=";
                break;
            default:
                throw new Exception("Illegal base64url string!");
        }

        return Encoding.UTF8.GetString(Convert.FromBase64String(input));
    }


    public JWTHeader GetDecodedHeader() =>
		decodedHeader ??=
        JsonSerializer.Deserialize<JWTHeader>(Base64UrlDecode(Header)) ?? throw new Exception("Invalid JWT header");

    public JWTPayload GetDecodedPayload() => 
		decodedPayload ??=
        JsonSerializer.Deserialize<JWTPayload>(Base64UrlDecode(Payload)) ?? throw new Exception("Invalid JWT payload");

    public string GetDecodedSignature() => 
        Base64UrlDecode(Signature);


    public static JWT? Parse(string? jwt)
    {
        if (jwt is null)
        {
            return null;
        }

        string[] parts = jwt.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        return new JWT(parts[0], parts[1], parts[2]);
    }

    public bool Verify()
    {
        if (GetDecodedPayload().exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            return false;
        }

		string generatedSignature = Base64UrlEncode(GenerateSignature());
        return generatedSignature == Signature;
    }

    public static JWT Generate(User user)
    {
        uint iat = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        uint exp = (uint)DateTimeOffset.UtcNow.AddDays(durationDays).ToUnixTimeSeconds();

        string header = Base64UrlEncode(JsonSerializer.Serialize(new JWTHeader()));
        string payload = Base64UrlEncode(JsonSerializer.Serialize(new JWTPayload()
        {
			user = user,
            iat = iat,
            exp = exp,
        }));
        string signature = Base64UrlEncode( GenerateSignature(header, payload) );

        return new JWT(header, payload, signature);
    }

    private static string GenerateSignature(string header, string payload)
    {
        byte[] secretBytes = Encoding.UTF8.GetBytes(Secret);
        using HMACSHA256? hmac = new(secretBytes);
        byte[] signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(header + "." + payload));
        return Base64UrlDecode(Convert.ToBase64String(signatureBytes));
    }

    public static string HashPassword(string password)
    {
        byte[] secretBytes = Encoding.UTF8.GetBytes(Secret);
        using var hmac = new HMACSHA256(secretBytes);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] passwordHash = hmac.ComputeHash(passwordBytes);
        return Convert.ToBase64String(passwordHash);
    }

    public string GenerateSignature() =>
        GenerateSignature(Header, Payload);

    public override string ToString() =>
        Header + "." + Payload + "." + Signature;



    public record class JWTHeader
    {
        public string typ { get; set; } = "JWT";
        public string alg { get; set; } = "SHA256";
    }

    public record class JWTPayload
    {
		public User user { get; set; } = new();
        public uint iat { get; set; } = 0;
        public uint exp { get; set; } = 0;
    }

    
}