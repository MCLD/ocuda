using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    /// <inheritdoc />
    public partial class prom_v100254 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ScheduleRequestSubject",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrderBy",
                table: "ScheduleRequestSubject",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // set all items to active by default
            migrationBuilder.Sql("UPDATE [ScheduleRequestSubject] SET [IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ScheduleRequestSubject");

            migrationBuilder.DropColumn(
                name: "OrderBy",
                table: "ScheduleRequestSubject");
        }
    }
}
