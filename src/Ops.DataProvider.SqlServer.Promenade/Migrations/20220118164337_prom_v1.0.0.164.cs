using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v100164 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CacheInventoryMinutes",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Default CacheInventoryMinutes
            migrationBuilder.Sql("UPDATE [Products] SET [CacheInventoryMinutes] = 5;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CacheInventoryMinutes",
                table: "Products");
        }
    }
}
