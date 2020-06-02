using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    public partial class ops_v10042 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(nullable: false),
                    OverrideEmailToAddress = table.Column<string>(nullable: true),
                    RestrictToDomain = table.Column<string>(nullable: true),
                    BccEmailAddress = table.Column<string>(nullable: true),
                    FromEmailAddress = table.Column<string>(nullable: false),
                    FromName = table.Column<string>(nullable: false),
                    BodyText = table.Column<string>(nullable: false),
                    BodyHtml = table.Column<string>(nullable: false),
                    SentResponse = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailSetups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    FromEmailAddress = table.Column<string>(maxLength: 255, nullable: false),
                    FromName = table.Column<string>(maxLength: 255, nullable: false),
                    EmailTemplateId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSetups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailSetups_EmailTemplates_EmailTemplateId",
                        column: x => x.EmailTemplateId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplateTexts",
                columns: table => new
                {
                    EmailTemplateId = table.Column<int>(nullable: false),
                    PromenadeLanguageName = table.Column<string>(maxLength: 255, nullable: false),
                    TemplateHtml = table.Column<string>(nullable: false),
                    TemplateMjml = table.Column<string>(nullable: false),
                    TemplateText = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateTexts", x => new { x.EmailTemplateId, x.PromenadeLanguageName });
                    table.ForeignKey(
                        name: "FK_EmailTemplateTexts_EmailTemplates_EmailTemplateId",
                        column: x => x.EmailTemplateId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailSetupTexts",
                columns: table => new
                {
                    EmailSetupId = table.Column<int>(nullable: false),
                    PromenadeLanguageName = table.Column<string>(maxLength: 255, nullable: false),
                    UrlParameters = table.Column<string>(maxLength: 255, nullable: true),
                    Preview = table.Column<string>(maxLength: 255, nullable: true),
                    BodyText = table.Column<string>(nullable: true),
                    BodyHtml = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSetupTexts", x => new { x.EmailSetupId, x.PromenadeLanguageName });
                    table.ForeignKey(
                        name: "FK_EmailSetupTexts_EmailSetups_EmailSetupId",
                        column: x => x.EmailSetupId,
                        principalTable: "EmailSetups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailSetups_EmailTemplateId",
                table: "EmailSetups",
                column: "EmailTemplateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailRecords");

            migrationBuilder.DropTable(
                name: "EmailSetupTexts");

            migrationBuilder.DropTable(
                name: "EmailTemplateTexts");

            migrationBuilder.DropTable(
                name: "EmailSetups");

            migrationBuilder.DropTable(
                name: "EmailTemplates");
        }
    }
}
