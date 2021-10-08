using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v100147 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleRequestSubjectTexts",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    ScheduleRequestSubjectId = table.Column<int>(type: "int", nullable: false),
                    SubjectText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleRequestSubjectTexts", x => new { x.ScheduleRequestSubjectId, x.LanguageId });
                });

            // populate the texts with the default values if present
            migrationBuilder.Sql("INSERT INTO [ScheduleRequestSubjectTexts]"
                + " SELECT l.[Id] [LanguageId], srs.[Id] [ScheduleRequestSubjectId]"
                + " , srs.[Subject] [SubjectText]"
                + " FROM [ScheduleRequestSubject] srs CROSS JOIN [Languages] l"
                + " WHERE l.[IsDefault] = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleRequestSubjectTexts");
        }
    }
}
