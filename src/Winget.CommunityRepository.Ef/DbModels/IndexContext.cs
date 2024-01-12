using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Winget.CommunityRepository.DbModels;

public partial class IndexContext : DbContext
{
    public IndexContext()
    {
    }

    public IndexContext(DbContextOptions<IndexContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<Command> Commands { get; set; }

    public virtual DbSet<CommandsMap> CommandsMaps { get; set; }

    public virtual DbSet<Id> Ids { get; set; }

    public virtual DbSet<Manifest> Manifests { get; set; }

    public virtual DbSet<Metadata> Metadata { get; set; }

    public virtual DbSet<Moniker> Monikers { get; set; }

    public virtual DbSet<Name> Names { get; set; }

    public virtual DbSet<NormName> NormNames { get; set; }

    public virtual DbSet<NormNamesMap> NormNamesMaps { get; set; }

    public virtual DbSet<NormPublisher> NormPublishers { get; set; }

    public virtual DbSet<NormPublishersMap> NormPublishersMaps { get; set; }

    public virtual DbSet<Pathpart> Pathparts { get; set; }

    public virtual DbSet<Pfn> Pfns { get; set; }

    public virtual DbSet<PfnsMap> PfnsMaps { get; set; }

    public virtual DbSet<Productcode> Productcodes { get; set; }

    public virtual DbSet<ProductcodesMap> ProductcodesMaps { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagsMap> TagsMaps { get; set; }

    public virtual DbSet<Upgradecode> Upgradecodes { get; set; }

    public virtual DbSet<UpgradecodesMap> UpgradecodesMaps { get; set; }

    public virtual DbSet<Version> Versions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("channels");

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Channel1).HasColumnName("channel");
        });

        modelBuilder.Entity<Command>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("commands");

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Command1).HasColumnName("command");
        });

        modelBuilder.Entity<CommandsMap>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("commands_map");

            entity.HasIndex(e => new { e.Command, e.Manifest }, "commands_map_pkindex").IsUnique();

            entity.Property(e => e.Command)
                .HasColumnType("INT64")
                .HasColumnName("command");
            entity.Property(e => e.Manifest)
                .HasColumnType("INT64")
                .HasColumnName("manifest");
        });

        modelBuilder.Entity<Id>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("ids");

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Id1).HasColumnName("id");
        });

        modelBuilder.Entity<Manifest>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("manifest");

            entity.HasIndex(e => e.Id, "manifest_id_index");

            entity.HasIndex(e => e.Moniker, "manifest_moniker_index");

            entity.HasIndex(e => e.Name, "manifest_name_index");

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.ArpMaxVersion)
                .HasColumnType("INT64")
                .HasColumnName("arp_max_version");
            entity.Property(e => e.ArpMinVersion)
                .HasColumnType("INT64")
                .HasColumnName("arp_min_version");
            entity.Property(e => e.Channel)
                .HasColumnType("INT64")
                .HasColumnName("channel");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.Id)
                .HasColumnType("INT64")
                .HasColumnName("id");
            entity.Property(e => e.Moniker)
                .HasColumnType("INT64")
                .HasColumnName("moniker");
            entity.Property(e => e.Name)
                .HasColumnType("INT64")
                .HasColumnName("name");
            entity.Property(e => e.Pathpart)
                .HasColumnType("INT64")
                .HasColumnName("pathpart");
            entity.Property(e => e.Version)
                .HasColumnType("INT64")
                .HasColumnName("version");
        });

        modelBuilder.Entity<Metadata>(entity =>
        {
            entity.HasKey(e => e.Name);

            entity.ToTable("metadata");

            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Moniker>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("monikers");

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Moniker1).HasColumnName("moniker");
        });

        modelBuilder.Entity<Name>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("names");

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Name1).HasColumnName("name");
        });

        modelBuilder.Entity<NormName>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("norm_names");

            entity.HasIndex(e => e.NormName1, "norm_names_pkindex").IsUnique();

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.NormName1).HasColumnName("norm_name");
        });

        modelBuilder.Entity<NormNamesMap>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("norm_names_map");

            entity.HasIndex(e => e.Manifest, "norm_names_map_index");

            entity.HasIndex(e => new { e.NormName, e.Manifest }, "norm_names_map_pkindex").IsUnique();

            entity.Property(e => e.Manifest)
                .HasColumnType("INT64")
                .HasColumnName("manifest");
            entity.Property(e => e.NormName)
                .HasColumnType("INT64")
                .HasColumnName("norm_name");
        });

        modelBuilder.Entity<NormPublisher>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("norm_publishers");

            entity.HasIndex(e => e.NormPublisher1, "norm_publishers_pkindex").IsUnique();

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.NormPublisher1).HasColumnName("norm_publisher");
        });

        modelBuilder.Entity<NormPublishersMap>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("norm_publishers_map");

            entity.HasIndex(e => e.Manifest, "norm_publishers_map_index");

            entity.HasIndex(e => new { e.NormPublisher, e.Manifest }, "norm_publishers_map_pkindex").IsUnique();

            entity.Property(e => e.Manifest)
                .HasColumnType("INT64")
                .HasColumnName("manifest");
            entity.Property(e => e.NormPublisher)
                .HasColumnType("INT64")
                .HasColumnName("norm_publisher");
        });

        modelBuilder.Entity<Pathpart>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("pathparts");

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Parent)
                .HasColumnType("INT64")
                .HasColumnName("parent");
            entity.Property(e => e.Pathpart1).HasColumnName("pathpart");
        });

        modelBuilder.Entity<Pfn>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("pfns");

            entity.HasIndex(e => e.Pfn1, "pfns_pkindex").IsUnique();

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Pfn1).HasColumnName("pfn");
        });

        modelBuilder.Entity<PfnsMap>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("pfns_map");

            entity.HasIndex(e => e.Manifest, "pfns_map_index");

            entity.HasIndex(e => new { e.Pfn, e.Manifest }, "pfns_map_pkindex").IsUnique();

            entity.Property(e => e.Manifest)
                .HasColumnType("INT64")
                .HasColumnName("manifest");
            entity.Property(e => e.Pfn)
                .HasColumnType("INT64")
                .HasColumnName("pfn");
        });

        modelBuilder.Entity<Productcode>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("productcodes");

            entity.HasIndex(e => e.Productcode1, "productcodes_pkindex").IsUnique();

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Productcode1).HasColumnName("productcode");
        });

        modelBuilder.Entity<ProductcodesMap>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("productcodes_map");

            entity.HasIndex(e => e.Manifest, "productcodes_map_index");

            entity.HasIndex(e => new { e.Productcode, e.Manifest }, "productcodes_map_pkindex").IsUnique();

            entity.Property(e => e.Manifest)
                .HasColumnType("INT64")
                .HasColumnName("manifest");
            entity.Property(e => e.Productcode)
                .HasColumnType("INT64")
                .HasColumnName("productcode");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("tags");

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Tag1).HasColumnName("tag");
        });

        modelBuilder.Entity<TagsMap>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tags_map");

            entity.HasIndex(e => new { e.Tag, e.Manifest }, "tags_map_pkindex").IsUnique();

            entity.Property(e => e.Manifest)
                .HasColumnType("INT64")
                .HasColumnName("manifest");
            entity.Property(e => e.Tag)
                .HasColumnType("INT64")
                .HasColumnName("tag");
        });

        modelBuilder.Entity<Upgradecode>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("upgradecodes");

            entity.HasIndex(e => e.Upgradecode1, "upgradecodes_pkindex").IsUnique();

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Upgradecode1).HasColumnName("upgradecode");
        });

        modelBuilder.Entity<UpgradecodesMap>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("upgradecodes_map");

            entity.HasIndex(e => e.Manifest, "upgradecodes_map_index");

            entity.HasIndex(e => new { e.Upgradecode, e.Manifest }, "upgradecodes_map_pkindex").IsUnique();

            entity.Property(e => e.Manifest)
                .HasColumnType("INT64")
                .HasColumnName("manifest");
            entity.Property(e => e.Upgradecode)
                .HasColumnType("INT64")
                .HasColumnName("upgradecode");
        });

        modelBuilder.Entity<Version>(entity =>
        {
            entity.HasKey(e => e.Rowid);

            entity.ToTable("versions");

            entity.Property(e => e.Rowid)
                .ValueGeneratedNever()
                .HasColumnName("rowid");
            entity.Property(e => e.Version1).HasColumnName("version");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
