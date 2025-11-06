using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    /// <inheritdoc />
    public partial class adicionandoColunasHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "duration",
                table: "tb_history",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "has_completed",
                table: "tb_history",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "duration",
                table: "tb_history");

            migrationBuilder.DropColumn(
                name: "has_completed",
                table: "tb_history");
        }
    }
}
