using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v100204 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BannerFeatureId",
                table: "PageItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeckId",
                table: "PageItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LayoutBannerTemplateId",
                table: "PageHeaders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeckId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CardDetails",
                columns: table => new
                {
                    CardId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Filename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Header = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardDetails", x => new { x.CardId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_CardDetails_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CardDetails_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageItems_BannerFeatureId",
                table: "PageItems",
                column: "BannerFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_PageItems_DeckId",
                table: "PageItems",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_PageHeaders_LayoutBannerTemplateId",
                table: "PageHeaders",
                column: "LayoutBannerTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CardDetails_LanguageId",
                table: "CardDetails",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_DeckId",
                table: "Cards",
                column: "DeckId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageHeaders_ImageFeatureTemplates_LayoutBannerTemplateId",
                table: "PageHeaders",
                column: "LayoutBannerTemplateId",
                principalTable: "ImageFeatureTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageItems_Decks_DeckId",
                table: "PageItems",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageItems_ImageFeatures_BannerFeatureId",
                table: "PageItems",
                column: "BannerFeatureId",
                principalTable: "ImageFeatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageHeaders_ImageFeatureTemplates_LayoutBannerTemplateId",
                table: "PageHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_PageItems_Decks_DeckId",
                table: "PageItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PageItems_ImageFeatures_BannerFeatureId",
                table: "PageItems");

            migrationBuilder.DropTable(
                name: "CardDetails");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Decks");

            migrationBuilder.DropIndex(
                name: "IX_PageItems_BannerFeatureId",
                table: "PageItems");

            migrationBuilder.DropIndex(
                name: "IX_PageItems_DeckId",
                table: "PageItems");

            migrationBuilder.DropIndex(
                name: "IX_PageHeaders_LayoutBannerTemplateId",
                table: "PageHeaders");

            migrationBuilder.DropColumn(
                name: "BannerFeatureId",
                table: "PageItems");

            migrationBuilder.DropColumn(
                name: "DeckId",
                table: "PageItems");

            migrationBuilder.DropColumn(
                name: "LayoutBannerTemplateId",
                table: "PageHeaders");
        }
    }
}
