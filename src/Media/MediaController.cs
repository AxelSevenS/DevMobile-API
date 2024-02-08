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
    public async Task<ActionResult<Media>> Create([FromForm] Media media, [FromForm] IFormFile file)
    {		
		if ( !CheckFileValidity(file) )
		{
			return BadRequest("No correct file attached");
		}

        if ( ! VerifyAuthZ(out uint id, out ActionResult<Media> error) )
		{
			return error;
		}

		// TODO: Implement file analysis to check if a file already exists.

        await repository.CreateMedia( media = media with
			{
				Id = repository.NewId,
				AuthorId = id,
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
    [HttpPut("{id}")]
    public async Task<ActionResult<Media>> Update(uint id, [FromForm] Media media)
    {
		Media? currentProduct = await repository.GetMediaById(id);
		if ( currentProduct is null )
		{
			return NotFound();
		}

        Media? result = await repository.UpdateMedia(currentProduct.Id, media with 
			{
				Id = currentProduct.Id,
				AuthorId = currentProduct.AuthorId,
				Name = media.Name ?? currentProduct.Name,
				Description = media.Description ?? currentProduct.Description,
			}
		);
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
    [HttpDelete]
    public async Task<ActionResult<Media>> Delete([FromQuery] uint id)
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