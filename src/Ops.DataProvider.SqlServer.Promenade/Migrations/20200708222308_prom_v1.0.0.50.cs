﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Ocuda.Ops.DataProvider.SqlServer.Promenade.Migrations
{
    public partial class prom_v10050 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HoursSegmentId",
                table: "Locations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoursSegmentId",
                table: "Locations");
        }
    }
}
