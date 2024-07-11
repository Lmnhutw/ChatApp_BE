using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp_BE.Migrations
{
    /// <inheritdoc />
    public partial class updateDbField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_UserId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Messages",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_UserId",
                table: "Messages",
                newName: "IX_Messages_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_Id",
                table: "Messages",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_Id",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Messages",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_Id",
                table: "Messages",
                newName: "IX_Messages_UserId");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_UserId",
                table: "Messages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}