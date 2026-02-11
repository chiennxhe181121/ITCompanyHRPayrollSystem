using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanResourcesManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LatePenalty",
                table: "Payrolls",
                newName: "MissingMinutesPenalty");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "OTAttendances",
                newName: "CheckOutImagePath");

            migrationBuilder.RenameColumn(
                name: "CheckOutTime",
                table: "OTAttendances",
                newName: "CheckOut");

            migrationBuilder.RenameColumn(
                name: "CheckInTime",
                table: "OTAttendances",
                newName: "CheckIn");

            migrationBuilder.RenameColumn(
                name: "LateMinutes",
                table: "Attendances",
                newName: "Status");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OTAttendances",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "CheckInImagePath",
                table: "OTAttendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckInImagePath",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckOutImagePath",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MissingMinutes",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AnnualLeaveBalance",
                columns: table => new
                {
                    AnnualLeaveBalanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    EntitledDays = table.Column<double>(type: "float(5)", precision: 5, scale: 2, nullable: false),
                    UsedDays = table.Column<double>(type: "float(5)", precision: 5, scale: 2, nullable: false),
                    RemainingDays = table.Column<double>(type: "float", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnualLeaveBalance", x => x.AnnualLeaveBalanceId);
                    table.ForeignKey(
                        name: "FK_AnnualLeaveBalance_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnualLeaveBalance_EmployeeId_Year",
                table: "AnnualLeaveBalance",
                columns: new[] { "EmployeeId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnualLeaveBalance");

            migrationBuilder.DropColumn(
                name: "CheckInImagePath",
                table: "OTAttendances");

            migrationBuilder.DropColumn(
                name: "CheckInImagePath",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutImagePath",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "MissingMinutes",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "MissingMinutesPenalty",
                table: "Payrolls",
                newName: "LatePenalty");

            migrationBuilder.RenameColumn(
                name: "CheckOutImagePath",
                table: "OTAttendances",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "CheckOut",
                table: "OTAttendances",
                newName: "CheckOutTime");

            migrationBuilder.RenameColumn(
                name: "CheckIn",
                table: "OTAttendances",
                newName: "CheckInTime");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Attendances",
                newName: "LateMinutes");

            migrationBuilder.AlterColumn<long>(
                name: "Status",
                table: "OTAttendances",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
