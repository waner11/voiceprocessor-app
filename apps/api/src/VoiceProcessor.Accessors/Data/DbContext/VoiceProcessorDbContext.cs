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
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
    public DbSet<PaymentHistory> PaymentHistories => Set<PaymentHistory>();
    public DbSet<CreditDeduction> CreditDeductions => Set<CreditDeduction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureVoice(modelBuilder);
        ConfigureGeneration(modelBuilder);
        ConfigureGenerationChunk(modelBuilder);
        ConfigureFeedback(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
        ConfigureApiKey(modelBuilder);
        ConfigureExternalLogin(modelBuilder);
        ConfigurePaymentHistory(modelBuilder);
        ConfigureCreditDeduction(modelBuilder);
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

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256);

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
            entity.HasIndex(e => new { e.GenerationId, e.UserId }).IsUnique();
        });
    }

    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.DeviceInfo)
                .HasMaxLength(256);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);

            entity.Property(e => e.ReplacedByToken)
                .HasMaxLength(256);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.RevokedAt });
        });
    }

    private static void ConfigureApiKey(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.ToTable("api_keys");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.KeyHash)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.KeyPrefix)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ApiKeys)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.KeyPrefix).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.IsActive });
        });
    }

    private static void ConfigureExternalLogin(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExternalLogin>(entity =>
        {
            entity.ToTable("external_logins");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Provider)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ProviderUserId)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.ProviderEmail)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ProviderName)
                .HasMaxLength(100);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ExternalLogins)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.Provider }).IsUnique();
            entity.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();
        });
    }

    private static void ConfigurePaymentHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentHistory>(entity =>
        {
            entity.ToTable("payment_histories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.StripeSessionId)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.StripePaymentIntentId)
                .HasMaxLength(255);

            entity.Property(e => e.Amount)
                .HasPrecision(10, 2);

            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(e => e.PackId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PackName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(e => e.User)
                .WithMany(u => u.PaymentHistories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.StripeSessionId).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private static void ConfigureCreditDeduction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CreditDeduction>(entity =>
        {
            entity.ToTable("credit_deductions");
            entity.HasKey(e => e.Id);

            // No FK constraints to users/generations: billing audit records must
            // survive entity deletion for reconciliation. Referential integrity
            // is enforced at the application level (only GenerationProcessor
            // writes to this table via TryDeductCreditsAsync).
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });
    }
}
