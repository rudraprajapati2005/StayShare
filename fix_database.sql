-- Add missing columns to Properties table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Properties]') AND name = 'ThumbnailCoverPath')
BEGIN
    ALTER TABLE [Properties] ADD [ThumbnailCoverPath] nvarchar(max) NULL;
    PRINT 'Added ThumbnailCoverPath column to Properties table';
END
ELSE
BEGIN
    PRINT 'ThumbnailCoverPath column already exists in Properties table';
END

-- Add Amenities column to Properties table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Properties]') AND name = 'Amenities')
BEGIN
    ALTER TABLE [Properties] ADD [Amenities] nvarchar(max) NULL;
    PRINT 'Added Amenities column to Properties table';
END
ELSE
BEGIN
    PRINT 'Amenities column already exists in Properties table';
END

-- Add missing columns to Rooms table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Rooms]') AND name = 'RentPerMonth')
BEGIN
    ALTER TABLE [Rooms] ADD [RentPerMonth] decimal(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added RentPerMonth column to Rooms table';
END
ELSE
BEGIN
    PRINT 'RentPerMonth column already exists in Rooms table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Rooms]') AND name = 'RoomType')
BEGIN
    ALTER TABLE [Rooms] ADD [RoomType] nvarchar(max) NULL;
    PRINT 'Added RoomType column to Rooms table';
END
ELSE
BEGIN
    PRINT 'RoomType column already exists in Rooms table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Rooms]') AND name = 'Amenities')
BEGIN
    ALTER TABLE [Rooms] ADD [Amenities] nvarchar(max) NULL;
    PRINT 'Added Amenities column to Rooms table';
END
ELSE
BEGIN
    PRINT 'Amenities column already exists in Rooms table';
END

PRINT 'Database schema update completed successfully!';
