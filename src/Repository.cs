using System.Text.Json;

namespace Kredit;

public abstract class Repository<T> where T : class {


    protected readonly List<T> Data = [];

	public abstract uint NewId { get; }

    public Repository(string fileName)
    {
        using FileStream? file = File.Open(fileName, FileMode.OpenOrCreate);

        if (file.Length > 0)
        {
            Span<byte> buffer = new byte[file.Length];
            file.Read(buffer);

            Data = JsonSerializer.Deserialize<List<T>>(buffer) ?? [];
        }
        else
        {
            Data = [];
        }
    }


    public abstract void SaveChanges();
}