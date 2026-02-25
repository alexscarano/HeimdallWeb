using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Sprint5_GoogleAuth_PasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "auth_provider",
                table: "tb_user",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Local");

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "tb_user",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "password_reset_expires",
                table: "tb_user",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_reset_token",
                table: "tb_user",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_user_external_id",
                table: "tb_user",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_user_password_reset_token",
                table: "tb_user",
                column: "password_reset_token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tb_user_external_id",
                table: "tb_user");

            migrationBuilder.DropIndex(
                name: "ix_tb_user_password_reset_token",
                table: "tb_user");

            migrationBuilder.DropColumn(
                name: "auth_provider",
                table: "tb_user");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "tb_user");

            migrationBuilder.DropColumn(
                name: "password_reset_expires",
                table: "tb_user");

            migrationBuilder.DropColumn(
                name: "password_reset_token",
                table: "tb_user");
        }
    }
}
