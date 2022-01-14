using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class nav1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NavigationTextsTemp",
                columns: table => new
                {
                    NavigationId = table.Column<int>(nullable: false),
                    LanguageId = table.Column<int>(nullable: false),
                    Label = table.Column<string>(maxLength: 255, nullable: true),
                    Link = table.Column<string>(maxLength: 255, nullable: true),
                    Title = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationTextsTemp", x => new { x.NavigationId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_NavigationTexts_Navigations_NavigationId",
                        column: x => x.NavigationId,
                        principalTable: "Navigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavigationTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"INSERT INTO NavigationTextsTemp (NavigationId, LanguageId, Label, Link, Title)
                                    SELECT N.Id, NT.LanguageId, NT.Label, NT.Link, NT.Title
                                    FROM NavigationTexts AS NT
                                    JOIN Navigations AS N
                                    ON NT.Id = N.NavigationTextId");

            migrationBuilder.DropColumn(
                name: "NavigationTextId",
                table: "Navigations");

            migrationBuilder.DropTable(
                name: "NavigationTexts");

            migrationBuilder.RenameTable(
                name: "NavigationTextsTemp",
                newName: "NavigationTexts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NavigationTextsTemp",
                table: "NavigationTexts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NavigationTexts",
                table: "NavigationTexts",
                columns: new[] { "NavigationId", "LanguageId" });

            migrationBuilder.CreateIndex(
                name: "IX_NavigationTexts_LanguageId",
                table: "NavigationTexts",
                column: "LanguageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NavigationTexts_LanguageId",
                table: "NavigationTexts");

            migrationBuilder.DropForeignKey(
                name: "FK_NavigationTexts_Navigations_NavigationId",
                table: "NavigationTexts");

            migrationBuilder.DropForeignKey(
                name: "FK_NavigationTexts_Languages_LanguageId",
                table: "NavigationTexts");

            migrationBuilder.RenameColumn(
                name: "NavigationId",
                table: "NavigationTexts",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "NavigationTextId",
                table: "Navigations",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE Navigations
                                    SET NavigationTextId = N.Id
                                    FROM Navigations AS N
                                    JOIN NavigationTexts AS NT
                                    ON N.Id = NT.Id");
        }
    }
}
