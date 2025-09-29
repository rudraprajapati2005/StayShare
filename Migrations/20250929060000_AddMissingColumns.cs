using Microsoft.EntityFrameworkCore.Migrations;

namespace StayShare.Migrations
{
    public partial class AddMissingColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ThumbnailCoverPath column to Properties table if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Properties]') AND name = 'ThumbnailCoverPath')
                BEGIN
                    ALTER TABLE [Properties] ADD [ThumbnailCoverPath] nvarchar(max) NULL;
                END
            ");

            // Add Amenities column to Properties table if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Properties]') AND name = 'Amenities')
                BEGIN
                    ALTER TABLE [Properties] ADD [Amenities] nvarchar(max) NULL;
                END
            ");

            // Add missing columns to Rooms table if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Rooms]') AND name = 'RentPerMonth')
                BEGIN
                    ALTER TABLE [Rooms] ADD [RentPerMonth] decimal(18,2) NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Rooms]') AND name = 'RoomType')
                BEGIN
                    ALTER TABLE [Rooms] ADD [RoomType] nvarchar(max) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Rooms]') AND name = 'Amenities')
                BEGIN
                    ALTER TABLE [Rooms] ADD [Amenities] nvarchar(max) NULL;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the columns if they exist
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Properties]') AND name = 'ThumbnailCoverPath')
                BEGIN
                    ALTER TABLE [Properties] DROP COLUMN [ThumbnailCoverPath];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Properties]') AND name = 'Amenities')
                BEGIN
                    ALTER TABLE [Properties] DROP COLUMN [Amenities];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Rooms]') AND name = 'RentPerMonth')
                BEGIN
                    ALTER TABLE [Rooms] DROP COLUMN [RentPerMonth];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Rooms]') AND name = 'RoomType')
                BEGIN
                    ALTER TABLE [Rooms] DROP COLUMN [RoomType];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Rooms]') AND name = 'Amenities')
                BEGIN
                    ALTER TABLE [Rooms] DROP COLUMN [Amenities];
                END
            ");
        }
    }
}
