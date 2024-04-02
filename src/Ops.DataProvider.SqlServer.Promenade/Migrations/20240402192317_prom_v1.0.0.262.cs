using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    /// <inheritdoc />
    public partial class prom_v100262 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationInteriorImageAltTexts",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    LocationInteriorImageId = table.Column<int>(type: "int", nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationInteriorImageAltTexts", x => new { x.LocationInteriorImageId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_LocationInteriorImageAltTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationInteriorImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImagePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationInteriorImages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationInteriorImageAltTexts_LanguageId",
                table: "LocationInteriorImageAltTexts",
                column: "LanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationInteriorImageAltTexts");

            migrationBuilder.DropTable(
                name: "LocationInteriorImages");
        }
    }
}
