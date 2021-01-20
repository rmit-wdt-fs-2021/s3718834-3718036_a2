using Microsoft.EntityFrameworkCore.Migrations;

namespace Assignment2.Data.Migrations
{
    public partial class customerregisteration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerTag",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "CustomerForeignKey",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CustomerForeignKey",
                table: "AspNetUsers",
                column: "CustomerForeignKey",
                unique: true,
                filter: "[CustomerForeignKey] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Customer_CustomerForeignKey",
                table: "AspNetUsers",
                column: "CustomerForeignKey",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Customer_CustomerForeignKey",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CustomerForeignKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CustomerForeignKey",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "CustomerTag",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
