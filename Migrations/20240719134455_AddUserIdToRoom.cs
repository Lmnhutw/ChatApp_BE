using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Users_ApplicationUserId",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Rooms",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_ApplicationUserId",
                table: "Rooms",
                newName: "IX_Rooms_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Users_Id",
                table: "Rooms",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Users_Id",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Rooms",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_Id",
                table: "Rooms",
                newName: "IX_Rooms_ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Users_ApplicationUserId",
                table: "Rooms",
                column: "ApplicationUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
