using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    /// <inheritdoc />
    public partial class prom_v100353 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Emedia",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Emedia",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableExternally",
                table: "Emedia",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHttpPost",
                table: "Emedia",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Emedia",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EmediaAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmediaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmediaAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmediaAccesses_Emedia_EmediaId",
                        column: x => x.EmediaId,
                        principalTable: "Emedia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmediaSubjects",
                columns: table => new
                {
                    EmediaId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmediaSubjects", x => new { x.SubjectId, x.EmediaId });
                    table.ForeignKey(
                        name: "FK_EmediaSubjects_Emedia_EmediaId",
                        column: x => x.EmediaId,
                        principalTable: "Emedia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmediaSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectTexts",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectTexts", x => new { x.LanguageId, x.SubjectId });
                    table.ForeignKey(
                        name: "FK_SubjectTexts_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectTexts_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmediaAccesses_EmediaId",
                table: "EmediaAccesses",
                column: "EmediaId");

            migrationBuilder.CreateIndex(
                name: "IX_EmediaSubjects_EmediaId",
                table: "EmediaSubjects",
                column: "EmediaId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectTexts_SubjectId",
                table: "SubjectTexts",
                column: "SubjectId");

            migrationBuilder.Sql("UPDATE [Emedia] SET [IsActive] = 1, [IsAvailableExternally] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmediaAccesses");

            migrationBuilder.DropTable(
                name: "EmediaSubjects");

            migrationBuilder.DropTable(
                name: "SubjectTexts");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Emedia");

            migrationBuilder.DropColumn(
                name: "IsAvailableExternally",
                table: "Emedia");

            migrationBuilder.DropColumn(
                name: "IsHttpPost",
                table: "Emedia");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Emedia");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Emedia",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
