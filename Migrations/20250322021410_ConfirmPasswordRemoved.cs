using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeirdAdminPanel.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmPasswordRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmPassword",
                table: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmPassword",
                table: "User",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
