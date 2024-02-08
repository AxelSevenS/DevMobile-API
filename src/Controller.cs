using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace ApiSevenet;

public abstract class Controller<T, TData>(T repository) : ControllerBase where T : Repository<TData> where TData : class
{
    protected readonly T repository = repository;

	/// <summary>
	/// Verifies wether the user is authenticated and if it is, set <c>id</c> to the authenticated user's Id.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	protected bool TryGetAuthenticatedUserId(out uint id)
	{
		id = 0;
		if (
			HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) is Claim claim && 
			uint.TryParse(claim.Value, out id)
		)
		{
			return id != 0;
		}

		return false;
	}

	/// <summary>
	/// Verify if the current authenticated user exists and has the given Id as its own
	/// </summary>
	/// <param name="validId"></param>
	/// <returns>True if the Authenticated user exists and has the given Id, False if the user is not authenticated or doesn't fit the given Id</returns>
	protected bool VerifyAuthenticatedId(uint validId) =>
		TryGetAuthenticatedUserId(out uint authenticatedId) && authenticatedId == validId;

	/// <summary>
	/// Verifies wether the current authenticated user has the given authorizations, if not <c>result</c> will contain a StatusCodeResult corresponding to the error.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="neededAuthorizations">The authorizations the authenticated user needs to posess to validate the check</param>
	/// <param name="authId">The Id of the currently authenticated user, if the function returns True</param>
	/// <param name="result">The StatusCodeResult corresponding to the error, if the function returns False</param>
	/// <returns>
	/// True if the user is authenticated and posesses the given <c>authorizations</c>, otherwise False.
	/// </returns>
	protected bool VerifyAuthZ(out uint authId, out ActionResult<TData> result)
	{
		if ( ! TryGetAuthenticatedUserId(out authId) )
		{
			result = Unauthorized();
			return false;
		}
		if ( ! HttpContext.User.IsInRole("Admin") )
		{
			result = Forbid();
			return false;
		}

		result = null!;
		return true;
	}

	/// <summary>
	/// Verifies wether the current authenticated user has the given authorizations, if not <c>result</c> will contain a StatusCodeResult corresponding to the error.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="authId">The Id that the authenticated user needs to have to be deemed "Owner" of the resource</param>
	/// <param name="neededAuthorizations">The authorizations the authenticated user needs to posess to validate the check and override the Ownership check</param>
	/// <param name="result">The StatusCodeResult corresponding to the error if the verification was unsuccessful</param>
	/// <returns>
	/// True if the user is authenticated and posesses the given <c>authorizations</c> OR has <c>authId</c> as their Id, otherwise False.
	/// </returns>
	protected bool VerifyOwnershipOrAuthZ(uint authId, out ActionResult<TData> result)
	{
		if ( ! TryGetAuthenticatedUserId(out uint currentId) )
		{
			result = Unauthorized();
			return false;
		}
		if ( authId != currentId && ! HttpContext.User.IsInRole("Admin") )
		{
			result = Forbid();
			return false;
		}

		result = null!;
		return true;
	}

}