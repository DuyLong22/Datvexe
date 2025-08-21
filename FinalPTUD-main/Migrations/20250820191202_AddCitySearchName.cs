using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusTicketBooking.Migrations
{
    public partial class AddCitySearchName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
            name: "SearchName",
            table: "Cities",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "SearchName",
            table: "Cities");
        }
    }
}
