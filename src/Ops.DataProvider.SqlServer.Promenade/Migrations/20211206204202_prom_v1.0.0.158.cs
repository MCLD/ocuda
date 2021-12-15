using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v100158 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InventoryStatus",
                table: "ProductLocationInventories",
                newName: "ManyThreshhold");

            migrationBuilder.AddColumn<int>(
                name: "ItemCount",
                table: "ProductLocationInventories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThreshholdUpdatedAt",
                table: "ProductLocationInventories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThreshholdUpdatedBy",
                table: "ProductLocationInventories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LocationProductMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationProductMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationProductMaps_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationProductMaps_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationProductMaps_LocationId",
                table: "LocationProductMaps",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationProductMaps_ProductId",
                table: "LocationProductMaps",
                column: "ProductId");

            // InventoryStatus -> ManyThreshhold
            migrationBuilder.Sql("UPDATE [ProductLocationInventories] SET [ManyThreshhold] = 0;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationProductMaps");

            migrationBuilder.DropColumn(
                name: "ItemCount",
                table: "ProductLocationInventories");

            migrationBuilder.DropColumn(
                name: "ThreshholdUpdatedAt",
                table: "ProductLocationInventories");

            migrationBuilder.DropColumn(
                name: "ThreshholdUpdatedBy",
                table: "ProductLocationInventories");

            migrationBuilder.RenameColumn(
                name: "ManyThreshhold",
                table: "ProductLocationInventories",
                newName: "InventoryStatus");

            // ManyThreshhold -> InventoryStatus
            migrationBuilder.Sql("UPDATE [ProductLocationInventories] SET [InventoryStatus] = 0;");
        }
    }
}
