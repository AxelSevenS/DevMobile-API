using System.Text.Json;
using Microsoft.AspNetCore.Mvc;


namespace ApiSevenet;

[ApiController]
[Route("api/users")]
public class UserController(UserRepository repository) : Controller<UserRepository, User>(repository)
{

	/// <summary>
	/// Get all users
	/// </summary>
	/// <returns>
	/// All users
	/// </returns>
	[HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll()
    {
        return Ok(await repository.GetUsers());
    }

    /// <summary>
    /// Get a user by id
    /// </summary>
    /// <param name="id">The id of the user</param>
    /// <returns>
    /// The user with the given id
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(string id)
    {
        if ( ! Guid.TryParse(id, out Guid guid) && guid == Guid.Empty)
        {
            return BadRequest();
        }

        return await repository.GetUserById(guid) switch
        {
            null => NotFound(),
            User user => Ok(user),
        };
    }

    /// <summary>
    /// Authenticate a user
    /// </summary>
    /// <param name="username">The username of the user</param>
    /// <param name="password">The password of the user</param>
    /// <returns>
    /// The JWT token of the user,
    ///     or NotFound if the user does not exist,
    ///     or BadRequest if the username/password is incorrect
    /// </returns>
    [HttpPost("auth")]
    public async Task<ActionResult> AuthenticateUser([FromForm]string username, [FromForm]string password)
    {
		return await repository.GetUserByUsernameAndPassword(username, JWT.HashPassword(password)) switch
        {
            OkObjectResult res when res.Value is User user => Ok(JsonSerializer.Serialize(JWT.Generate(user).ToString())),
			ActionResult res => res
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
        User? result = await repository.PostUser( new()
			{
				Username = username,
				Password = JWT.HashPassword(password)
			});

        if (result is null)
        {
            return BadRequest();
        }

        repository.SaveChanges();
        return Ok(result);
    }

    /// <summary>
    /// Update a user
    /// </summary>
    /// <param name="id">The id of the user</param>
    /// <param name="user">The user to update</param>
    /// <returns>
    /// The updated user
    /// </returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUser(string id, [FromForm] User user)
    {
        if ( (! Guid.TryParse(id, out Guid guid) && guid == Guid.Empty) || user is null)
        {
            return BadRequest();
        }

        User? result = await repository.PutUserById(guid, user);
        if (result is null)
        {
            return NotFound();
        }

        repository.SaveChanges();
        return Ok(result);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">The id of the user</param>
    /// <returns>
    /// The deleted user
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<User>> DeleteUser(string id)
    {
        if ( ! Guid.TryParse(id, out Guid guid) && guid == Guid.Empty)
        {
            return BadRequest();
        }

        User? result = await repository.DeleteUserById(guid);
        if (result is null)
        {
            return NotFound();
        }

        repository.SaveChanges();
        return Ok(result);
    }
}