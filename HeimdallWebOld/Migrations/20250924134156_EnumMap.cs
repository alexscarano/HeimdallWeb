using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    /// <inheritdoc />
    public partial class EnumMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<sbyte>(
                name: "severity",
                table: "tb_finding",
                type: "TINYINT",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "ENUM('Baixo','Medio','Alto','Critico')",
                oldMaxLength: 10)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "severity",
                table: "tb_finding",
                type: "ENUM('Baixo','Medio','Alto','Critico')",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(sbyte),
                oldType: "TINYINT",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
