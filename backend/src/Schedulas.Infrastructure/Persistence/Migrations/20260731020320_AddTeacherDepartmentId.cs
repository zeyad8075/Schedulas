using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schedulas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherDepartmentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "department_id",
                table: "teachers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_teachers_department_id",
                table: "teachers",
                column: "department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_teachers_departments_department_id",
                table: "teachers",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teachers_departments_department_id",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "ix_teachers_department_id",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "teachers");
        }
    }
}
