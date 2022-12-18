using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SelfSign.DAL.Migrations
{
    public partial class RequestHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_History_Users_UserId",
                table: "History");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "History",
                newName: "RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_History_UserId",
                table: "History",
                newName: "IX_History_RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_History_Requests_RequestId",
                table: "History",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_History_Requests_RequestId",
                table: "History");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "History",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_History_RequestId",
                table: "History",
                newName: "IX_History_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_History_Users_UserId",
                table: "History",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
