using Microsoft.EntityFrameworkCore.Migrations;

namespace Assignment2.Data.Migrations
{
    public partial class FixedForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Customer_CustomerForeignKey",
                table: "Account");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Customer_CustomerForeignKey",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BillPay_Account_AccountForeignKey",
                table: "BillPay");

            migrationBuilder.DropForeignKey(
                name: "FK_BillPay_Payee_PayeeForeignKey",
                table: "BillPay");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Account_AccountForeignKey",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_AccountForeignKey",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_BillPay_AccountForeignKey",
                table: "BillPay");

            migrationBuilder.DropIndex(
                name: "IX_BillPay_PayeeForeignKey",
                table: "BillPay");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CustomerForeignKey",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Account_CustomerForeignKey",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "AccountForeignKey",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "AccountForeignKey",
                table: "BillPay");

            migrationBuilder.DropColumn(
                name: "PayeeForeignKey",
                table: "BillPay");

            migrationBuilder.DropColumn(
                name: "CustomerForeignKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CustomerForeignKey",
                table: "Account");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AccountNumber",
                table: "Transaction",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_BillPay_AccountNumber",
                table: "BillPay",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_BillPay_PayeeId",
                table: "BillPay",
                column: "PayeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CustomerId",
                table: "AspNetUsers",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_CustomerId",
                table: "Account",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Customer_CustomerId",
                table: "Account",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Customer_CustomerId",
                table: "AspNetUsers",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BillPay_Account_AccountNumber",
                table: "BillPay",
                column: "AccountNumber",
                principalTable: "Account",
                principalColumn: "AccountNumber",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BillPay_Payee_PayeeId",
                table: "BillPay",
                column: "PayeeId",
                principalTable: "Payee",
                principalColumn: "PayeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account_AccountNumber",
                table: "Transaction",
                column: "AccountNumber",
                principalTable: "Account",
                principalColumn: "AccountNumber",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Customer_CustomerId",
                table: "Account");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Customer_CustomerId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BillPay_Account_AccountNumber",
                table: "BillPay");

            migrationBuilder.DropForeignKey(
                name: "FK_BillPay_Payee_PayeeId",
                table: "BillPay");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Account_AccountNumber",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_AccountNumber",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_BillPay_AccountNumber",
                table: "BillPay");

            migrationBuilder.DropIndex(
                name: "IX_BillPay_PayeeId",
                table: "BillPay");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CustomerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Account_CustomerId",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "AccountForeignKey",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountForeignKey",
                table: "BillPay",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayeeForeignKey",
                table: "BillPay",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerForeignKey",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerForeignKey",
                table: "Account",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AccountForeignKey",
                table: "Transaction",
                column: "AccountForeignKey");

            migrationBuilder.CreateIndex(
                name: "IX_BillPay_AccountForeignKey",
                table: "BillPay",
                column: "AccountForeignKey");

            migrationBuilder.CreateIndex(
                name: "IX_BillPay_PayeeForeignKey",
                table: "BillPay",
                column: "PayeeForeignKey");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CustomerForeignKey",
                table: "AspNetUsers",
                column: "CustomerForeignKey",
                unique: true,
                filter: "[CustomerForeignKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Account_CustomerForeignKey",
                table: "Account",
                column: "CustomerForeignKey");

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Customer_CustomerForeignKey",
                table: "Account",
                column: "CustomerForeignKey",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Customer_CustomerForeignKey",
                table: "AspNetUsers",
                column: "CustomerForeignKey",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillPay_Account_AccountForeignKey",
                table: "BillPay",
                column: "AccountForeignKey",
                principalTable: "Account",
                principalColumn: "AccountNumber",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillPay_Payee_PayeeForeignKey",
                table: "BillPay",
                column: "PayeeForeignKey",
                principalTable: "Payee",
                principalColumn: "PayeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account_AccountForeignKey",
                table: "Transaction",
                column: "AccountForeignKey",
                principalTable: "Account",
                principalColumn: "AccountNumber",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
