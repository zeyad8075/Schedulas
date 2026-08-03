using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schedulas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PhaseG1FinalSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "duration",
                table: "activities",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "end_time",
                table: "activities",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "priority",
                table: "activities",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "duration",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "end_time",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "activities");
        }
    }
}
