using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    /// <inheritdoc />
    public partial class ops_v100346 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RenewCardResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailSetupId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewCardResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenewCardResponses_EmailSetups_EmailSetupId",
                        column: x => x.EmailSetupId,
                        principalTable: "EmailSetups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RenewCardResponses_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RenewCardResponses_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RenewCardResults",
                columns: table => new
                {
                    RenewCardRequestId = table.Column<int>(type: "int", nullable: false),
                    RenewCardResponseId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    IsDiscarded = table.Column<bool>(type: "bit", nullable: false),
                    ResponseText = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewCardResults", x => x.RenewCardRequestId);
                    table.ForeignKey(
                        name: "FK_RenewCardResults_RenewCardResponses_RenewCardResponseId",
                        column: x => x.RenewCardResponseId,
                        principalTable: "RenewCardResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RenewCardResults_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RenewCardResponses_CreatedBy",
                table: "RenewCardResponses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RenewCardResponses_EmailSetupId",
                table: "RenewCardResponses",
                column: "EmailSetupId");

            migrationBuilder.CreateIndex(
                name: "IX_RenewCardResponses_UpdatedBy",
                table: "RenewCardResponses",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RenewCardResults_CreatedBy",
                table: "RenewCardResults",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RenewCardResults_RenewCardResponseId",
                table: "RenewCardResults",
                column: "RenewCardResponseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RenewCardResults");

            migrationBuilder.DropTable(
                name: "RenewCardResponses");
        }
    }
}
