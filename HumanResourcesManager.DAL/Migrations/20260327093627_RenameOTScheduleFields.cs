using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanResourcesManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameOTScheduleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ToTime",
                table: "OverTimeRequests",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "FromTime",
                table: "OverTimeRequests",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "ToTime",
                table: "OTSchedules",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "ToDate",
                table: "OTSchedules",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "FromTime",
                table: "OTSchedules",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "FromDate",
                table: "OTSchedules",
                newName: "EndDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "OverTimeRequests",
                newName: "ToTime");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "OverTimeRequests",
                newName: "FromTime");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "OTSchedules",
                newName: "ToTime");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "OTSchedules",
                newName: "ToDate");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "OTSchedules",
                newName: "FromTime");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "OTSchedules",
                newName: "FromDate");
        }
    }
}
