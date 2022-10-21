using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    public partial class ops_v100205 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitLocationMaps");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AsOf",
                table: "RosterDetails");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "RosterDetails");

            migrationBuilder.DropColumn(
                name: "IsVacant",
                table: "RosterDetails");

            migrationBuilder.DropColumn(
                name: "RehireDate",
                table: "RosterDetails");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "RosterDetails");

            migrationBuilder.AddColumn<DateTime>(
                name: "VacateDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "RosterHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImported",
                table: "RosterHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "PositionNum",
                table: "RosterDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "RosterDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "RosterDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DivisionName",
                table: "RosterDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "RosterDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LocationName",
                table: "RosterDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReportsToName",
                table: "RosterDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RosterDivisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdInRoster = table.Column<int>(type: "int", nullable: false),
                    MapToLocationId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RosterDivisions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RosterDivisions_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RosterLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdInRoster = table.Column<int>(type: "int", nullable: false),
                    MapToLocationId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RosterLocations_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RosterLocations_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RosterDivisions_CreatedBy",
                table: "RosterDivisions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RosterDivisions_UpdatedBy",
                table: "RosterDivisions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RosterLocations_CreatedBy",
                table: "RosterLocations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RosterLocations_UpdatedBy",
                table: "RosterLocations",
                column: "UpdatedBy");

            migrationBuilder.Sql("UPDATE [RosterHeaders] SET [IsImported] = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RosterDivisions");

            migrationBuilder.DropTable(
                name: "RosterLocations");

            migrationBuilder.DropColumn(
                name: "VacateDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "RosterHeaders");

            migrationBuilder.DropColumn(
                name: "IsImported",
                table: "RosterHeaders");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "RosterDetails");

            migrationBuilder.DropColumn(
                name: "DivisionName",
                table: "RosterDetails");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "RosterDetails");

            migrationBuilder.DropColumn(
                name: "LocationName",
                table: "RosterDetails");

            migrationBuilder.DropColumn(
                name: "ReportsToName",
                table: "RosterDetails");

            migrationBuilder.AddColumn<int>(
                name: "Unit",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PositionNum",
                table: "RosterDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "RosterDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AsOf",
                table: "RosterDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "RosterDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVacant",
                table: "RosterDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RehireDate",
                table: "RosterDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Unit",
                table: "RosterDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UnitLocationMaps",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_UnitLocationMaps_CreatedBy",
                table: "UnitLocationMaps",
                column: "CreatedBy");
        }
    }
}
