using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    /// <inheritdoc />
    public partial class prom_v100268 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SegmentId",
                table: "LocationFeatures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NameSegmentId",
                table: "Features",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TextSegmentId",
                table: "Features",
                type: "int",
                nullable: true);

            string scriptPath = $"SqlScripts/{nameof(prom_v100268)}.sql";

            if (File.Exists(scriptPath))
            {
                migrationBuilder.Sql(File.ReadAllText(scriptPath));
            }

            migrationBuilder.DropColumn(
                name: "Text",
                table: "LocationFeatures");

            migrationBuilder.DropColumn(
                name: "BodyText",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "IconText",
                table: "Features");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SegmentId",
                table: "LocationFeatures");

            migrationBuilder.DropColumn(
                name: "NameSegmentId",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "TextSegmentId",
                table: "Features");

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "LocationFeatures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyText",
                table: "Features",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconText",
                table: "Features",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);
        }
    }
}
