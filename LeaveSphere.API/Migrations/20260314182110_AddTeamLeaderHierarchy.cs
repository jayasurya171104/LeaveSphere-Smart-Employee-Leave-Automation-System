using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveSphere.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamLeaderHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "LeaveRequests",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "LeaveRequests",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TeamLeaderId",
                table: "LeaveRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "LeaveBalances",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TeamLeaderId",
                table: "LeaveBalances",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Employees",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TeamLeaders",
                columns: table => new
                {
                    TeamLeaderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateOfJoining = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamLeaders", x => x.TeamLeaderId);
                    table.ForeignKey(
                        name: "FK_TeamLeaders_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_TeamLeaderId",
                table: "LeaveRequests",
                column: "TeamLeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_TeamLeaderId",
                table: "LeaveBalances",
                column: "TeamLeaderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamLeaders_DepartmentId",
                table: "TeamLeaders",
                column: "DepartmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalances_TeamLeaders_TeamLeaderId",
                table: "LeaveBalances",
                column: "TeamLeaderId",
                principalTable: "TeamLeaders",
                principalColumn: "TeamLeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_TeamLeaders_TeamLeaderId",
                table: "LeaveRequests",
                column: "TeamLeaderId",
                principalTable: "TeamLeaders",
                principalColumn: "TeamLeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalances_TeamLeaders_TeamLeaderId",
                table: "LeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_TeamLeaders_TeamLeaderId",
                table: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "TeamLeaders");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_TeamLeaderId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveBalances_TeamLeaderId",
                table: "LeaveBalances");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "TeamLeaderId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "TeamLeaderId",
                table: "LeaveBalances");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "LeaveBalances",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "PasswordHash",
                keyValue: null,
                column: "PasswordHash",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Employees",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalances_Employees_EmployeeId",
                table: "LeaveBalances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
