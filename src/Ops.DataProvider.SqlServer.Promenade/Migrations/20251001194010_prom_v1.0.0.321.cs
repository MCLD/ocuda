using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    /// <inheritdoc />
    public partial class prom_v100321 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageAltTextSegmentId",
                table: "Locations",
                type: "int",
                maxLength: 130,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MapAltTextSegmentId",
                table: "Locations",
                type: "int",
                maxLength: 130,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageAltTextSegmentId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "MapAltTextSegmentId",
                table: "Locations");
        }
    }
}
