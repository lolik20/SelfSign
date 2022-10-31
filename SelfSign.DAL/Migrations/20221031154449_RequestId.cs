using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SelfSign.DAL.Migrations
{
    public partial class RequestId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MyDssRequestId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Documents");

            migrationBuilder.AddColumn<Guid>(
                name: "MyDssRequestId",
                table: "Users",
                type: "uuid",
                nullable: true);
        }
    }
}
