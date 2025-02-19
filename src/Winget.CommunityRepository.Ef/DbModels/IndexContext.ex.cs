using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Winget.CommunityRepository.DbModels;
public partial class IndexContext
{
    private static DbContextOptions<IndexContext> optionsBuilder(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IndexContext>();
        optionsBuilder.UseSqlite(connectionString);
        return optionsBuilder.Options;
    }
    public IndexContext(string connectionString) : base(optionsBuilder(connectionString))
    {
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Manifest>()
            .HasOne(m => m.IdValue)
            .WithMany()
            .HasForeignKey(m => m.Id);

        modelBuilder.Entity<Manifest>()
            .HasOne(m => m.NameValue)
            .WithMany()
            .HasForeignKey(m => m.Name);

        modelBuilder.Entity<Manifest>()
            .HasOne(m => m.VersionValue)
            .WithMany()
            .HasForeignKey(m => m.Version);

        modelBuilder.Entity<Manifest>()
            .HasMany(m => m.Tags)
            .WithMany(t => t.Manifests)
            .UsingEntity<TagsMap>()
            ;

        //modelBuilder.Entity<TagsMap>()
        //    .HasOne(tm => tm.ManifestValue)
        //    .WithMany(m => m.TagsMaps)
        //    .HasForeignKey(tm => tm.Manifest);

        //modelBuilder.Entity<TagsMap>()
        //    .HasOne(tm => tm.TagValue)
        //    .WithMany(t => t.TagsMaps)
        //    .HasForeignKey(tm => tm.Tag);
    }

    public override void Dispose()
    {
        if (Database.GetDbConnection() is SqliteConnection sqliteConnection)
        {
            SqliteConnection.ClearPool(sqliteConnection);
        }
        base.Dispose();
    }
}
