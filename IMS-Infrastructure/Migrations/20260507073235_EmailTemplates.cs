using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EmailTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpires",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubCategoryId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TicketComments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "TicketComments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TicketComments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "TicketComments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TicketComments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketCommentLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCommentLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketCommentLikes_TicketComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "TicketComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketCommentLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketCommentReactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCommentReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketCommentReactions_TicketComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "TicketComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketCommentReactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CategoryId",
                table: "Tickets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SubCategoryId",
                table: "Tickets",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_IsDeleted",
                table: "TicketComments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_ParentCommentId",
                table: "TicketComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentLikes_CommentId",
                table: "TicketCommentLikes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentLikes_CommentId_UserId_IsDeleted",
                table: "TicketCommentLikes",
                columns: new[] { "CommentId", "UserId", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentLikes_IsDeleted",
                table: "TicketCommentLikes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentLikes_UserId",
                table: "TicketCommentLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentReactions_CommentId",
                table: "TicketCommentReactions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentReactions_CommentId_UserId_IsDeleted",
                table: "TicketCommentReactions",
                columns: new[] { "CommentId", "UserId", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentReactions_IsDeleted",
                table: "TicketCommentReactions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCommentReactions_UserId",
                table: "TicketCommentReactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_TicketComments_ParentCommentId",
                table: "TicketComments",
                column: "ParentCommentId",
                principalTable: "TicketComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Categories_CategoryId",
                table: "Tickets",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_SubCategories_SubCategoryId",
                table: "Tickets",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_TicketComments_ParentCommentId",
                table: "TicketComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Categories_CategoryId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_SubCategories_SubCategoryId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "TicketCommentLikes");

            migrationBuilder.DropTable(
                name: "TicketCommentReactions");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CategoryId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SubCategoryId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_TicketComments_IsDeleted",
                table: "TicketComments");

            migrationBuilder.DropIndex(
                name: "IX_TicketComments_ParentCommentId",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TicketComments");

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpires",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordResetToken", "ResetTokenExpires" },
                values: new object[] { null, null });
        }
    }
}
