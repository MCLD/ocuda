using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10063 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PodcastDirectories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastDirectories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Podcasts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(maxLength: 255, nullable: false),
                    Stub = table.Column<string>(maxLength: 50, nullable: false),
                    Author = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(maxLength: 1000, nullable: false),
                    Category = table.Column<string>(maxLength: 1000, nullable: false),
                    ImageUrl = table.Column<string>(maxLength: 255, nullable: false),
                    Language = table.Column<string>(maxLength: 8, nullable: false),
                    IsExplicit = table.Column<bool>(nullable: false),
                    IsSerial = table.Column<bool>(nullable: false),
                    OwnerName = table.Column<string>(maxLength: 255, nullable: false),
                    OwnerEmail = table.Column<string>(maxLength: 255, nullable: false),
                    IsBlocked = table.Column<bool>(nullable: false),
                    IsCompleted = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Podcasts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PodcastDirectoryInfos",
                columns: table => new
                {
                    PodcastId = table.Column<int>(nullable: false),
                    PodcastDirectoryId = table.Column<int>(nullable: false),
                    Url = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastDirectoryInfos", x => new { x.PodcastId, x.PodcastDirectoryId });
                    table.ForeignKey(
                        name: "FK_PodcastDirectoryInfos_PodcastDirectories_PodcastDirectoryId",
                        column: x => x.PodcastDirectoryId,
                        principalTable: "PodcastDirectories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PodcastDirectoryInfos_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PodcastItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PodcastId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 255, nullable: false),
                    Stub = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: false),
                    IsExplicit = table.Column<bool>(nullable: false),
                    ImageUrl = table.Column<string>(maxLength: 255, nullable: true),
                    Keywords = table.Column<string>(maxLength: 255, nullable: true),
                    Season = table.Column<int>(nullable: true),
                    Episode = table.Column<int>(nullable: true),
                    MediaUrl = table.Column<string>(maxLength: 255, nullable: false),
                    MediaType = table.Column<string>(maxLength: 32, nullable: false),
                    MediaSize = table.Column<int>(nullable: false),
                    Duration = table.Column<int>(nullable: false),
                    Guid = table.Column<string>(maxLength: 255, nullable: false),
                    GuidPermaLink = table.Column<bool>(nullable: false),
                    PublishDate = table.Column<DateTime>(nullable: true),
                    IsBlocked = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodcastItems_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PodcastDirectoryInfos_PodcastDirectoryId",
                table: "PodcastDirectoryInfos",
                column: "PodcastDirectoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastItems_PodcastId",
                table: "PodcastItems",
                column: "PodcastId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PodcastDirectoryInfos");

            migrationBuilder.DropTable(
                name: "PodcastItems");

            migrationBuilder.DropTable(
                name: "PodcastDirectories");

            migrationBuilder.DropTable(
                name: "Podcasts");
        }
    }
}
