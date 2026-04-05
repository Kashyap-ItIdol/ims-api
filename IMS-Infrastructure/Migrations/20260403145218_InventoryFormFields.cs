using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InventoryFormFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Inventory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "Inventory",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItemPictureUrl",
                table: "Inventory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Inventory",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Table",
                table: "Inventory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "ItemPictureUrl",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "Table",
                table: "Inventory");
        }
    }
}
