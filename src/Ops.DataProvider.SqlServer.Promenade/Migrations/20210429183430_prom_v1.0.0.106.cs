using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v100106 : Migration
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
                name: "PageFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageFeatureTemplates",
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
                    table.PrimaryKey("PK_PageFeatureTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Webslides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webslides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebslideTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebslideTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageFeatureItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageFeatureId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageFeatureItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageFeatureItems_PageFeatures_PageFeatureId",
                        column: x => x.PageFeatureId,
                        principalTable: "PageFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WebslideItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebslideId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebslideItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebslideItems_Webslides_WebslideId",
                        column: x => x.WebslideId,
                        principalTable: "Webslides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PageFeatureItemTexts",
                columns: table => new
                {
                    PageFeatureItemId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageFeatureItemTexts", x => new { x.PageFeatureItemId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_PageFeatureItemTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageFeatureItemTexts_PageFeatureItems_PageFeatureItemId",
                        column: x => x.PageFeatureItemId,
                        principalTable: "PageFeatureItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WebslideItemTexts",
                columns: table => new
                {
                    WebslideItemId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebslideItemTexts", x => new { x.LanguageId, x.WebslideItemId });
                    table.ForeignKey(
                        name: "FK_WebslideItemTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WebslideItemTexts_WebslideItems_WebslideItemId",
                        column: x => x.WebslideItemId,
                        principalTable: "WebslideItems",
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
                name: "IX_PageFeatureItems_PageFeatureId",
                table: "PageFeatureItems",
                column: "PageFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_PageFeatureItemTexts_LanguageId",
                table: "PageFeatureItemTexts",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_WebslideItems_WebslideId",
                table: "WebslideItems",
                column: "WebslideId");

            migrationBuilder.CreateIndex(
                name: "IX_WebslideItemTexts_WebslideItemId",
                table: "WebslideItemTexts",
                column: "WebslideItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageHeaders_PageFeatureTemplates_LayoutFeatureTemplateId",
                table: "PageHeaders",
                column: "LayoutFeatureTemplateId",
                principalTable: "PageFeatureTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageHeaders_WebslideTemplates_LayoutWebslideTemplateId",
                table: "PageHeaders",
                column: "LayoutWebslideTemplateId",
                principalTable: "WebslideTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageItems_PageFeatures_PageFeatureId",
                table: "PageItems",
                column: "PageFeatureId",
                principalTable: "PageFeatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageItems_Webslides_WebslideId",
                table: "PageItems",
                column: "WebslideId",
                principalTable: "Webslides",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageHeaders_PageFeatureTemplates_LayoutFeatureTemplateId",
                table: "PageHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_PageHeaders_WebslideTemplates_LayoutWebslideTemplateId",
                table: "PageHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_PageItems_PageFeatures_PageFeatureId",
                table: "PageItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PageItems_Webslides_WebslideId",
                table: "PageItems");

            migrationBuilder.DropTable(
                name: "PageFeatureItemTexts");

            migrationBuilder.DropTable(
                name: "PageFeatureTemplates");

            migrationBuilder.DropTable(
                name: "WebslideItemTexts");

            migrationBuilder.DropTable(
                name: "WebslideTemplates");

            migrationBuilder.DropTable(
                name: "PageFeatureItems");

            migrationBuilder.DropTable(
                name: "WebslideItems");

            migrationBuilder.DropTable(
                name: "PageFeatures");

            migrationBuilder.DropTable(
                name: "Webslides");

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
