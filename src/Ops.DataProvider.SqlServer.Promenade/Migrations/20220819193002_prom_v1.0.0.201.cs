using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v100201 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaximumFileSizeBytes",
                table: "ImageFeatureTemplates",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumFileSizeBytes",
                table: "ImageFeatureTemplates");
        }
    }
}
