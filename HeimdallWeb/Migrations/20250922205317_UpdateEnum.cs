using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tb_history",
                keyColumn: "raw_json_result",
                keyValue: null,
                column: "raw_json_result",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "raw_json_result",
                table: "tb_history",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "severity",
                table: "tb_finding",
                type: "ENUM('Baixo','Medio','Alto','Critico')",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "ENUM('Low','Medium','High','Critical')",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "raw_json_result",
                table: "tb_history",
                type: "json",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "json")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "severity",
                table: "tb_finding",
                type: "ENUM('Low','Medium','High','Critical')",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "ENUM('Baixo','Medio','Alto','Critico')",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
