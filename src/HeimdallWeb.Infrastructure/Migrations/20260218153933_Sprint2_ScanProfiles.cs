using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HeimdallWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2_ScanProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_scan_profile",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    config_json = table.Column<string>(type: "text", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_scan_profile", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_tb_scan_profile_name",
                table: "tb_scan_profile",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_scan_profile");
        }
    }
}
