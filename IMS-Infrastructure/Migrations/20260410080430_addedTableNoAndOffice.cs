using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedTableNoAndOffice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TableNo",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssignedTo",
                table: "Assets",
                column: "AssignedTo");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Users_AssignedTo",
                table: "Assets",
                column: "AssignedTo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Users_AssignedTo",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_AssignedTo",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TableNo",
                table: "Users");
        }
    }
}
