using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Infrastructure.EfCore.SqlServer.Migrations
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
                name: "ShortenUrls",
                schema: "shorten",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LongUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ShortUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsExpiring = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortenUrls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShortUrlTracks",
                schema: "shorten",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShortenUrlId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AccessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortUrlTracks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShortenUrls_Code",
                schema: "shorten",
                table: "ShortenUrls",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortenUrls_Id",
                schema: "shorten",
                table: "ShortenUrls",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrlTracks_Code",
                schema: "shorten",
                table: "ShortUrlTracks",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrlTracks_Id",
                schema: "shorten",
                table: "ShortUrlTracks",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrlTracks_ShortenUrlId",
                schema: "shorten",
                table: "ShortUrlTracks",
                column: "ShortenUrlId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShortenUrls",
                schema: "shorten");

            migrationBuilder.DropTable(
                name: "ShortUrlTracks",
                schema: "shorten");
        }
    }
}
