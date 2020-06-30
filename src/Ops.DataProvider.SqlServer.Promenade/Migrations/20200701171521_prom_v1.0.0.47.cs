using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10047 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FollowupEmailSetupId",
                table: "ScheduleRequestSubject",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FollowupSentAt",
                table: "ScheduleRequest",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnderway",
                table: "ScheduleRequest",
                nullable: false,
                defaultValue: false);

            migrationBuilder
                .Sql("UPDATE [ScheduleRequest] SET [Language] = 'en-US' WHERE [Language] = 'English'");
            migrationBuilder
                .Sql("UPDATE [ScheduleRequest] SET [Language] = 'es-US' WHERE [Language] = 'español'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowupEmailSetupId",
                table: "ScheduleRequestSubject");

            migrationBuilder.DropColumn(
                name: "FollowupSentAt",
                table: "ScheduleRequest");

            migrationBuilder.DropColumn(
                name: "IsUnderway",
                table: "ScheduleRequest");

            migrationBuilder
                .Sql("UPDATE [ScheduleRequest] SET [Language] = 'English' WHERE [Language] = 'en-US'");
            migrationBuilder
                .Sql("UPDATE [ScheduleRequest] SET [Language] = 'español' WHERE [Language] = 'es-US'");
        }
    }
}
