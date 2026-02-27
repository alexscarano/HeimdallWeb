using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFindingHistoricalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "presente_ha_scans",
                table: "tb_finding",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_historico",
                table: "tb_finding",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "presente_ha_scans",
                table: "tb_finding");

            migrationBuilder.DropColumn(
                name: "status_historico",
                table: "tb_finding");
        }
    }
}
