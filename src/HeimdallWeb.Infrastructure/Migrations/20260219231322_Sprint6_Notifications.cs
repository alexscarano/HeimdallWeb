using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HeimdallWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Sprint6_Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_notification",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_notification", x => x.notification_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tb_notification_created_at",
                table: "tb_notification",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tb_notification_user_id_is_read",
                table: "tb_notification",
                columns: new[] { "user_id", "is_read" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_notification");
        }
    }
}
