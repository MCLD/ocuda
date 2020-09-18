using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10067 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Copyright",
                table: "Podcasts",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "Podcasts",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Podcasts",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "PodcastItems",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PodcastItems",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("UPDATE [PodcastItems] SET [UpdatedAt] = ISNULL([PublishDate], GETDATE())");
            migrationBuilder.Sql("MERGE INTO [Podcasts] p USING (SELECT [PodcastId], ISNULL(MAX([PublishDate]), GETDATE()) [PublishDate] FROM [PodcastItems] GROUP BY [PodcastId]) as items ON p.[id] = items.[PodcastId] WHEN MATCHED THEN UPDATE SET [UpdatedAt] = items.[PublishDate];");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Copyright",
                table: "Podcasts");

            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "Podcasts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Podcasts");

            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "PodcastItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PodcastItems");
        }
    }
}
