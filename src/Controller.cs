using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ApiSevenet;

public abstract class Controller<T, TData>(T repository) : ControllerBase where T : Repository<TData> where TData : class
{
    protected readonly T repository = repository;

	public static bool IsAuthValid(HttpRequest request, out JWT token) {
        var authorization = request.Headers.Authorization;
        string accessToken = authorization.FirstOrDefault()?.ToString()[7..] ?? "\"\""; // Remove "Bearer " prefix

        accessToken = JsonSerializer.Deserialize<string>(accessToken) ?? "";
        token = JWT.Parse(accessToken)!;

        return token is not null && token.Verify();
    }

}