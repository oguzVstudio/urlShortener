using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Infrastructure.EfCore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shorten");

            migrationBuilder.CreateTable(
                name: "short_link_access_logs",
                schema: "shorten",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    short_link_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    accessed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_link_access_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "short_links",
                schema: "shorten",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    long_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    short_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_expiring = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_links", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_short_link_access_logs_code",
                schema: "shorten",
                table: "short_link_access_logs",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_short_link_access_logs_id",
                schema: "shorten",
                table: "short_link_access_logs",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_short_link_access_logs_short_link_id",
                schema: "shorten",
                table: "short_link_access_logs",
                column: "short_link_id");

            migrationBuilder.CreateIndex(
                name: "ix_short_links_code",
                schema: "shorten",
                table: "short_links",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_short_links_id",
                schema: "shorten",
                table: "short_links",
                column: "id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "short_link_access_logs",
                schema: "shorten");

            migrationBuilder.DropTable(
                name: "short_links",
                schema: "shorten");
        }
    }
}
