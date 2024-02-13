using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kredit;

[ApiController]
[Route("api/media")]
public class MediaController(MediaRepository repository) : Controller<MediaRepository, Media>(repository)
{

	private static bool CheckFileValidity(IFormFile file)
	{
		return file.ContentType.StartsWith("audio") || file.ContentType.StartsWith("video") || file.ContentType.StartsWith("image");
	}

    /// <summary>
    /// Get all media
    /// </summary>
    /// <returns>
    /// All media
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Media>>> GetAll()
    {
        return Ok(await repository.GetMedia());
    }

    /// <summary>
    /// Get a media by id
    /// </summary>
    /// <param name="id">The id of the media</param>
    /// <returns>
    /// The media with the given id
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Media?>> GetById(uint id) =>
		await repository.GetMediaById(id);

    /// <summary>
    /// Get a media by id
    /// </summary>
    /// <param name="id">The id of the media</param>
    /// <returns>
    /// The media with the given id
    /// </returns>
    [HttpGet("byAuthor/{id}")]
    public async Task<ActionResult<IEnumerable<Media?>>> GetByAuthorId(uint id)
    {
		return Ok(await repository.GetMediaByAuthorId(id));
    }

    /// <summary>
    /// Creates a new media
    /// </summary>
    /// <param name="media">The Product to add to the database</param>
    /// <returns>
    /// The added Product
    /// </returns>
    [HttpPut]
	[Authorize]
    public async Task<ActionResult<Media>> Create([FromForm] string name, [FromForm] string description, [FromForm] IFormFile file)
    {		
		if ( !CheckFileValidity(file) )
		{
			return BadRequest("No correct file attached");
		}

        if ( ! TryGetAuthenticatedUserId(out uint id) )
		{
			return Unauthorized();
		}

		// TODO: Implement file analysis to check if a file already exists.

        Media? media = await repository.CreateMedia( new()
			{
				Id = repository.NewId,
				AuthorId = id,
                Name = name,
                Description = description,
			},
			file
		);
        
        repository.SaveChanges();
        return Ok(media);
    }

    /// <summary>
    /// Updates a media
    /// </summary>
    /// <param name="id">The id of the media</param>
    /// <param name="media">The media to update</param>
    /// <returns>
    /// The updated media
    /// </returns>
    [HttpPatch("{id}")]
    [Authorize]
    public async Task<ActionResult<Media>> Update(uint id, [FromForm] uint? authorId, [FromForm] string? name, [FromForm] string? description)
    {

        if (authorId is null && name is null && description is null)
        {
            return BadRequest();
        }

		Media? media = await repository.GetMediaById(id);
		if ( media is null )
		{
			return NotFound();
		}

        if ( ! VerifyOwnershipOrAuthZ(media.AuthorId, out ActionResult<Media> error) )
		{
			return error;
		}

        Media? result = await repository.UpdateMedia(media.Id, media with
        {
            AuthorId = authorId ?? media.AuthorId,
            Name = name ?? media.Name,
            Description = description ?? media.Description
        });

        if ( result is null )
        {
            return NotFound();
        }

        repository.SaveChanges();
        return Ok(result);
    }

    /// <summary>
    /// Delete a media
    /// </summary>
    /// <param name="id">The id of the media</param>
    /// <returns></returns>
    /// <response code="200">The media was deleted</response>
    /// <response code="404">The media was not found</response>
    /// <response code="400">The id was 0</response>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<Media>> Delete(uint id)
    {		
        Media? media = await repository.GetMediaById(id);
		if ( media is null )
		{
			return NotFound();
		}

        if ( ! VerifyOwnershipOrAuthZ(media.AuthorId, out ActionResult<Media> error) )
		{
			return error;
		}

		await repository.DeleteMedia(id);
        repository.SaveChanges();
        return Ok(media);
    }
}