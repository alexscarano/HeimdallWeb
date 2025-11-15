using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    /// <inheritdoc />
    public partial class SetNullLogFk_OnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_log_tb_history_history_id",
                table: "tb_log");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_log_tb_user_user_id",
                table: "tb_log");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_log_tb_history_history_id",
                table: "tb_log",
                column: "history_id",
                principalTable: "tb_history",
                principalColumn: "history_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_log_tb_user_user_id",
                table: "tb_log",
                column: "user_id",
                principalTable: "tb_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_log_tb_history_history_id",
                table: "tb_log");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_log_tb_user_user_id",
                table: "tb_log");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_log_tb_history_history_id",
                table: "tb_log",
                column: "history_id",
                principalTable: "tb_history",
                principalColumn: "history_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_log_tb_user_user_id",
                table: "tb_log",
                column: "user_id",
                principalTable: "tb_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
