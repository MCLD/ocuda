using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v100210 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SegmentWrapId",
                table: "Segments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SegmentWrap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentWrap", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Segments_SegmentWrapId",
                table: "Segments",
                column: "SegmentWrapId");

            migrationBuilder.AddForeignKey(
                name: "FK_Segments_SegmentWrap_SegmentWrapId",
                table: "Segments",
                column: "SegmentWrapId",
                principalTable: "SegmentWrap",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Segments_SegmentWrap_SegmentWrapId",
                table: "Segments");

            migrationBuilder.DropTable(
                name: "SegmentWrap");

            migrationBuilder.DropIndex(
                name: "IX_Segments_SegmentWrapId",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "SegmentWrapId",
                table: "Segments");
        }
    }
}
