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


-- Ensure ParentLinks table has new columns expected by the model
IF OBJECT_ID(N'[ParentLinks]', N'U') IS NOT NULL
BEGIN
    -- Make LinkedAt nullable if not already
    IF EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[ParentLinks]') AND name = 'LinkedAt' AND is_nullable = 0
    )
    BEGIN
        ALTER TABLE [ParentLinks] ALTER COLUMN [LinkedAt] datetime2 NULL;
        PRINT 'Altered ParentLinks.LinkedAt to be NULL';
    END

    -- Add Message
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ParentLinks]') AND name = 'Message')
    BEGIN
        ALTER TABLE [ParentLinks] ADD [Message] nvarchar(500) NULL;
        PRINT 'Added ParentLinks.Message';
    END

    -- Add RelationshipType
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ParentLinks]') AND name = 'RelationshipType')
    BEGIN
        ALTER TABLE [ParentLinks] ADD [RelationshipType] int NOT NULL DEFAULT 0;
        PRINT 'Added ParentLinks.RelationshipType';
    END

    -- Add RequestedAt
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ParentLinks]') AND name = 'RequestedAt')
    BEGIN
        ALTER TABLE [ParentLinks] ADD [RequestedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
        PRINT 'Added ParentLinks.RequestedAt';
    END

    -- Add RespondedAt
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ParentLinks]') AND name = 'RespondedAt')
    BEGIN
        ALTER TABLE [ParentLinks] ADD [RespondedAt] datetime2 NULL;
        PRINT 'Added ParentLinks.RespondedAt';
    END

    -- Add ResponseMessage
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ParentLinks]') AND name = 'ResponseMessage')
    BEGIN
        ALTER TABLE [ParentLinks] ADD [ResponseMessage] nvarchar(500) NULL;
        PRINT 'Added ParentLinks.ResponseMessage';
    END

    -- Add Status
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ParentLinks]') AND name = 'Status')
    BEGIN
        ALTER TABLE [ParentLinks] ADD [Status] int NOT NULL DEFAULT 0;
        PRINT 'Added ParentLinks.Status';
    END
END
ELSE
BEGIN
    PRINT 'ParentLinks table not found; skipping ParentLinks fixes.';
END