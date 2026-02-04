using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoiceProcessor.Accessors.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPresetToGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Preset",
                table: "generations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_feedbacks_GenerationId_UserId",
                table: "feedbacks",
                columns: new[] { "GenerationId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_feedbacks_GenerationId_UserId",
                table: "feedbacks");

            migrationBuilder.DropColumn(
                name: "Preset",
                table: "generations");
        }
    }
}
