using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIASummaryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "issue",
                table: "tb_ia_summary");

            migrationBuilder.RenameColumn(
                name: "recommendation",
                table: "tb_ia_summary",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "tb_ia_summary",
                newName: "main_category");

            migrationBuilder.AddColumn<string>(
                name: "overall_risk",
                table: "tb_ia_summary",
                type: "ENUM('Baixo','Medio','Alto','Critico')",
                maxLength: 10,
                nullable: true,
                defaultValue: "Baixo")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "summary_text",
                table: "tb_ia_summary",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "overall_risk",
                table: "tb_ia_summary");

            migrationBuilder.DropColumn(
                name: "summary_text",
                table: "tb_ia_summary");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "tb_ia_summary",
                newName: "recommendation");

            migrationBuilder.RenameColumn(
                name: "main_category",
                table: "tb_ia_summary",
                newName: "category");

            migrationBuilder.AddColumn<string>(
                name: "issue",
                table: "tb_ia_summary",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
