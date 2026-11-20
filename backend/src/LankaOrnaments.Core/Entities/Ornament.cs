namespace LankaOrnaments.Core.Entities;

public class Ornament
{
    public int OrnamentId { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>One of <see cref="Constants.OrnamentCategories"/>.</summary>
    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; }

    /// <summary>False when the piece is withdrawn from sale (reported as out of stock).</summary>
    public bool IsAvailable { get; set; }

    public string Metal { get; set; } = string.Empty;

    public decimal WeightGrams { get; set; }

    public string? Gemstone { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
