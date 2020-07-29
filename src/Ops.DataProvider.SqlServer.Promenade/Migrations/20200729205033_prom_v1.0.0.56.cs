using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10056 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CancellationEmailSetupId",
                table: "ScheduleRequestSubject",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationSentAt",
                table: "ScheduleRequest",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "ScheduleRequest",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationEmailSetupId",
                table: "ScheduleRequestSubject");

            migrationBuilder.DropColumn(
                name: "CancellationSentAt",
                table: "ScheduleRequest");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "ScheduleRequest");
        }
    }
}
