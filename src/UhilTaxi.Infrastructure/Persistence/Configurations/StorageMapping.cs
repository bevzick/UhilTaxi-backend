using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UhilTaxi.Infrastructure.Persistence.Configurations;

internal static class StorageMapping
{
    public static void Configure<T>(EntityTypeBuilder<T> builder, string table) where T : class
    {
        builder.ToTable(table);
        foreach (var property in builder.Metadata.GetProperties().ToList())
        {
            builder.Property(property.Name).HasColumnName(JsonNamingPolicy.SnakeCaseLower.ConvertName(property.Name));
            if (property.ClrType == typeof(DateTime))
                builder.Property<DateTime>(property.Name).HasPrecision(0)
                    .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            else if (property.ClrType == typeof(DateTime?))
                builder.Property<DateTime?>(property.Name).HasPrecision(0)
                    .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);
        }
    }
}
