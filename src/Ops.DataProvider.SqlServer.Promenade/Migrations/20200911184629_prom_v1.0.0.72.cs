using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10072 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLayoutPage",
                table: "PageHeaders",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CarouselButtonLabels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarouselButtonLabels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Carousels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carousels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageLayouts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageHeaderId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    SocialCardId = table.Column<int>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageLayouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageLayouts_PageHeaders_PageHeaderId",
                        column: x => x.PageHeaderId,
                        principalTable: "PageHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageLayouts_SocialCards_SocialCardId",
                        column: x => x.SocialCardId,
                        principalTable: "SocialCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarouselButtonLabelTexts",
                columns: table => new
                {
                    CarouselButtonLabelId = table.Column<int>(nullable: false),
                    LanguageId = table.Column<int>(nullable: false),
                    Text = table.Column<string>(maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarouselButtonLabelTexts", x => new { x.CarouselButtonLabelId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_CarouselButtonLabelTexts_CarouselButtonLabels_CarouselButtonLabelId",
                        column: x => x.CarouselButtonLabelId,
                        principalTable: "CarouselButtonLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarouselButtonLabelTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarouselItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarouselId = table.Column<int>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarouselItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarouselItems_Carousels_CarouselId",
                        column: x => x.CarouselId,
                        principalTable: "Carousels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarouselTexts",
                columns: table => new
                {
                    CarouselId = table.Column<int>(nullable: false),
                    LanguageId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarouselTexts", x => new { x.CarouselId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_CarouselTexts_Carousels_CarouselId",
                        column: x => x.CarouselId,
                        principalTable: "Carousels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarouselTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PageItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageLayoutId = table.Column<int>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    CarouselId = table.Column<int>(nullable: true),
                    SegmentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageItems_Carousels_CarouselId",
                        column: x => x.CarouselId,
                        principalTable: "Carousels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageItems_PageLayouts_PageLayoutId",
                        column: x => x.PageLayoutId,
                        principalTable: "PageLayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageItems_Segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PageLayoutTexts",
                columns: table => new
                {
                    PageLayoutId = table.Column<int>(nullable: false),
                    LanguageId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageLayoutTexts", x => new { x.PageLayoutId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_PageLayoutTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageLayoutTexts_PageLayouts_PageLayoutId",
                        column: x => x.PageLayoutId,
                        principalTable: "PageLayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarouselButtons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarouselItemId = table.Column<int>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    Url = table.Column<string>(maxLength: 255, nullable: false),
                    LabelId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarouselButtons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarouselButtons_CarouselItems_CarouselItemId",
                        column: x => x.CarouselItemId,
                        principalTable: "CarouselItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarouselButtons_CarouselButtonLabels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "CarouselButtonLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarouselItemTexts",
                columns: table => new
                {
                    CarouselItemId = table.Column<int>(nullable: false),
                    LanguageId = table.Column<int>(nullable: false),
                    Label = table.Column<string>(maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(maxLength: 255, nullable: false),
                    Title = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarouselItemTexts", x => new { x.CarouselItemId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_CarouselItemTexts_CarouselItems_CarouselItemId",
                        column: x => x.CarouselItemId,
                        principalTable: "CarouselItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarouselItemTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarouselButtonLabelTexts_LanguageId",
                table: "CarouselButtonLabelTexts",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_CarouselButtons_CarouselItemId",
                table: "CarouselButtons",
                column: "CarouselItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CarouselButtons_LabelId",
                table: "CarouselButtons",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_CarouselItems_CarouselId",
                table: "CarouselItems",
                column: "CarouselId");

            migrationBuilder.CreateIndex(
                name: "IX_CarouselItemTexts_LanguageId",
                table: "CarouselItemTexts",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_CarouselTexts_LanguageId",
                table: "CarouselTexts",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_PageItems_CarouselId",
                table: "PageItems",
                column: "CarouselId");

            migrationBuilder.CreateIndex(
                name: "IX_PageItems_PageLayoutId",
                table: "PageItems",
                column: "PageLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_PageItems_SegmentId",
                table: "PageItems",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PageLayouts_PageHeaderId",
                table: "PageLayouts",
                column: "PageHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PageLayouts_SocialCardId",
                table: "PageLayouts",
                column: "SocialCardId");

            migrationBuilder.CreateIndex(
                name: "IX_PageLayoutTexts_LanguageId",
                table: "PageLayoutTexts",
                column: "LanguageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarouselButtonLabelTexts");

            migrationBuilder.DropTable(
                name: "CarouselButtons");

            migrationBuilder.DropTable(
                name: "CarouselItemTexts");

            migrationBuilder.DropTable(
                name: "CarouselTexts");

            migrationBuilder.DropTable(
                name: "PageItems");

            migrationBuilder.DropTable(
                name: "PageLayoutTexts");

            migrationBuilder.DropTable(
                name: "CarouselButtonLabels");

            migrationBuilder.DropTable(
                name: "CarouselItems");

            migrationBuilder.DropTable(
                name: "PageLayouts");

            migrationBuilder.DropTable(
                name: "Carousels");

            migrationBuilder.DropColumn(
                name: "IsLayoutPage",
                table: "PageHeaders");
        }
    }
}
