using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Entities;

namespace LankaOrnaments.Infrastructure.Data.Seed;

/// <summary>Sample catalogue (same pieces as the Phase 1 frontend demo data).</summary>
internal static class SampleOrnaments
{
    public static IEnumerable<Ornament> Create() =>
    [
        Item("LO-RG-001", "Ceylon Blue Sapphire Solitaire", OrnamentCategories.GoldRings,
            "A 1.2 carat cornflower-blue Ceylon sapphire set in a polished 18K yellow-gold band with a delicate cathedral setting.",
            385000m, 8, true, "18K Yellow Gold", 4.2m, "Blue Sapphire", true, "2026-10-28"),
        Item("LO-RG-002", "Classic 22K Wedding Band", OrnamentCategories.GoldRings,
            "A comfort-fit 22K gold band with a softly brushed centre and high-polish edges. Made to be worn every day.",
            118000m, 25, true, "22K Yellow Gold", 5.8m, null, false, "2026-09-14"),
        Item("LO-RG-003", "Padparadscha Halo Ring", OrnamentCategories.GoldRings,
            "A rare lotus-pink padparadscha sapphire framed by a halo of brilliant-cut diamonds in rose gold.",
            645000m, 2, true, "18K Rose Gold", 4.9m, "Padparadscha", true, "2026-11-10"),
        Item("LO-NK-001", "Kandyan Bridal Necklace", OrnamentCategories.Necklaces,
            "A traditional Kandyan-style bridal necklace with hand-chased gold medallions and ruby accents.",
            1250000m, 0, true, "22K Yellow Gold", 64m, "Ruby", true, "2026-08-02"),
        Item("LO-NK-002", "Freshwater Pearl Strand", OrnamentCategories.Necklaces,
            "Lustrous hand-knotted freshwater pearls finished with an 18K gold filigree clasp.",
            98000m, 12, true, "18K Yellow Gold", 22m, "Pearl", false, "2026-10-05"),
        Item("LO-NK-003", "Emerald Drop Choker", OrnamentCategories.Necklaces,
            "A slim gold choker holding a single pear-shaped emerald for a modern, elegant look.",
            312000m, 0, false, "18K Yellow Gold", 11.5m, "Emerald", false, "2026-07-21"),
        Item("LO-BR-001", "Diamond Tennis Bracelet", OrnamentCategories.Bracelets,
            "Forty brilliant-cut diamonds in a flexible white-gold line bracelet with a hidden safety clasp.",
            890000m, 3, true, "18K White Gold", 12.3m, "Diamond", true, "2026-11-02"),
        Item("LO-BR-002", "Twisted Rope Bangle", OrnamentCategories.Bracelets,
            "A solid 22K gold bangle with a hand-twisted rope texture that catches the light beautifully.",
            236000m, 9, true, "22K Yellow Gold", 15.6m, null, false, "2026-09-30"),
        Item("LO-BR-003", "Moonstone Charm Bracelet", OrnamentCategories.Bracelets,
            "Dainty gold chain with Sri Lankan moonstone charms that glow with a soft blue sheen.",
            74500m, 15, true, "18K Yellow Gold", 6.1m, "Moonstone", false, "2026-11-15"),
        Item("LO-ER-001", "Sapphire Jhumka Earrings", OrnamentCategories.Earrings,
            "Bell-shaped jhumkas in 22K gold with blue sapphire drops and fine granulation work.",
            168000m, 7, true, "22K Yellow Gold", 9.4m, "Blue Sapphire", true, "2026-10-19"),
        Item("LO-ER-002", "Pearl Drop Earrings", OrnamentCategories.Earrings,
            "Elegant South Sea pearls suspended from polished gold hooks. Timeless for any occasion.",
            64000m, 20, true, "18K Yellow Gold", 3.8m, "Pearl", false, "2026-08-25"),
        Item("LO-ER-003", "Ruby Cluster Studs", OrnamentCategories.Earrings,
            "Flower-shaped clusters of vivid rubies in a secure screw-back setting.",
            142000m, 0, false, "18K Rose Gold", 3.2m, "Ruby", false, "2026-06-11"),
        Item("LO-PD-001", "Cat's Eye Lotus Pendant", OrnamentCategories.Pendants,
            "A chrysoberyl cat's eye cabochon in a lotus-petal gold frame, symbolising purity and protection.",
            205000m, 6, true, "22K Yellow Gold", 5.4m, "Cat's Eye", true, "2026-11-18"),
        Item("LO-PD-002", "Yellow Sapphire Teardrop", OrnamentCategories.Pendants,
            "A sunny yellow sapphire teardrop in a minimalist bezel, on an 18-inch gold chain.",
            158000m, 1, true, "18K Yellow Gold", 4.1m, "Yellow Sapphire", false, "2026-10-01"),
        Item("LO-PD-003", "Diamond Initial Pendant", OrnamentCategories.Pendants,
            "A personalised pavé-diamond initial pendant, a meaningful everyday keepsake.",
            92000m, 0, true, "18K White Gold", 2.6m, "Diamond", false, "2026-11-08"),
        Item("LO-RG-004", "Emerald Eternity Ring", OrnamentCategories.GoldRings,
            "A full eternity band of calibré-cut emeralds in 18K yellow gold.",
            274000m, 11, true, "18K Yellow Gold", 3.6m, "Emerald", false, "2026-07-04"),
    ];

    private static Ornament Item(
        string sku, string name, string category, string description, decimal price, int stock, bool isAvailable,
        string metal, decimal weight, string? gemstone, bool isFeatured, string createdOn)
    {
        var createdAt = DateTime.SpecifyKind(DateTime.Parse(createdOn, System.Globalization.CultureInfo.InvariantCulture).AddHours(9), DateTimeKind.Utc);
        return new Ornament
        {
            Sku = sku,
            Name = name,
            Category = category,
            Description = description,
            Price = price,
            StockQuantity = stock,
            IsAvailable = isAvailable,
            Metal = metal,
            WeightGrams = weight,
            Gemstone = gemstone,
            IsFeatured = isFeatured,
            ImageUrl = null,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
    }
}
