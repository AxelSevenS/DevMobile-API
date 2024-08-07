using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kredit;

[ApiController]
[Route("api/users")]
public class UserController(UserRepository repo, JwtOptions jwtOptions) : Controller<UserRepository, User>(repo)
{
	/// <summary>
	/// Get all users
	/// </summary>
	/// <returns>
	/// All users
	/// </returns>
	[HttpGet]
    public async Task<List<User>> GetAll() =>
		[.. await repository.GetUsers()];

    /// <summary>
    /// Get a user by id
    /// </summary>
    /// <param name="id">The id of the user</param>
    /// <returns>
    /// The user with the given id
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(uint id) =>
		await repository.GetUserById(id) switch
        {
            User user => Ok(user),
            null => NotFound(),
        };

    /// <summary>
    /// Authenticate a user
    /// </summary>
    /// <param name="username">The username of the user</param>
    /// <param name="password">The password of the user</param>
    /// <returns>
    /// The JWT token of the user,
    ///     or NotFound if the user does not exist
    /// </returns>
    [HttpPost("auth")]
    public async Task<ActionResult> AuthenticateUser([FromForm]string username, [FromForm]string password)
	{
		password = jwtOptions.HashPassword(password);
		return await repository.GetUserByUsernameAndPassword(username, password) switch
        {
            User user => Ok( JsonSerializer.Serialize(jwtOptions.GenerateFrom(user).Write()) ),
            null => NotFound(),
        };
	}

    /// <summary>
    /// Register a user
    /// </summary>
    /// <param name="username">The username of the user</param>
    /// <param name="password">The password of the user</param>
    /// <returns>
    /// The user,
    ///    or BadRequest if the user already exists
    /// </returns>
    [HttpPut]
    public async Task<ActionResult<User>> RegisterUser([FromForm]string username, [FromForm]string password)
    {
        User? result = await repository.PostUser( 
			new()
			{
				Username = username,
				Password = jwtOptions.HashPassword(password)
			}
		);

        if (result is not User user)
        {
            return BadRequest();
        }

        repository.SaveChanges();
        return Ok(user);
    }

    /// <summary>
    /// Update a user
    /// </summary>
    /// <param name="id">The id of the user</param>
    /// <param name="user">The user to update</param>
    /// <returns>
    /// The updated user
    /// </returns>
    [HttpPatch("{id}")]
	[Authorize]
    public async Task<ActionResult<User>> UpdateUser(uint id, [FromForm] string? username, [FromForm] string? password, [FromForm] string? roles)
    {
        if (username is null && password is null && roles is null)
        {
            return BadRequest();
        }
		
		if ( ! VerifyOwnershipOrAuthZ(id, out ActionResult<User> error))
		{
			return error;
		}


		User? current = await repository.GetUserById(id);
        if ( current is null )
        {
            return NotFound();
        }
        if ((await repository.GetUsers()).Any(u => u.Username == username && u.Id != id))
        {
            return BadRequest("Username already taken");
        }

        bool isAdmin = HttpContext.User.FindFirstValue(JwtOptions.RoleClaim) is string authenticatedRoles && authenticatedRoles == "Admin";

		User? updated = await repository.PutUserById(id, current with
        {
            Username = username ?? current.Username,
            Password = password ?? current.Password,
            Roles = roles is not null && isAdmin ? roles : current.Roles
        });

        repository.SaveChanges();
        return Ok(updated);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">The id of the user</param>
    /// <returns>
    /// The deleted user
    /// </returns>
    [HttpDelete("{id}")]
	[Authorize]
    public async Task<ActionResult<User>> DeleteUser(uint id)
    {
		if ( ! VerifyOwnershipOrAuthZ(id, out ActionResult<User> error))
		{
			return error;
		}

		User? current = await repository.GetUserById(id);
        if ( current is null )
        {
            return NotFound();
        }

        User? deleted = await repository.DeleteUserById(id);

        repository.SaveChanges();
        return Ok(deleted);
    }
}