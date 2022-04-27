using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    public partial class ops_v100182 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TitleClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TitleClasses_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TitleClasses_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TitleClassMappings",
                columns: table => new
                {
                    TitleClassId = table.Column<int>(type: "int", nullable: false),
                    UserTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleClassMappings", x => new { x.TitleClassId, x.UserTitle });
                    table.ForeignKey(
                        name: "FK_TitleClassMappings_TitleClasses_TitleClassId",
                        column: x => x.TitleClassId,
                        principalTable: "TitleClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TitleClasses_CreatedBy",
                table: "TitleClasses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TitleClasses_UpdatedBy",
                table: "TitleClasses",
                column: "UpdatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TitleClassMappings");

            migrationBuilder.DropTable(
                name: "TitleClasses");
        }
    }
}
