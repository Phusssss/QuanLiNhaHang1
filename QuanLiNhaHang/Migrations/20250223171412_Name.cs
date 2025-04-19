using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLiNhaHang.Migrations
{
    /// <inheritdoc />
    public partial class Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameTable",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "idTable",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_idProduct",
                table: "OrderDetails",
                column: "idProduct");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Products_idProduct",
                table: "OrderDetails",
                column: "idProduct",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Products_idProduct",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_idProduct",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "NameTable",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "idTable",
                table: "Orders");
        }
    }
}
