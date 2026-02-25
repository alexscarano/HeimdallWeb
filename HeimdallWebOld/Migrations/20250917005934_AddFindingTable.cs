using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddFindingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_finding",
                columns: table => new
                {
                    finding_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    severity = table.Column<string>(type: "ENUM('Low','Medium','High','Critical')", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    evidence = table.Column<string>(type: "text", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    recommendation = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    history_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_finding", x => x.finding_id);
                    table.ForeignKey(
                        name: "FK_tb_finding_tb_history_history_id",
                        column: x => x.history_id,
                        principalTable: "tb_history",
                        principalColumn: "history_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_tb_finding_history_id",
                table: "tb_finding",
                column: "history_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_finding");
        }
    }
}
