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
                name: "short_url_tracks",
                schema: "shorten",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shorten_url_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    accessed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_url_tracks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shorten_urls",
                schema: "shorten",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    long_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    short_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_expiring = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shorten_urls", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_short_url_tracks_code",
                schema: "shorten",
                table: "short_url_tracks",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_short_url_tracks_id",
                schema: "shorten",
                table: "short_url_tracks",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_short_url_tracks_shorten_url_id",
                schema: "shorten",
                table: "short_url_tracks",
                column: "shorten_url_id");

            migrationBuilder.CreateIndex(
                name: "ix_shorten_urls_code",
                schema: "shorten",
                table: "shorten_urls",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shorten_urls_id",
                schema: "shorten",
                table: "shorten_urls",
                column: "id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "short_url_tracks",
                schema: "shorten");

            migrationBuilder.DropTable(
                name: "shorten_urls",
                schema: "shorten");
        }
    }
}
