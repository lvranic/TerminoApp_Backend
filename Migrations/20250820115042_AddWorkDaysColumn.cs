using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminoApp_NewBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkDaysColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "WorkDays",
                table: "Users",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingHoursRange",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkDays",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WorkingHoursRange",
                table: "Users");
        }
    }
}
