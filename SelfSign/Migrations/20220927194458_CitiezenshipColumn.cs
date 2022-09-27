using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SelfSign.Migrations
{
    public partial class CitiezenshipColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Citizenship",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Citizenship",
                table: "Users");
        }
    }
}
