using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v100133 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AriaLabel",
                table: "NavigationTexts",
                newName: "Link");

            migrationBuilder.Sql("UPDATE nt SET nt.[Link] = n.[Link] FROM [Navigations] n INNER JOIN [NavigationTexts] nt ON nt.[Id] = n.[NavigationTextId]");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "Navigations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Link",
                table: "NavigationTexts",
                newName: "AriaLabel");

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Navigations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
