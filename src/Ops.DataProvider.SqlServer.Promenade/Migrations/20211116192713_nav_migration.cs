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
                        name: "FK_NavigationTextsTemp_Navigations_NavigationId",
                        column: x => x.NavigationId,
                        principalTable: "Navigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavigationTextsTemp_Languages_LanguageId",
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

            migrationBuilder.CreateIndex(
                name: "IX_NavigationTexts_LanguageId",
                table: "NavigationTexts",
                column: "LanguageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NavigationTexts");

            migrationBuilder.CreateTable(
               name: "NavigationTexts",
               columns: table => new
               {
                   Id = table.Column<int>(nullable: false),
                   LanguageId = table.Column<int>(nullable: false),
                   Link = table.Column<string>(maxLength: 255, nullable: true),
                   Label = table.Column<string>(maxLength: 255, nullable: true),
                   Title = table.Column<string>(maxLength: 255, nullable: true),
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_NavigationTexts", x => new { x.Id, x.LanguageId });
               });

            migrationBuilder.AddColumn<int>(
                name: "NavigationTextId",
                table: "Navigations",
                type: "int",
                nullable: true);
        }
    }
}
