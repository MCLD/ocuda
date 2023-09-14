using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    /// <inheritdoc />
    public partial class prom_v100246 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VolunteerForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeaderSegmentId = table.Column<int>(type: "int", nullable: true),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false),
                    NotifyStaffEmailSetupId = table.Column<int>(type: "int", nullable: true),
                    ThanksPageHeaderId = table.Column<int>(type: "int", nullable: true),
                    VolunteerFormType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerForms_PageHeaders_ThanksPageHeaderId",
                        column: x => x.ThanksPageHeaderId,
                        principalTable: "PageHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationForms",
                columns: table => new
                {
                    FormId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationForms", x => new { x.LocationId, x.FormId });
                    table.ForeignKey(
                        name: "FK_LocationForms_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationForms_VolunteerForms_FormId",
                        column: x => x.FormId,
                        principalTable: "VolunteerForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerFormSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Availability = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Experience = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GuardianEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GuardianName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GuardianPhone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Regularity = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StaffNotifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VolunteerFormId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerFormSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerFormSubmissions_VolunteerForms_VolunteerFormId",
                        column: x => x.VolunteerFormId,
                        principalTable: "VolunteerForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationForms_FormId",
                table: "LocationForms",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerForms_ThanksPageHeaderId",
                table: "VolunteerForms",
                column: "ThanksPageHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerFormSubmissions_VolunteerFormId",
                table: "VolunteerFormSubmissions",
                column: "VolunteerFormId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationForms");

            migrationBuilder.DropTable(
                name: "VolunteerFormSubmissions");

            migrationBuilder.DropTable(
                name: "VolunteerForms");
        }
    }
}
