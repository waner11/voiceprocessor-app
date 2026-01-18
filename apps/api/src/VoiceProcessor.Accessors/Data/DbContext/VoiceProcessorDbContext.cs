using Microsoft.EntityFrameworkCore;
using VoiceProcessor.Domain.Entities;

namespace VoiceProcessor.Accessors.Data.DbContext;

public class VoiceProcessorDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public VoiceProcessorDbContext(DbContextOptions<VoiceProcessorDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Voice> Voices => Set<Voice>();
    public DbSet<Generation> Generations => Set<Generation>();
    public DbSet<GenerationChunk> GenerationChunks => Set<GenerationChunk>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureVoice(modelBuilder);
        ConfigureGeneration(modelBuilder);
        ConfigureGenerationChunk(modelBuilder);
        ConfigureFeedback(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Name)
                .HasMaxLength(100);

            entity.Property(e => e.Tier)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private static void ConfigureVoice(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Voice>(entity =>
        {
            entity.ToTable("voices");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ProviderVoiceId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Provider)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.Language).HasMaxLength(10);
            entity.Property(e => e.Accent).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.AgeGroup).HasMaxLength(20);
            entity.Property(e => e.UseCase).HasMaxLength(50);
            entity.Property(e => e.PreviewUrl).HasMaxLength(500);
            entity.Property(e => e.CostPerThousandChars).HasPrecision(10, 6);

            entity.HasIndex(e => e.Provider);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.Provider, e.ProviderVoiceId }).IsUnique();
        });
    }

    private static void ConfigureGeneration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Generation>(entity =>
        {
            entity.ToTable("generations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.InputText).IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.RoutingPreference)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.SelectedProvider)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.AudioUrl).HasMaxLength(500);
            entity.Property(e => e.AudioFormat).HasMaxLength(10);
            entity.Property(e => e.EstimatedCost).HasPrecision(10, 6);
            entity.Property(e => e.ActualCost).HasPrecision(10, 6);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Generations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Voice)
                .WithMany(v => v.Generations)
                .HasForeignKey(e => e.VoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private static void ConfigureGenerationChunk(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenerationChunk>(entity =>
        {
            entity.ToTable("generation_chunks");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Text).IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.Provider)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.AudioUrl).HasMaxLength(500);
            entity.Property(e => e.Cost).HasPrecision(10, 6);

            entity.HasOne(e => e.Generation)
                .WithMany(g => g.Chunks)
                .HasForeignKey(e => e.GenerationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.GenerationId);
            entity.HasIndex(e => new { e.GenerationId, e.Index });
        });
    }

    private static void ConfigureFeedback(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.ToTable("feedbacks");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Comment).HasMaxLength(1000);

            entity.HasOne(e => e.Generation)
                .WithMany(g => g.Feedbacks)
                .HasForeignKey(e => e.GenerationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.GenerationId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Rating);
        });
    }
}
