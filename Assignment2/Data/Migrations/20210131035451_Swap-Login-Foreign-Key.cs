using Microsoft.EntityFrameworkCore.Migrations;

namespace Assignment2.Data.Migrations
{
    public partial class SwapLoginForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Customer_CustomerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CustomerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "LoginId",
                table: "Customer",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_LoginId",
                table: "Customer",
                column: "LoginId",
                unique: true,
                filter: "[LoginId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_AspNetUsers_LoginId",
                table: "Customer",
                column: "LoginId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_AspNetUsers_LoginId",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_LoginId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "LoginId",
                table: "Customer");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CustomerId",
                table: "AspNetUsers",
                column: "CustomerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Customer_CustomerId",
                table: "AspNetUsers",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
