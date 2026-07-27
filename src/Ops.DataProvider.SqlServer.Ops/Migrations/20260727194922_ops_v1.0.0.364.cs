using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    /// <inheritdoc />
    public partial class ops_v100364 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "LinkLibraries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FileDate",
                table: "Files",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "FileLibraries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "FileLibraries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FileThumbnails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: false),
                    ThumbnailFile = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileThumbnails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileThumbnails_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileThumbnails_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileThumbnails_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileThumbnails_CreatedBy",
                table: "FileThumbnails",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FileThumbnails_FileId",
                table: "FileThumbnails",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_FileThumbnails_UpdatedBy",
                table: "FileThumbnails",
                column: "UpdatedBy");

            migrationBuilder.Sql("UPDATE [FileLibraries] SET [IsFeatured] = 1;"
                + "UPDATE [LinkLibraries] SET [IsFeatured] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileThumbnails");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "LinkLibraries");

            migrationBuilder.DropColumn(
                name: "FileDate",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "FileLibraries");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "FileLibraries");
        }
    }
}
