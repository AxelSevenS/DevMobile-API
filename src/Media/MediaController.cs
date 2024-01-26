using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using static ApiSevenet.JWT;

namespace ApiSevenet;

[ApiController]
[Route("api/media")]
public class MediaController(MediaRepository repository) : Controller<MediaRepository, Media>(repository)
{
	private static readonly string[] acceptedTypes = [
		"image/jpeg",
		"image/png",
		"image/gif",
		"audio/x-wav",
		"audio/mp3",
		"audio/ogg",
		"audio/webm",
		"video/mp4",
		"video/webm",
		"video/x-msvideo",
		"video/mpeg",
		"video/ogg",
	];


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
    public async Task<ActionResult<Media?>> GetById(string id)
    {
        if ( ! Guid.TryParse(id, out Guid guid) && guid == Guid.Empty)
        {
            return BadRequest();
        }

		return await repository.GetMediaById(guid);
    }

    /// <summary>
    /// Get a media by id
    /// </summary>
    /// <param name="id">The id of the media</param>
    /// <returns>
    /// The media with the given id
    /// </returns>
    [HttpGet("byAuthor/{id}")]
    public async Task<ActionResult<IEnumerable<Media?>>> GetByAuthorId(Guid id)
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
    public async Task<ActionResult<Media>> Create([FromForm] Media media)
    {		
		if ( 
			! Request.HasFormContentType || 
			Request.Form.Files.Where(f => acceptedTypes.Contains(f.ContentType)).FirstOrDefault() is not IFormFile file
		)
		{
			return BadRequest("No correct file attached");
		}

        if ( !IsAuthValid(Request, out JWT token) )
		{
            return Unauthorized();
        }

		// TODO: Implement file analysis to check if a file already exists.

        await repository.CreateMedia( media = media with
			{
				Id = Guid.NewGuid(),
				Author = token.GetDecodedPayload().user.Id,
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
    public async Task<ActionResult<Media>> Update(string id, [FromForm] Media media)
    {
        if ( ! Guid.TryParse(id, out Guid guid) && guid == Guid.Empty)
        {
            return BadRequest();
        }
		
		Media? currentProduct = await repository.GetMediaById(guid);
		if ( currentProduct is null )
		{
			return NotFound();
		}

        Media? result = await repository.UpdateMedia(currentProduct.Id, media with 
			{
				Id = currentProduct.Id,
				Author = currentProduct.Author,
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
    public async Task<ActionResult> Delete([FromQuery] string id)
    {
        if ( ! Guid.TryParse(id, out Guid guid) || guid == Guid.Empty)
        {
            return BadRequest();
        }
		
        Media? media = await repository.GetMediaById(guid);
		if ( media is null )
		{
			return NotFound();
		}

		if ( ! IsAuthValid(Request, out JWT token) || (! token.GetDecodedPayload().user.Admin && token.GetDecodedPayload().user.Id != media.Author))
		{
			return Unauthorized();
		}

		await repository.DeleteMedia(guid);
        repository.SaveChanges();
        return Ok(media);
    }
}