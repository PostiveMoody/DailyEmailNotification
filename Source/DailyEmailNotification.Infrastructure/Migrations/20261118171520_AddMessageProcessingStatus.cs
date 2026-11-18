using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyEmailNotification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageProcessingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Messages",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Messages");
        }
    }
}
