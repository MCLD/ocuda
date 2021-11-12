using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    public partial class ops_v100135 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHomeSection",
                table: "Sections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SupervisorsOnly",
                table: "Sections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_SectionId",
                table: "Posts",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_FileLibraries_SectionId",
                table: "FileLibraries",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileLibraries_Sections_SectionId",
                table: "FileLibraries",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Sections_SectionId",
                table: "Posts",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                "DECLARE @SysadminId int = (SELECT TOP 1[Id] FROM[Users] WHERE[IsSysadmin] = 1);"
                + "IF @SysadminId IS NULL"
                + " BEGIN"
                + "  INSERT INTO[Users]([CreatedAt], [CreatedBy], [ExcludeFromRoster], [IsDeleted],"
                + "    [IsSysadmin], [Name], [ReauthenticateUser], [Username])"
                + "    VALUES (GETDATE(), 0, 0, 0, 1, 'System', 0, 'sysadmin');"
                + "  SET @SysadminId = SCOPE_IDENTITY();"
                + " END"
                + " INSERT INTO [Sections] "
                + " ([CreatedAt], [CreatedBy], [Name], [Icon], [Stub], [IsHomeSection])"
                + " VALUES(GETDATE(), @SysadminId, 'Home', 'fas fa-info-circle', 'Home', 1);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileLibraries_Sections_SectionId",
                table: "FileLibraries");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Sections_SectionId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_SectionId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_FileLibraries_SectionId",
                table: "FileLibraries");

            migrationBuilder.DropColumn(
                name: "IsHomeSection",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "SupervisorsOnly",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Posts");
        }
    }
}