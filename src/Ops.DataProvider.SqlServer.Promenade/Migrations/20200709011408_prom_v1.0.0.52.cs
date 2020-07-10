using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10051 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SegmentTexts",
                table: "SegmentTexts");

            migrationBuilder.DropIndex(
                name: "IX_SegmentTexts_LanguageId",
                table: "SegmentTexts");

            migrationBuilder.DropIndex(
                name: "IX_SegmentTexts_SegmentId",
                table: "SegmentTexts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SegmentTexts",
                table: "SegmentTexts",
                columns: new[] { "LanguageId", "SegmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_SegmentTexts_SegmentId",
                table: "SegmentTexts",
                column: "SegmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SegmentTexts",
                table: "SegmentTexts");

            migrationBuilder.DropIndex(
                name: "IX_SegmentTexts_SegmentId",
                table: "SegmentTexts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SegmentTexts",
                table: "SegmentTexts",
                columns: new[] { "SegmentId", "LanguageId" });

            migrationBuilder.CreateIndex(
                name: "IX_SegmentTexts_LanguageId",
                table: "SegmentTexts",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentTexts_SegmentId",
                table: "SegmentTexts",
                column: "SegmentId",
                unique: true);
        }
    }
}
