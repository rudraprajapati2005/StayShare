-- Fix occupancies that are marked as active but have future join dates
UPDATE RoomOccupancies 
SET IsActive = 0 
WHERE IsActive = 1 
AND JoinedAt > GETDATE();

-- Also fix any that should be active but aren't (join date in the past)
UPDATE RoomOccupancies 
SET IsActive = 1 
WHERE IsActive = 0 
AND Status = 1 -- OccupancyStatus.Accepted
AND JoinedAt <= GETDATE();
