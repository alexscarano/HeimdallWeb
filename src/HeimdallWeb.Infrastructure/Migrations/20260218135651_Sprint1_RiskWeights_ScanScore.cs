using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HeimdallWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Sprint1_RiskWeights_ScanScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "grade",
                table: "tb_history",
                type: "character varying(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "score",
                table: "tb_history",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_risk_weights",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    weight = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 1.0m),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_risk_weights", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_tb_risk_weights_category",
                table: "tb_risk_weights",
                column: "category",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_risk_weights");

            migrationBuilder.DropColumn(
                name: "grade",
                table: "tb_history");

            migrationBuilder.DropColumn(
                name: "score",
                table: "tb_history");
        }
    }
}
