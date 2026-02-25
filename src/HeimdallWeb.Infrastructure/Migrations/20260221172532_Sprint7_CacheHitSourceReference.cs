using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Sprint7_CacheHitSourceReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "source_history_id",
                table: "tb_history",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_history_source_history_id",
                table: "tb_history",
                column: "source_history_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_history_tb_history_source_history_id",
                table: "tb_history",
                column: "source_history_id",
                principalTable: "tb_history",
                principalColumn: "history_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_history_tb_history_source_history_id",
                table: "tb_history");

            migrationBuilder.DropIndex(
                name: "ix_tb_history_source_history_id",
                table: "tb_history");

            migrationBuilder.DropColumn(
                name: "source_history_id",
                table: "tb_history");
        }
    }
}
