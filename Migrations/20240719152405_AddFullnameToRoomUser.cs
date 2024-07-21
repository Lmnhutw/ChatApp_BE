using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddFullnameToRoomUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "RoomUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "RoomUsers");
        }
    }
}
