using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    /// <inheritdoc />
    public partial class prom_v100267 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NavBannerId",
                table: "PageItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NavBanners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavBanners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NavBannerImages",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    NavBannerId = table.Column<int>(type: "int", nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ImageAltText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavBannerImages", x => new { x.NavBannerId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_NavBannerImages_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavBannerImages_NavBanners_NavBannerId",
                        column: x => x.NavBannerId,
                        principalTable: "NavBanners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NavBannerLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Icon = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NavBannerId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavBannerLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NavBannerLinks_NavBanners_NavBannerId",
                        column: x => x.NavBannerId,
                        principalTable: "NavBanners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NavBannerLinkTexts",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    NavBannerLinkId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavBannerLinkTexts", x => new { x.NavBannerLinkId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_NavBannerLinkTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavBannerLinkTexts_NavBannerLinks_NavBannerLinkId",
                        column: x => x.NavBannerLinkId,
                        principalTable: "NavBannerLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageItems_NavBannerId",
                table: "PageItems",
                column: "NavBannerId");

            migrationBuilder.CreateIndex(
                name: "IX_NavBannerImages_LanguageId",
                table: "NavBannerImages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_NavBannerLinks_NavBannerId",
                table: "NavBannerLinks",
                column: "NavBannerId");

            migrationBuilder.CreateIndex(
                name: "IX_NavBannerLinkTexts_LanguageId",
                table: "NavBannerLinkTexts",
                column: "LanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageItems_NavBanners_NavBannerId",
                table: "PageItems",
                column: "NavBannerId",
                principalTable: "NavBanners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageItems_NavBanners_NavBannerId",
                table: "PageItems");

            migrationBuilder.DropTable(
                name: "NavBannerImages");

            migrationBuilder.DropTable(
                name: "NavBannerLinkTexts");

            migrationBuilder.DropTable(
                name: "NavBannerLinks");

            migrationBuilder.DropTable(
                name: "NavBanners");

            migrationBuilder.DropIndex(
                name: "IX_PageItems_NavBannerId",
                table: "PageItems");

            migrationBuilder.DropColumn(
                name: "NavBannerId",
                table: "PageItems");
        }
    }
}
