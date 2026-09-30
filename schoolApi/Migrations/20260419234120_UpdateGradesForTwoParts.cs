using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace schoolApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGradesForTwoParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExamType",
                table: "Grades",
                newName: "Part");

            migrationBuilder.AddColumn<string>(
                name: "GradeType",
                table: "Grades",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradeType",
                table: "Grades");

            migrationBuilder.RenameColumn(
                name: "Part",
                table: "Grades",
                newName: "ExamType");
        }
    }
}
