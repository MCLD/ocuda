using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10032 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequireComments",
                table: "ScheduleRequestSubject",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireEmail",
                table: "ScheduleRequestSubject",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SegmentId",
                table: "ScheduleRequestSubject",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ScheduleRequest",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.Sql(@"UPDATE [ScheduleRequestSubject] SET [RequireComments] = 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequireComments",
                table: "ScheduleRequestSubject");

            migrationBuilder.DropColumn(
                name: "RequireEmail",
                table: "ScheduleRequestSubject");

            migrationBuilder.DropColumn(
                name: "SegmentId",
                table: "ScheduleRequestSubject");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ScheduleRequest",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
