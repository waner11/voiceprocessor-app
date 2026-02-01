using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoiceProcessor.Accessors.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditDeductions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credit_deductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    GenerationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Credits = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_deductions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_credit_deductions_IdempotencyKey",
                table: "credit_deductions",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_credit_deductions_UserId_CreatedAt",
                table: "credit_deductions",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credit_deductions");
        }
    }
}
