using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            .HasOne(m => m.NameValue)
            .WithMany()
            .HasForeignKey(m => m.Name);

        modelBuilder.Entity<Manifest>()
            .HasOne(m => m.VersionValue)
            .WithMany()
            .HasForeignKey(m => m.Version);
    }

    public override void Dispose()
    {
        Database.GetDbConnection().Close();
        base.Dispose();
    }
}
