using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10026 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            @"
                UPDATE [ScheduleRequest]
                SET [Notes] = 'No notes provided.'
                WHERE [Notes] IS NULL;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ScheduleRequest",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ScheduleRequest",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255);
        }
    }
}
