using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanResourcesManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOTScheduleDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationHours",
                table: "OTSchedules");

            migrationBuilder.RenameColumn(
                name: "WorkDate",
                table: "OTSchedules",
                newName: "ToDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "FromDate",
                table: "OTSchedules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ToTime",
                table: "OTSchedules",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromDate",
                table: "OTSchedules");

            migrationBuilder.DropColumn(
                name: "ToTime",
                table: "OTSchedules");

            migrationBuilder.RenameColumn(
                name: "ToDate",
                table: "OTSchedules",
                newName: "WorkDate");

            migrationBuilder.AddColumn<double>(
                name: "DurationHours",
                table: "OTSchedules",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
