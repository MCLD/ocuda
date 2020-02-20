using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v1002cs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.CreateTable(
                name: "EmediaGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    SortOrder = table.Column<int>(nullable: false),
                    SegmentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmediaGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmediaGroups_Segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmediaGroups_SegmentId",
                table: "EmediaGroups",
                column: "SegmentId");

            migrationBuilder.InsertData(
                table: "EmediaGroups",
                columns: new[] {"Id", "Name", "SortOrder" },
                values: new object[] { 1, "Default", 1});

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Emedia",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Emedia_GroupId",
                table: "Emedia",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Emedia_EmediaGroups_GroupId",
                table: "Emedia",
                column: "GroupId",
                principalTable: "EmediaGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emedia_EmediaGroups_GroupId",
                table: "Emedia");

            migrationBuilder.DropIndex(
               name: "IX_Emedia_GroupId",
               table: "Emedia");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Emedia");

            migrationBuilder.DropTable(
                name: "EmediaGroups");
        }
    }
}
