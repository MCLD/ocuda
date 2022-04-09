using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    public partial class ops_v100174 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssociatedLocation",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Unit",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Unit",
                table: "RosterDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistoricalIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionTaken = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DateReported = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateReportedString = table.Column<string>(type: "nvarchar(28)", maxLength: 28, nullable: false),
                    DescribedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "varchar(5000)", maxLength: 5000, nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncidentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncidentAtString = table.Column<string>(type: "nvarchar(28)", maxLength: 28, nullable: false),
                    IncidentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    PeopleInvolved = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReportedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReportedByTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Witnesses = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricalIncidents_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricalIncidents_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentRelationships",
                columns: table => new
                {
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    RelatedIncidentId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentRelationships", x => new { x.IncidentId, x.RelatedIncidentId });
                    table.ForeignKey(
                        name: "FK_IncidentRelationships_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentTypes_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentTypes_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitLocationMaps",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitLocationMaps", x => x.UnitId);
                    table.ForeignKey(
                        name: "FK_UnitLocationMaps_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(2500)", maxLength: 2500, nullable: false),
                    IncidentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncidentTypeId = table.Column<int>(type: "int", nullable: false),
                    InjuriesDamages = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    LawEnforcementContacted = table.Column<bool>(type: "bit", nullable: false),
                    LocationDescription = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ReportedByName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_IncidentTypes_IncidentTypeId",
                        column: x => x.IncidentTypeId,
                        principalTable: "IncidentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentFollowups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentFollowups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentFollowups_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentFollowups_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentFollowups_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barcode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    IncidentParticipantType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentParticipants_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentParticipants_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentParticipants_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentStaffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    IncidentParticipantType = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentStaffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentStaffs_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentStaffs_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentStaffs_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentStaffs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalIncidents_CreatedBy",
                table: "HistoricalIncidents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalIncidents_UpdatedBy",
                table: "HistoricalIncidents",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentFollowups_CreatedBy",
                table: "IncidentFollowups",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentFollowups_IncidentId",
                table: "IncidentFollowups",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentFollowups_UpdatedBy",
                table: "IncidentFollowups",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentParticipants_CreatedBy",
                table: "IncidentParticipants",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentParticipants_IncidentId",
                table: "IncidentParticipants",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentParticipants_UpdatedBy",
                table: "IncidentParticipants",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentRelationships_CreatedBy",
                table: "IncidentRelationships",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_CreatedBy",
                table: "Incidents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_IncidentTypeId",
                table: "Incidents",
                column: "IncidentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_UpdatedBy",
                table: "Incidents",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentStaffs_CreatedBy",
                table: "IncidentStaffs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentStaffs_IncidentId",
                table: "IncidentStaffs",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentStaffs_UpdatedBy",
                table: "IncidentStaffs",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentStaffs_UserId",
                table: "IncidentStaffs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentTypes_CreatedBy",
                table: "IncidentTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentTypes_UpdatedBy",
                table: "IncidentTypes",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UnitLocationMaps_CreatedBy",
                table: "UnitLocationMaps",
                column: "CreatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricalIncidents");

            migrationBuilder.DropTable(
                name: "IncidentFollowups");

            migrationBuilder.DropTable(
                name: "IncidentParticipants");

            migrationBuilder.DropTable(
                name: "IncidentRelationships");

            migrationBuilder.DropTable(
                name: "IncidentStaffs");

            migrationBuilder.DropTable(
                name: "UnitLocationMaps");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "IncidentTypes");

            migrationBuilder.DropColumn(
                name: "AssociatedLocation",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "RosterDetails");
        }
    }
}
