using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLiNhaHang.Migrations
{
    /// <inheritdoc />
    public partial class al1l1l1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Jobs_idJob",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "idJob",
                table: "Users",
                newName: "IdJob");

            migrationBuilder.RenameIndex(
                name: "IX_Users_idJob",
                table: "Users",
                newName: "IX_Users_IdJob");

            migrationBuilder.CreateTable(
                name: "Timekeepings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TypeJob = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timekeepings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timekeepings_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Timekeepings_IdUser",
                table: "Timekeepings",
                column: "IdUser");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Jobs_IdJob",
                table: "Users",
                column: "IdJob",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Jobs_IdJob",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Timekeepings");

            migrationBuilder.RenameColumn(
                name: "IdJob",
                table: "Users",
                newName: "idJob");

            migrationBuilder.RenameIndex(
                name: "IX_Users_IdJob",
                table: "Users",
                newName: "IX_Users_idJob");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Jobs_idJob",
                table: "Users",
                column: "idJob",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
