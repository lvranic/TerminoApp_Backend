using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerminoApp_NewBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitFixServiceProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DurationMinutes",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 30,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ProviderId",
                table: "Reservations",
                column: "ProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_ProviderId",
                table: "Reservations",
                column: "ProviderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_ProviderId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ProviderId",
                table: "Reservations");

            migrationBuilder.AlterColumn<int>(
                name: "DurationMinutes",
                table: "Services",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 30);
        }
    }
}
