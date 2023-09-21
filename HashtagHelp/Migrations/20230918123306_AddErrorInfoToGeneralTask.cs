using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HashtagHelp.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorInfoToGeneralTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorInfo",
                table: "GeneralTasks",
                type: "LONGTEXT",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorInfo",
                table: "GeneralTasks");
        }
    }
}
