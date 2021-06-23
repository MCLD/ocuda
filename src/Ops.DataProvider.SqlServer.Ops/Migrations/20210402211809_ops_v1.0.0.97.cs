using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    public partial class ops_v10097 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DigitalDisplayAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalDisplayAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalDisplayAssets_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalDisplayAssets_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DigitalDisplays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasicAuthentication = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastAttempt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCommunication = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastContentVerification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocationDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RemoteAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SlideCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalDisplays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalDisplays_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalDisplays_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DigitalDisplaySets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastContentUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalDisplaySets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalDisplaySets_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalDisplaySets_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DigitalDisplayItems",
                columns: table => new
                {
                    DigitalDisplayAssetId = table.Column<int>(type: "int", nullable: false),
                    DigitalDisplayId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalDisplayItems", x => new { x.DigitalDisplayAssetId, x.DigitalDisplayId });
                    table.ForeignKey(
                        name: "FK_DigitalDisplayItems_DigitalDisplayAssets_DigitalDisplayAssetId",
                        column: x => x.DigitalDisplayAssetId,
                        principalTable: "DigitalDisplayAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalDisplayItems_DigitalDisplays_DigitalDisplayId",
                        column: x => x.DigitalDisplayId,
                        principalTable: "DigitalDisplays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DigitalDisplayAssetSets",
                columns: table => new
                {
                    DigitalDisplayAssetId = table.Column<int>(type: "int", nullable: false),
                    DigitalDisplaySetId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalDisplayAssetSets", x => new { x.DigitalDisplayAssetId, x.DigitalDisplaySetId });
                    table.ForeignKey(
                        name: "FK_DigitalDisplayAssetSets_DigitalDisplayAssets_DigitalDisplayAssetId",
                        column: x => x.DigitalDisplayAssetId,
                        principalTable: "DigitalDisplayAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalDisplayAssetSets_DigitalDisplaySets_DigitalDisplaySetId",
                        column: x => x.DigitalDisplaySetId,
                        principalTable: "DigitalDisplaySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DigitalDisplayDisplaySets",
                columns: table => new
                {
                    DigitalDisplayId = table.Column<int>(type: "int", nullable: false),
                    DigitalDisplaySetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalDisplayDisplaySets", x => new { x.DigitalDisplayId, x.DigitalDisplaySetId });
                    table.ForeignKey(
                        name: "FK_DigitalDisplayDisplaySets_DigitalDisplays_DigitalDisplayId",
                        column: x => x.DigitalDisplayId,
                        principalTable: "DigitalDisplays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalDisplayDisplaySets_DigitalDisplaySets_DigitalDisplaySetId",
                        column: x => x.DigitalDisplaySetId,
                        principalTable: "DigitalDisplaySets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalDisplayAssets_CreatedBy",
                table: "DigitalDisplayAssets",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalDisplayAssets_UpdatedBy",
                table: "DigitalDisplayAssets",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalDisplayAssetSets_DigitalDisplaySetId",
                table: "DigitalDisplayAssetSets",
                column: "DigitalDisplaySetId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalDisplayDisplaySets_DigitalDisplaySetId",
                table: "DigitalDisplayDisplaySets",
                column: "DigitalDisplaySetId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalDisplayItems_DigitalDisplayId",
                table: "DigitalDisplayItems",
                column: "DigitalDisplayId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalDisplays_CreatedBy",
                table: "DigitalDisplays",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalDisplays_UpdatedBy",
                table: "DigitalDisplays",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalDisplaySets_CreatedBy",
                table: "DigitalDisplaySets",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalDisplaySets_UpdatedBy",
                table: "DigitalDisplaySets",
                column: "UpdatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DigitalDisplayAssetSets");

            migrationBuilder.DropTable(
                name: "DigitalDisplayDisplaySets");

            migrationBuilder.DropTable(
                name: "DigitalDisplayItems");

            migrationBuilder.DropTable(
                name: "DigitalDisplaySets");

            migrationBuilder.DropTable(
                name: "DigitalDisplayAssets");

            migrationBuilder.DropTable(
                name: "DigitalDisplays");
        }
    }
}
