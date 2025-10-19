using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTechLab1TaskTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramChatIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<long>(
                name: "TelegramChatId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "AspNetUsers");

            
        }
    }
}
