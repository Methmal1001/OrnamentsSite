using LankaOrnaments.Core.Constants;
using LankaOrnaments.Core.Contracts;
using LankaOrnaments.Core.Entities;
using LankaOrnaments.Core.Exceptions;
using LankaOrnaments.Core.Interfaces;
using LankaOrnaments.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LankaOrnaments.Infrastructure.Services;

internal sealed class OrnamentService(LankaOrnamentsDbContext db, TimeProvider timeProvider) : IOrnamentService
{
    public async Task<IReadOnlyList<OrnamentDto>> ListAsync(OrnamentQuery? query = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Ornament> ornaments = db.Ornaments.AsNoTracking();
        query ??= new OrnamentQuery();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Parameterised LIKE; the default SQL Server collation is case-insensitive.
            var pattern = $"%{EscapeLike(query.Search.Trim())}%";
            ornaments = ornaments.Where(o =>
                EF.Functions.Like(o.Name, pattern, @"\")
                || EF.Functions.Like(o.Description, pattern, @"\")
                || EF.Functions.Like(o.Sku, pattern, @"\")
                || EF.Functions.Like(o.Metal, pattern, @"\")
                || (o.Gemstone != null && EF.Functions.Like(o.Gemstone, pattern, @"\")));
        }

        if (!string.IsNullOrEmpty(query.Category))
        {
            ornaments = ornaments.Where(o => o.Category == query.Category);
        }

        // Mirrors Mapping.AvailabilityOf so filters match the labels shown in the UI.
        ornaments = query.Availability switch
        {
            Availability.OutOfStock => ornaments.Where(o => !o.IsAvailable),
            Availability.PreOrder => ornaments.Where(o => o.IsAvailable && o.StockQuantity == 0),
            Availability.LowStock => ornaments.Where(o => o.IsAvailable && o.StockQuantity > 0 && o.StockQuantity <= Availability.LowStockThreshold),
            Availability.InStock => ornaments.Where(o => o.IsAvailable && o.StockQuantity > Availability.LowStockThreshold),
            _ => ornaments,
        };

        ornaments = query.Sort switch
        {
            "newest" => ornaments.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.OrnamentId),
            "price-asc" => ornaments.OrderBy(o => o.Price).ThenBy(o => o.OrnamentId),
            "price-desc" => ornaments.OrderByDescending(o => o.Price).ThenBy(o => o.OrnamentId),
            _ => ornaments.OrderByDescending(o => o.IsFeatured).ThenByDescending(o => o.CreatedAt).ThenByDescending(o => o.OrnamentId),
        };

        var list = await ornaments.ToListAsync(cancellationToken);
        return list.Select(Mapping.ToDto).ToList();
    }

    private static string EscapeLike(string value) =>
        value.Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal)
            .Replace("[", @"\[", StringComparison.Ordinal);

    public async Task<OrnamentDto> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var ornament = await db.Ornaments.AsNoTracking().FirstOrDefaultAsync(o => o.OrnamentId == id, cancellationToken)
            ?? throw new NotFoundException("Ornament not found");
        return Mapping.ToDto(ornament);
    }

    public async Task<OrnamentDto> CreateAsync(OrnamentInput input, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var ornament = new Ornament { CreatedAt = now, UpdatedAt = now };
        Mapping.Apply(input, ornament);
        await EnsureSkuIsFreeAsync(ornament.Sku, exceptId: null, cancellationToken);

        db.Ornaments.Add(ornament);
        await SaveAsync(cancellationToken);
        return Mapping.ToDto(ornament);
    }

    public async Task<OrnamentDto> UpdateAsync(int id, OrnamentInput input, CancellationToken cancellationToken = default)
    {
        var ornament = await db.Ornaments.FirstOrDefaultAsync(o => o.OrnamentId == id, cancellationToken)
            ?? throw new NotFoundException("Ornament not found");

        Mapping.Apply(input, ornament);
        ornament.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await EnsureSkuIsFreeAsync(ornament.Sku, exceptId: id, cancellationToken);

        await SaveAsync(cancellationToken);
        return Mapping.ToDto(ornament);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await db.Ornaments.Where(o => o.OrnamentId == id).ExecuteDeleteAsync(cancellationToken);
        if (deleted == 0) throw new NotFoundException("Ornament not found");
    }

    private async Task EnsureSkuIsFreeAsync(string sku, int? exceptId, CancellationToken cancellationToken)
    {
        if (await db.Ornaments.AnyAsync(o => o.Sku == sku && o.OrnamentId != exceptId, cancellationToken))
        {
            throw new ConflictException($"An ornament with SKU {sku} already exists");
        }
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // Unique index race (two requests with the same SKU).
            throw new ConflictException("An ornament with this SKU already exists");
        }
    }
}
