using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    public partial class ops_v100146 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SectionManagerGroups");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "Stub",
                table: "Sections",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "Stub",
                table: "Posts",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "Stub",
                table: "LinkLibraries",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "Stub",
                table: "FileLibraries",
                newName: "Slug");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishedAt",
                table: "Posts",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "PinnedUntil",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PermissionGroupReplaceFiles",
                columns: table => new
                {
                    PermissionGroupId = table.Column<int>(type: "int", nullable: false),
                    FileLibraryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionGroupReplaceFiles", x => new { x.PermissionGroupId, x.FileLibraryId });
                    table.ForeignKey(
                        name: "FK_PermissionGroupReplaceFiles_FileLibraries_FileLibraryId",
                        column: x => x.FileLibraryId,
                        principalTable: "FileLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PermissionGroupSectionManager",
                columns: table => new
                {
                    PermissionGroupId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionGroupSectionManager", x => new { x.PermissionGroupId, x.SectionId });
                    table.ForeignKey(
                        name: "FK_PermissionGroupSectionManager_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionGroupReplaceFiles_FileLibraryId",
                table: "PermissionGroupReplaceFiles",
                column: "FileLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionGroupSectionManager_SectionId",
                table: "PermissionGroupSectionManager",
                column: "SectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionGroupReplaceFiles");

            migrationBuilder.DropTable(
                name: "PermissionGroupSectionManager");

            migrationBuilder.DropColumn(
                name: "PinnedUntil",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "Sections",
                newName: "Stub");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "Posts",
                newName: "Stub");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "LinkLibraries",
                newName: "Stub");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "FileLibraries",
                newName: "Stub");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PublishedAt",
                table: "Posts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SectionManagerGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionManagerGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionManagerGroups_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectionManagerGroups_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectionManagerGroups_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SectionManagerGroups_CreatedBy",
                table: "SectionManagerGroups",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SectionManagerGroups_SectionId",
                table: "SectionManagerGroups",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionManagerGroups_UpdatedBy",
                table: "SectionManagerGroups",
                column: "UpdatedBy");
        }
    }
}
