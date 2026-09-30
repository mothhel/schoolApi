using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace schoolApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Lessons",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Lessons");
        }
    }
}
