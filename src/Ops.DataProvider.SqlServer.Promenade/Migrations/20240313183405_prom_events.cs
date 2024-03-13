using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    /// <inheritdoc />
    public partial class prom_events : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgeGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SegmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgeGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgeGroups_Segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEventRegistrationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ScheduledEventRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEventRegistrationHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEventRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RelatedScheduledEventRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScheduledEventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEventRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescriptionSegmentId = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<int>(type: "int", nullable: true),
                    IsAllDay = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    LocationDescriptionId = table.Column<int>(type: "int", nullable: true),
                    MaxPeople = table.Column<int>(type: "int", nullable: false),
                    PublishOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledEventType = table.Column<int>(type: "int", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TitleSegmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEventAgeGroups",
                columns: table => new
                {
                    AgeGroupId = table.Column<int>(type: "int", nullable: false),
                    ScheduledEventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEventAgeGroups", x => new { x.ScheduledEventId, x.AgeGroupId });
                    table.ForeignKey(
                        name: "FK_ScheduledEventAgeGroups_AgeGroups_AgeGroupId",
                        column: x => x.AgeGroupId,
                        principalTable: "AgeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledEventAgeGroups_ScheduledEvents_ScheduledEventId",
                        column: x => x.ScheduledEventId,
                        principalTable: "ScheduledEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEventLocations",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ScheduledEventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEventLocations", x => new { x.ScheduledEventId, x.LocationId });
                    table.ForeignKey(
                        name: "FK_ScheduledEventLocations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledEventLocations_ScheduledEvents_ScheduledEventId",
                        column: x => x.ScheduledEventId,
                        principalTable: "ScheduledEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEventRelationships",
                columns: table => new
                {
                    RelatedScheduledEventId = table.Column<int>(type: "int", nullable: false),
                    ScheduledEventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEventRelationships", x => new { x.ScheduledEventId, x.RelatedScheduledEventId });
                    table.ForeignKey(
                        name: "FK_ScheduledEventRelationships_ScheduledEvents_RelatedScheduledEventId",
                        column: x => x.RelatedScheduledEventId,
                        principalTable: "ScheduledEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledEventRelationships_ScheduledEvents_ScheduledEventId",
                        column: x => x.ScheduledEventId,
                        principalTable: "ScheduledEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgeGroups_SegmentId",
                table: "AgeGroups",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEventAgeGroups_AgeGroupId",
                table: "ScheduledEventAgeGroups",
                column: "AgeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEventLocations_LocationId",
                table: "ScheduledEventLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEventRelationships_RelatedScheduledEventId",
                table: "ScheduledEventRelationships",
                column: "RelatedScheduledEventId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEvents_Slug",
                table: "ScheduledEvents",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledEventAgeGroups");

            migrationBuilder.DropTable(
                name: "ScheduledEventLocations");

            migrationBuilder.DropTable(
                name: "ScheduledEventRegistrationHistories");

            migrationBuilder.DropTable(
                name: "ScheduledEventRegistrations");

            migrationBuilder.DropTable(
                name: "ScheduledEventRelationships");

            migrationBuilder.DropTable(
                name: "AgeGroups");

            migrationBuilder.DropTable(
                name: "ScheduledEvents");
        }
    }
}
