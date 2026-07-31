namespace AspireNext.Server.Models;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<Product> Products { get; set; } = [];
}
