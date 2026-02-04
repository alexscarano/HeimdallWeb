using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    /// <inheritdoc />
    public partial class UniqueIndexEmailLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tb_user_email",
                table: "tb_user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_user_username",
                table: "tb_user",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tb_user_email",
                table: "tb_user");

            migrationBuilder.DropIndex(
                name: "IX_tb_user_username",
                table: "tb_user");
        }
    }
}
