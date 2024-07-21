using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp_BE.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Users_AdminId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "RoomUsers");

            migrationBuilder.DropColumn(
                name: "IsModerator",
                table: "RoomUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "Rooms",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_AdminId",
                table: "Rooms",
                newName: "IX_Rooms_ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Users_ApplicationUserId",
                table: "Rooms",
                column: "ApplicationUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Users_ApplicationUserId",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Rooms",
                newName: "AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_ApplicationUserId",
                table: "Rooms",
                newName: "IX_Rooms_AdminId");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "RoomUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsModerator",
                table: "RoomUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Users_AdminId",
                table: "Rooms",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
