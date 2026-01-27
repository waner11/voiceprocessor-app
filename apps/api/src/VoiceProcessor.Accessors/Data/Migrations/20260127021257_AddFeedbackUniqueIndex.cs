using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoiceProcessor.Accessors.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_GenerationId_UserId",
                table: "feedbacks",
                columns: new[] { "GenerationId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_GenerationId_UserId",
                table: "feedbacks");
        }
    }
}
