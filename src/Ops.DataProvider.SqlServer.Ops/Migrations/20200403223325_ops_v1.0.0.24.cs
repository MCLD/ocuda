using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    public partial class ops_v10024 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleClaims",
                columns: table => new
                {
                    ScheduleRequestId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleClaims", x => x.ScheduleRequestId);
                    table.ForeignKey(
                        name: "FK_ScheduleClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleLogCallDispositions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Disposition = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleLogCallDispositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleLogCallDispositionId = table.Column<int>(nullable: true),
                    DurationMinutes = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Notes = table.Column<string>(maxLength: 1000, nullable: true),
                    ScheduleRequestId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleLogs_ScheduleLogCallDispositions_ScheduleLogCallDispositionId",
                        column: x => x.ScheduleLogCallDispositionId,
                        principalTable: "ScheduleLogCallDispositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleClaims_UserId",
                table: "ScheduleClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleLogs_ScheduleLogCallDispositionId",
                table: "ScheduleLogs",
                column: "ScheduleLogCallDispositionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleClaims");

            migrationBuilder.DropTable(
                name: "ScheduleLogs");

            migrationBuilder.DropTable(
                name: "ScheduleLogCallDispositions");
        }
    }
}
