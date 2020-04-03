using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10024 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleRequestSubject",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleRequestSubject", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleRequestTelephone",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Phone = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleRequestTelephone", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestedTime = table.Column<DateTime>(nullable: false),
                    ScheduleRequestSubjectId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Email = table.Column<string>(maxLength: 255, nullable: true),
                    ScheduleRequestTelephoneId = table.Column<int>(nullable: false),
                    Language = table.Column<string>(maxLength: 255, nullable: false),
                    Notes = table.Column<string>(maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    IsClaimed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleRequest_ScheduleRequestSubject_ScheduleRequestSubjectId",
                        column: x => x.ScheduleRequestSubjectId,
                        principalTable: "ScheduleRequestSubject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleRequest_ScheduleRequestTelephone_ScheduleRequestTelephoneId",
                        column: x => x.ScheduleRequestTelephoneId,
                        principalTable: "ScheduleRequestTelephone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleRequest_ScheduleRequestSubjectId",
                table: "ScheduleRequest",
                column: "ScheduleRequestSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleRequest_ScheduleRequestTelephoneId",
                table: "ScheduleRequest",
                column: "ScheduleRequestTelephoneId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleRequest");

            migrationBuilder.DropTable(
                name: "ScheduleRequestSubject");

            migrationBuilder.DropTable(
                name: "ScheduleRequestTelephone");
        }
    }
}
