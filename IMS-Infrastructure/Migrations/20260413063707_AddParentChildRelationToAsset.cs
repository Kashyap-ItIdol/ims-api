using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParentChildRelationToAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentAssetId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ParentAssetId",
                table: "Assets",
                column: "ParentAssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Assets_ParentAssetId",
                table: "Assets",
                column: "ParentAssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Assets_ParentAssetId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_ParentAssetId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ParentAssetId",
                table: "Assets");
        }
    }
}
