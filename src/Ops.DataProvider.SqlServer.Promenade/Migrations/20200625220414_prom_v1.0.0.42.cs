using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10042 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelatedEmailSetupId",
                table: "ScheduleRequestSubject",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NotificationSentAt",
                table: "ScheduleRequest",
                nullable: true);

            // sentinel value so old requests aren't sent emails
            migrationBuilder
                .Sql(@"UPDATE [ScheduleRequest] SET [NotificationSentAt] = '1999-01-01'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedEmailSetupId",
                table: "ScheduleRequestSubject");

            migrationBuilder.DropColumn(
                name: "NotificationSentAt",
                table: "ScheduleRequest");
        }
    }
}
