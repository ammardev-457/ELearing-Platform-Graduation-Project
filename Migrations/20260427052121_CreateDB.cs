using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELProject.Migrations
{
    /// <inheritdoc />
    public partial class CreateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsFreePreview",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "Lessons");

            migrationBuilder.RenameColumn(
                name: "VideoUrl",
                table: "Lessons",
                newName: "FileUrl");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Courses",
                newName: "ShortDescription");

            migrationBuilder.RenameColumn(
                name: "ProfileImage",
                table: "AspNetUsers",
                newName: "PathOfProfileImageInDb");

            migrationBuilder.AddColumn<string>(
                name: "LongDescription",
                table: "Courses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Thumbnail",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => new { x.ApplicationUserId, x.Id });
                    table.ForeignKey(
                        name: "FK_RefreshToken_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "LongDescription",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Thumbnail",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "FileUrl",
                table: "Lessons",
                newName: "VideoUrl");

            migrationBuilder.RenameColumn(
                name: "ShortDescription",
                table: "Courses",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "PathOfProfileImageInDb",
                table: "AspNetUsers",
                newName: "ProfileImage");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Lessons",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFreePreview",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProcessingStatus",
                table: "Lessons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
