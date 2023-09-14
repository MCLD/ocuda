using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Ops.Migrations
{
    /// <inheritdoc />
    public partial class ops_v100246 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VolunteerFormSubmissionEmailRecords",
                columns: table => new
                {
                    EmailRecordId = table.Column<int>(type: "int", nullable: false),
                    VolunterFormSubmissionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerFormSubmissionEmailRecords", x => new { x.VolunterFormSubmissionId, x.EmailRecordId });
                    table.ForeignKey(
                        name: "FK_VolunteerFormSubmissionEmailRecords_EmailRecords_EmailRecordId",
                        column: x => x.EmailRecordId,
                        principalTable: "EmailRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VolunteerFormSubmissionEmailRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerFormUserMappings",
                columns: table => new
                {
                    VolunteerFormId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerFormUserMappings", x => new { x.VolunteerFormId, x.LocationId, x.UserId });
                    table.ForeignKey(
                        name: "FK_VolunteerFormUserMappings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerFormSubmissionEmailRecords_EmailRecordId",
                table: "VolunteerFormSubmissionEmailRecords",
                column: "EmailRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerFormSubmissionEmailRecords_UserId",
                table: "VolunteerFormSubmissionEmailRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerFormUserMappings_UserId",
                table: "VolunteerFormUserMappings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VolunteerFormSubmissionEmailRecords");

            migrationBuilder.DropTable(
                name: "VolunteerFormUserMappings");
        }
    }
}
