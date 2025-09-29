using Microsoft.EntityFrameworkCore.Migrations;

namespace StayShare.Migrations
{
    public partial class PropertyModelFixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailCoverPath",
                table: "Properties",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailCoverPath",
                table: "Properties");
        }
    }
}
