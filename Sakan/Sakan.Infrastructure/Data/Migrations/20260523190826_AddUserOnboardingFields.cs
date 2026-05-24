using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sakan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OnboardingCompleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OnboardingIntent",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnboardingCompleted",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OnboardingIntent",
                table: "AspNetUsers");
        }
    }
}
