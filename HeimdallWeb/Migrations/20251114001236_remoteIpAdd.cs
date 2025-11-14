using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    /// <inheritdoc />
    public partial class remoteIpAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "remote_ip",
                table: "tb_log",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "remote_ip",
                table: "tb_log");
        }
    }
}
