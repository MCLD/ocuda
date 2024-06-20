using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    /// <inheritdoc />
    public partial class ops_dev : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LibraryPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DescriptionSegmentId = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    HistoricId = table.Column<int>(type: "int", nullable: true),
                    ImportedBy = table.Column<int>(type: "int", nullable: true),
                    Instructor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAllDay = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsGuardianInfoRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    IsRegistrationRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsStaffRegistrationRequired = table.Column<bool>(type: "bit", nullable: false),
                    LocationDescriptionSegmentId = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    MaxAgeMonths = table.Column<int>(type: "int", nullable: true),
                    MaxPeople = table.Column<int>(type: "int", nullable: false),
                    MaxWaitList = table.Column<int>(type: "int", nullable: true),
                    MinAgeMonths = table.Column<int>(type: "int", nullable: true),
                    OwnedByUserId = table.Column<int>(type: "int", nullable: false),
                    ScheduledEventId = table.Column<int>(type: "int", nullable: true),
                    SignUpEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SignUpStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sponsor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SponsorLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TitleSegmentId = table.Column<int>(type: "int", nullable: false),
                    TotalAttendance = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryPrograms_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LibraryPrograms_Users_OwnedByUserId",
                        column: x => x.OwnedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LibraryPrograms_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LibraryProgramRelationships",
                columns: table => new
                {
                    LibraryProgramId = table.Column<int>(type: "int", nullable: false),
                    RelatedLibraryProgramId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryProgramRelationships", x => new { x.LibraryProgramId, x.RelatedLibraryProgramId });
                    table.ForeignKey(
                        name: "FK_LibraryProgramRelationships_LibraryPrograms_LibraryProgramId",
                        column: x => x.LibraryProgramId,
                        principalTable: "LibraryPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LibraryProgramRelationships_LibraryPrograms_RelatedLibraryProgramId",
                        column: x => x.RelatedLibraryProgramId,
                        principalTable: "LibraryPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LibraryProgramRelationships_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryProgramRelationships_CreatedBy",
                table: "LibraryProgramRelationships",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryProgramRelationships_RelatedLibraryProgramId",
                table: "LibraryProgramRelationships",
                column: "RelatedLibraryProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryPrograms_CreatedBy",
                table: "LibraryPrograms",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryPrograms_OwnedByUserId",
                table: "LibraryPrograms",
                column: "OwnedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryPrograms_UpdatedBy",
                table: "LibraryPrograms",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryProgramRelationships");

            migrationBuilder.DropTable(
                name: "LibraryPrograms");
        }
    }
}
