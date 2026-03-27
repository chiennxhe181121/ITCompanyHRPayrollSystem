using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanResourcesManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOTSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OTScheduleId",
                table: "OverTimeRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OTSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DurationHours = table.Column<double>(type: "float", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OTSchedules_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OTSchedules_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OverTimeRequests_OTScheduleId",
                table: "OverTimeRequests",
                column: "OTScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_OTSchedules_DepartmentId",
                table: "OTSchedules",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OTSchedules_ManagerId",
                table: "OTSchedules",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_OverTimeRequests_OTSchedules_OTScheduleId",
                table: "OverTimeRequests",
                column: "OTScheduleId",
                principalTable: "OTSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OverTimeRequests_OTSchedules_OTScheduleId",
                table: "OverTimeRequests");

            migrationBuilder.DropTable(
                name: "OTSchedules");

            migrationBuilder.DropIndex(
                name: "IX_OverTimeRequests_OTScheduleId",
                table: "OverTimeRequests");

            migrationBuilder.DropColumn(
                name: "OTScheduleId",
                table: "OverTimeRequests");
        }
    }
}
