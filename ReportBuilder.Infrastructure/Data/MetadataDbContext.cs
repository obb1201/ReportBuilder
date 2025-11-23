using Microsoft.EntityFrameworkCore;
using ReportBuilder.Infrastructure.Data.Entities;

namespace ReportBuilder.Infrastructure.Data;

/// <summary>
/// Database context for metadata storage
/// </summary>
public class MetadataDbContext : DbContext
{
    public MetadataDbContext(DbContextOptions<MetadataDbContext> options)
        : base(options)
    {
    }

    public DbSet<MetadataObjectEntity> MetadataObjects { get; set; }
    public DbSet<MetadataFieldEntity> MetadataFields { get; set; }
    public DbSet<MetadataRelationshipEntity> MetadataRelationships { get; set; }
    public DbSet<ChildRelationshipEntity> ChildRelationships { get; set; }
    public DbSet<MetadataSyncStatusEntity> MetadataSyncStatus { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MetadataObject configuration
        modelBuilder.Entity<MetadataObjectEntity>(entity =>
        {
            entity.HasIndex(e => e.ApiName).IsUnique();
            entity.HasIndex(e => e.Label);
            entity.HasIndex(e => e.IsCustom);
            entity.HasIndex(e => e.LastSyncedAt);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            entity.HasMany(e => e.Fields)
                .WithOne(e => e.Object)
                .HasForeignKey(e => e.ObjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Relationships)
                .WithOne(e => e.FromObject)
                .HasForeignKey(e => e.FromObjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ChildRelationships)
                .WithOne(e => e.ParentObject)
                .HasForeignKey(e => e.ParentObjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MetadataField configuration
        modelBuilder.Entity<MetadataFieldEntity>(entity =>
        {
            entity.HasIndex(e => new { e.ObjectId, e.ApiName }).IsUnique();
            entity.HasIndex(e => e.DataType);
            entity.HasIndex(e => e.IsCustom);
            entity.HasIndex(e => e.IsCalculated);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // MetadataRelationship configuration
        modelBuilder.Entity<MetadataRelationshipEntity>(entity =>
        {
            entity.HasIndex(e => new { e.FromObjectId, e.FromField });
            entity.HasIndex(e => e.ToObject);
            entity.HasIndex(e => e.RelationshipName);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // ChildRelationship configuration
        modelBuilder.Entity<ChildRelationshipEntity>(entity =>
        {
            entity.HasIndex(e => new { e.ParentObjectId, e.ChildObject });
            entity.HasIndex(e => e.RelationshipName);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // MetadataSyncStatus configuration
        modelBuilder.Entity<MetadataSyncStatusEntity>(entity =>
        {
            entity.HasIndex(e => e.LastSyncCompleted);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
