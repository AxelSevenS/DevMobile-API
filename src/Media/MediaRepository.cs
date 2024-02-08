using System.Text.Json;

namespace Kredit;

public class MediaRepository : Repository<Media>
{
    public static readonly string fileDirectory = "Resources/Media";
    public static readonly string fileName = "data.json";

	public override uint NewId => (Data.MaxBy(m => m.Id)?.Id ?? 0) + 1;

	public MediaRepository() : base(fileName) {}


    /// <summary>
    /// Save the data to the file
    /// </summary>
    public override void SaveChanges()
    {
        string jsonString = JsonSerializer.Serialize(Data);
        File.WriteAllText(fileName, jsonString);
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>
    /// All products
    /// </returns>
    public async Task<IEnumerable<Media>> GetMedia()
    {
        return await Task.Run(() => Data);
    }

    /// <summary>
    /// Get a product by id
    /// </summary>
    /// <param name="id">The id of the product</param>
    /// <remarks>
    /// This will not update the database, use <see cref="SaveChanges"/> to do that
    /// </remarks>
    /// <returns>
    /// The product with the given id
    /// </returns>
    public async Task<Media?> GetMediaById(uint id)
    {
        return await Task.Run(() => Data.FirstOrDefault(x => x.Id == id));
    }
	
    /// <summary>
    /// Get a product by id
    /// </summary>
    /// <param name="id">The id of the product</param>
    /// <remarks>
    /// This will not update the database, use <see cref="SaveChanges"/> to do that
    /// </remarks>
    /// <returns>
    /// The product with the given id
    /// </returns>
    public async Task<IEnumerable<Media?>> GetMediaByAuthorId(uint id)
    {
        return await Task.Run(() => Data.Where(x => x.AuthorId == id));
    }

    /// <summary>
    /// Post a product
    /// </summary>
    /// <param name="product">The product to add to the database</param>
    /// <remarks>
    /// This will not update the database, use <see cref="SaveChanges"/> to do that
    /// </remarks>
    /// <returns>
    /// The added product
    /// </returns>
    public async Task<Media?> CreateMedia(Media product, IFormFile file)
    {
		product.Extension = Path.GetExtension(file.FileName);

		Directory.CreateDirectory(fileDirectory);
		string path = Path.Combine(fileDirectory, product.GetFileName());

		using (FileStream stream = new(path, FileMode.Create))
		{
			await file.CopyToAsync(stream);
		}


		Data.Add(product);
		return product;
    }

    /// <summary>
    /// Update a product
    /// </summary>
    /// <param name="id">The id of the product</param>
    /// <param name="product">The product to update</param>
    /// <remarks>
    /// This will not update the database, use <see cref="SaveChanges"/> to do that
    /// </remarks>
    /// <returns>
    /// The updated product
    /// </returns>
    public async Task<Media?> UpdateMedia(uint id, Media product)
    {
        Media? productToUpdate = await GetMediaById(id);
        if (productToUpdate is not null)
        {
            productToUpdate = Data[Data.IndexOf(productToUpdate)] = productToUpdate with
            {
                Name = product.Name,
                Description = product.Description,
            };
        }
        
        return productToUpdate;
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">The id of the product</param>
    /// <remarks>
    /// This will not update the database, use <see cref="SaveChanges"/> to do that
    /// </remarks>
    /// <returns>
    /// The deleted product
    /// </returns>
    public async Task<Media?> DeleteMedia(uint id)
    {
        Media? product = await GetMediaById(id);
        if (product is not null)
        {
			File.Delete(Path.Combine(fileDirectory, product.GetFileName()));
            Data.Remove(product);
        }

        return product;
    }
}