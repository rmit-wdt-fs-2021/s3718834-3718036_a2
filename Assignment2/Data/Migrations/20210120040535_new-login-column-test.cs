using Microsoft.EntityFrameworkCore.Migrations;

namespace Assignment2.Data.Migrations
{
    public partial class newlogincolumntest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerTag",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerTag",
                table: "AspNetUsers");
        }
    }
}
