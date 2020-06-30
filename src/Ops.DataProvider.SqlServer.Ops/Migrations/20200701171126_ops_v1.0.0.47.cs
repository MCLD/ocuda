using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    public partial class ops_v10047 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ToEmailAddress",
                table: "EmailRecords",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToName",
                table: "EmailRecords",
                nullable: false,
                defaultValue: "");

            migrationBuilder
                .Sql("UPDATE [Users] SET [Name] = 'System' WHERE [IsSysadmin] = 1 AND [Name] IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToEmailAddress",
                table: "EmailRecords");

            migrationBuilder.DropColumn(
                name: "ToName",
                table: "EmailRecords");
        }
    }
}
