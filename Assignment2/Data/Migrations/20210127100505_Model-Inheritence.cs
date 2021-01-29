using Microsoft.EntityFrameworkCore.Migrations;

namespace Assignment2.Data.Migrations
{
    public partial class ModelInheritence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PayeeName",
                table: "Payee",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CustomerName",
                table: "Customer",
                newName: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Payee",
                newName: "PayeeName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Customer",
                newName: "CustomerName");
        }
    }
}
