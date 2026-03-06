using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskDashboard.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicStageToFaculty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "Faculties",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stage",
                table: "Faculties");
        }
    }
}
