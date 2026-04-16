using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addNotesFieldInAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Assets");
        }
    }
}
