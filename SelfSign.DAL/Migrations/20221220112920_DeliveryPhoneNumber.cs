using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SelfSign.DAL.Migrations
{
    public partial class DeliveryPhoneNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Deliveries",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Deliveries");
        }
    }
}
