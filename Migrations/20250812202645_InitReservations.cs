using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminoApp_NewBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_AdminId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_UserId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_AdminId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "StartUtc",
                table: "Reservations",
                newName: "StartsAt");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "Reservations",
                newName: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ProviderId_StartsAt",
                table: "Reservations",
                columns: new[] { "ProviderId", "StartsAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_ProviderId",
                table: "Reservations",
                column: "ProviderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_UserId",
                table: "Reservations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_ProviderId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_UserId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ProviderId_StartsAt",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "StartsAt",
                table: "Reservations",
                newName: "StartUtc");

            migrationBuilder.RenameColumn(
                name: "ProviderId",
                table: "Reservations",
                newName: "AdminId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_AdminId",
                table: "Reservations",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_AdminId",
                table: "Reservations",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_UserId",
                table: "Reservations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
