using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StayShare.Migrations
{
    public partial class UpdateParentLinkModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LinkedAt",
                table: "ParentLinks",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ParentLinks",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelationshipType",
                table: "ParentLinks",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAt",
                table: "ParentLinks",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "ParentLinks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseMessage",
                table: "ParentLinks",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ParentLinks",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "ParentLinks");

            migrationBuilder.DropColumn(
                name: "RelationshipType",
                table: "ParentLinks");

            migrationBuilder.DropColumn(
                name: "RequestedAt",
                table: "ParentLinks");

            migrationBuilder.DropColumn(
                name: "RespondedAt",
                table: "ParentLinks");

            migrationBuilder.DropColumn(
                name: "ResponseMessage",
                table: "ParentLinks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ParentLinks");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LinkedAt",
                table: "ParentLinks",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
