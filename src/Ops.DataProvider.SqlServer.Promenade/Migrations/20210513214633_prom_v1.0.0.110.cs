using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v100110 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PageFeatureId",
                table: "PageItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WebslideId",
                table: "PageItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LayoutFeatureTemplateId",
                table: "PageHeaders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LayoutWebslideTemplateId",
                table: "PageHeaders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImageFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageFeatureTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    ItemsToDisplay = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageFeatureTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageFeatureItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageFeatureId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageFeatureItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageFeatureItems_ImageFeatures_ImageFeatureId",
                        column: x => x.ImageFeatureId,
                        principalTable: "ImageFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImageFeatureItemTexts",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    ImageFeatureItemId = table.Column<int>(type: "int", nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageFeatureItemTexts", x => new { x.LanguageId, x.ImageFeatureItemId });
                    table.ForeignKey(
                        name: "FK_ImageFeatureItemTexts_ImageFeatureItems_ImageFeatureItemId",
                        column: x => x.ImageFeatureItemId,
                        principalTable: "ImageFeatureItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImageFeatureItemTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageItems_PageFeatureId",
                table: "PageItems",
                column: "PageFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_PageItems_WebslideId",
                table: "PageItems",
                column: "WebslideId");

            migrationBuilder.CreateIndex(
                name: "IX_PageHeaders_LayoutFeatureTemplateId",
                table: "PageHeaders",
                column: "LayoutFeatureTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PageHeaders_LayoutWebslideTemplateId",
                table: "PageHeaders",
                column: "LayoutWebslideTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageFeatureItems_ImageFeatureId",
                table: "ImageFeatureItems",
                column: "ImageFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageFeatureItemTexts_ImageFeatureItemId",
                table: "ImageFeatureItemTexts",
                column: "ImageFeatureItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageHeaders_ImageFeatureTemplates_LayoutFeatureTemplateId",
                table: "PageHeaders",
                column: "LayoutFeatureTemplateId",
                principalTable: "ImageFeatureTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageHeaders_ImageFeatureTemplates_LayoutWebslideTemplateId",
                table: "PageHeaders",
                column: "LayoutWebslideTemplateId",
                principalTable: "ImageFeatureTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageItems_ImageFeatures_PageFeatureId",
                table: "PageItems",
                column: "PageFeatureId",
                principalTable: "ImageFeatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageItems_ImageFeatures_WebslideId",
                table: "PageItems",
                column: "WebslideId",
                principalTable: "ImageFeatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageHeaders_ImageFeatureTemplates_LayoutFeatureTemplateId",
                table: "PageHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_PageHeaders_ImageFeatureTemplates_LayoutWebslideTemplateId",
                table: "PageHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_PageItems_ImageFeatures_PageFeatureId",
                table: "PageItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PageItems_ImageFeatures_WebslideId",
                table: "PageItems");

            migrationBuilder.DropTable(
                name: "ImageFeatureItemTexts");

            migrationBuilder.DropTable(
                name: "ImageFeatureTemplates");

            migrationBuilder.DropTable(
                name: "ImageFeatureItems");

            migrationBuilder.DropTable(
                name: "ImageFeatures");

            migrationBuilder.DropIndex(
                name: "IX_PageItems_PageFeatureId",
                table: "PageItems");

            migrationBuilder.DropIndex(
                name: "IX_PageItems_WebslideId",
                table: "PageItems");

            migrationBuilder.DropIndex(
                name: "IX_PageHeaders_LayoutFeatureTemplateId",
                table: "PageHeaders");

            migrationBuilder.DropIndex(
                name: "IX_PageHeaders_LayoutWebslideTemplateId",
                table: "PageHeaders");

            migrationBuilder.DropColumn(
                name: "PageFeatureId",
                table: "PageItems");

            migrationBuilder.DropColumn(
                name: "WebslideId",
                table: "PageItems");

            migrationBuilder.DropColumn(
                name: "LayoutFeatureTemplateId",
                table: "PageHeaders");

            migrationBuilder.DropColumn(
                name: "LayoutWebslideTemplateId",
                table: "PageHeaders");
        }
    }
}
