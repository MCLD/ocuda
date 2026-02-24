using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    /// <inheritdoc />
    public partial class prom_v100346 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RenewCardRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianBarcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuardianName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDiscarded = table.Column<bool>(type: "bit", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SameAddress = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewCardRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenewCardRequests_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RenewCardRequests_LanguageId",
                table: "RenewCardRequests",
                column: "LanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RenewCardRequests");
        }
    }
}
