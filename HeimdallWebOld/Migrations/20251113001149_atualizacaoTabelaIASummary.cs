using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimdallWeb.Migrations
{
    /// <inheritdoc />
    public partial class atualizacaoTabelaIASummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_ia_summary_tb_history_history_id",
                table: "tb_ia_summary");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_ia_summary",
                table: "tb_ia_summary");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "tb_ia_summary");

            migrationBuilder.AddColumn<int>(
                name: "findings_critical",
                table: "tb_ia_summary",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "findings_high",
                table: "tb_ia_summary",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "findings_low",
                table: "tb_ia_summary",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "findings_medium",
                table: "tb_ia_summary",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ia_notes",
                table: "tb_ia_summary",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "total_findings",
                table: "tb_ia_summary",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_tb_ia_summary",
                table: "tb_ia_summary",
                column: "ia_summary_id");

            migrationBuilder.AddForeignKey(
                name: "fk_tb_ia_summary_tb_history_history_id",
                table: "tb_ia_summary",
                column: "history_id",
                principalTable: "tb_history",
                principalColumn: "history_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tb_ia_summary_tb_history_history_id",
                table: "tb_ia_summary");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tb_ia_summary",
                table: "tb_ia_summary");

            migrationBuilder.DropColumn(
                name: "findings_critical",
                table: "tb_ia_summary");

            migrationBuilder.DropColumn(
                name: "findings_high",
                table: "tb_ia_summary");

            migrationBuilder.DropColumn(
                name: "findings_low",
                table: "tb_ia_summary");

            migrationBuilder.DropColumn(
                name: "findings_medium",
                table: "tb_ia_summary");

            migrationBuilder.DropColumn(
                name: "ia_notes",
                table: "tb_ia_summary");

            migrationBuilder.DropColumn(
                name: "total_findings",
                table: "tb_ia_summary");

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "tb_ia_summary",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_ia_summary",
                table: "tb_ia_summary",
                column: "ia_summary_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_ia_summary_tb_history_history_id",
                table: "tb_ia_summary",
                column: "history_id",
                principalTable: "tb_history",
                principalColumn: "history_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
