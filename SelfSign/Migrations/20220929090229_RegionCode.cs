using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SelfSign.Migrations
{
    public partial class RegionCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegionCode",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegionCode",
                table: "Users");
        }
    }
}
