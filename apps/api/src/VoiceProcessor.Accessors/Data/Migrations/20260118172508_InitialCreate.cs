using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoiceProcessor.Accessors.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Tier = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreditsRemaining = table.Column<int>(type: "integer", nullable: false),
                    CreditsUsedThisMonth = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "voices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Provider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProviderVoiceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Accent = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AgeGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UseCase = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PreviewUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CostPerThousandChars = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "generations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    InputText = table.Column<string>(type: "text", nullable: false),
                    CharacterCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RoutingPreference = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SelectedProvider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AudioUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AudioFormat = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AudioDurationMs = table.Column<int>(type: "integer", nullable: true),
                    AudioSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    ActualCost = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    ChunkCount = table.Column<int>(type: "integer", nullable: false),
                    ChunksCompleted = table.Column<int>(type: "integer", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_generations_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_generations_voices_VoiceId",
                        column: x => x.VoiceId,
                        principalTable: "voices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GenerationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WasDownloaded = table.Column<bool>(type: "boolean", nullable: true),
                    PlaybackCount = table.Column<int>(type: "integer", nullable: true),
                    PlaybackDurationMs = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_feedbacks_generations_GenerationId",
                        column: x => x.GenerationId,
                        principalTable: "generations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_feedbacks_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "generation_chunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GenerationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CharacterCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Provider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AudioUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AudioDurationMs = table.Column<int>(type: "integer", nullable: true),
                    AudioSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generation_chunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_generation_chunks_generations_GenerationId",
                        column: x => x.GenerationId,
                        principalTable: "generations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feedbacks_GenerationId",
                table: "feedbacks",
                column: "GenerationId");

            migrationBuilder.CreateIndex(
                name: "IX_feedbacks_Rating",
                table: "feedbacks",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_feedbacks_UserId",
                table: "feedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_generation_chunks_GenerationId",
                table: "generation_chunks",
                column: "GenerationId");

            migrationBuilder.CreateIndex(
                name: "IX_generation_chunks_GenerationId_Index",
                table: "generation_chunks",
                columns: new[] { "GenerationId", "Index" });

            migrationBuilder.CreateIndex(
                name: "IX_generations_CreatedAt",
                table: "generations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_generations_Status",
                table: "generations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_generations_UserId",
                table: "generations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_generations_VoiceId",
                table: "generations",
                column: "VoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_users_CreatedAt",
                table: "users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voices_IsActive",
                table: "voices",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_voices_Provider",
                table: "voices",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_voices_Provider_ProviderVoiceId",
                table: "voices",
                columns: new[] { "Provider", "ProviderVoiceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feedbacks");

            migrationBuilder.DropTable(
                name: "generation_chunks");

            migrationBuilder.DropTable(
                name: "generations");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "voices");
        }
    }
}
