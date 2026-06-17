using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELProject.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueStudentIdandCourseIdInOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_StudentId_CourseId",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StudentId",
                table: "Orders",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_StudentId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "Questions");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StudentId_CourseId",
                table: "Orders",
                columns: new[] { "StudentId", "CourseId" },
                unique: true);
        }
    }
}
