using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HeimdallWeb.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQLMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    profile_image = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_user", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_history",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    target = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: false),
                    raw_json_result = table.Column<string>(type: "jsonb", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: false),
                    has_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_history", x => x.history_id);
                    table.ForeignKey(
                        name: "FK_tb_history_tb_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_user_usage",
                columns: table => new
                {
                    user_usage_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    request_counts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_user_usage", x => x.user_usage_id);
                    table.ForeignKey(
                        name: "FK_tb_user_usage_tb_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_finding",
                columns: table => new
                {
                    finding_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    severity = table.Column<short>(type: "smallint", nullable: false),
                    evidence = table.Column<string>(type: "text", nullable: false),
                    recommendation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    history_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_finding", x => x.finding_id);
                    table.ForeignKey(
                        name: "FK_tb_finding_tb_history_history_id",
                        column: x => x.history_id,
                        principalTable: "tb_history",
                        principalColumn: "history_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_ia_summary",
                columns: table => new
                {
                    ia_summary_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    summary_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    main_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    overall_risk = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    total_findings = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    findings_critical = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    findings_high = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    findings_medium = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    findings_low = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ia_notes = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    history_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_ia_summary", x => x.ia_summary_id);
                    table.ForeignKey(
                        name: "FK_tb_ia_summary_tb_history_history_id",
                        column: x => x.history_id,
                        principalTable: "tb_history",
                        principalColumn: "history_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_log",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    code = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Info"),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    details = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    history_id = table.Column<int>(type: "integer", nullable: true),
                    remote_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_log", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_tb_log_tb_history_history_id",
                        column: x => x.history_id,
                        principalTable: "tb_history",
                        principalColumn: "history_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_log_tb_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tb_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_technology",
                columns: table => new
                {
                    technology_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    technology_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    version = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    technology_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    technology_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    history_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tb_technology", x => x.technology_id);
                    table.ForeignKey(
                        name: "FK_tb_technology_tb_history_history_id",
                        column: x => x.history_id,
                        principalTable: "tb_history",
                        principalColumn: "history_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tb_finding_created_at",
                table: "tb_finding",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tb_finding_history_id",
                table: "tb_finding",
                column: "history_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_finding_severity",
                table: "tb_finding",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "ix_tb_history_created_date",
                table: "tb_history",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "ix_tb_history_has_completed",
                table: "tb_history",
                column: "has_completed");

            migrationBuilder.CreateIndex(
                name: "ix_tb_history_raw_json_gin",
                table: "tb_history",
                column: "raw_json_result")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_tb_history_target",
                table: "tb_history",
                column: "target");

            migrationBuilder.CreateIndex(
                name: "ix_tb_history_user_id",
                table: "tb_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_ia_summary_created_date",
                table: "tb_ia_summary",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "ix_tb_ia_summary_history_id",
                table: "tb_ia_summary",
                column: "history_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_ia_summary_main_category",
                table: "tb_ia_summary",
                column: "main_category");

            migrationBuilder.CreateIndex(
                name: "ix_tb_ia_summary_overall_risk",
                table: "tb_ia_summary",
                column: "overall_risk");

            migrationBuilder.CreateIndex(
                name: "ix_tb_log_code",
                table: "tb_log",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_tb_log_history_id",
                table: "tb_log",
                column: "history_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_log_level",
                table: "tb_log",
                column: "level");

            migrationBuilder.CreateIndex(
                name: "ix_tb_log_timestamp",
                table: "tb_log",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_tb_log_user_id",
                table: "tb_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_technology_category",
                table: "tb_technology",
                column: "technology_category");

            migrationBuilder.CreateIndex(
                name: "ix_tb_technology_history_id",
                table: "tb_technology",
                column: "history_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_technology_name",
                table: "tb_technology",
                column: "technology_name");

            migrationBuilder.CreateIndex(
                name: "ix_tb_user_created_at",
                table: "tb_user",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ux_tb_user_email",
                table: "tb_user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_tb_user_username",
                table: "tb_user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tb_user_usage_date",
                table: "tb_user_usage",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_tb_user_usage_user_id",
                table: "tb_user_usage",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tb_user_usage_user_id_date",
                table: "tb_user_usage",
                columns: new[] { "user_id", "date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_finding");

            migrationBuilder.DropTable(
                name: "tb_ia_summary");

            migrationBuilder.DropTable(
                name: "tb_log");

            migrationBuilder.DropTable(
                name: "tb_technology");

            migrationBuilder.DropTable(
                name: "tb_user_usage");

            migrationBuilder.DropTable(
                name: "tb_history");

            migrationBuilder.DropTable(
                name: "tb_user");
        }
    }
}
