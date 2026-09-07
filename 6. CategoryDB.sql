/* ==================================================
   DATABASE: CategoryDB
   SERVICE: CategoryAPI (Thời)
   ================================================== */
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CategoryDB')
BEGIN
    CREATE DATABASE CategoryDB;
END
GO

USE CategoryDB;
GO

DROP TABLE IF EXISTS Categories;

-- Bảng Categories
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

INSERT INTO Categories (CategoryName, Description, IsActive)
VALUES
(N'Music', N'Music events, live shows, concerts, and festivals', 1),
(N'Conference', N'Professional conferences, seminars, and forums', 1),
(N'Workshop', N'Hands-on workshops and practical training sessions', 1),
(N'Exhibition', N'Art, technology, and product exhibitions', 1),
(N'Sports', N'Sports events, tournaments, and friendly matches', 1),
(N'Entertainment', N'Entertainment events, festivals, and shows', 1),
(N'Education', N'Educational and academic events and programs', 1),
(N'Community', N'Community gatherings, networking, and charity events', 1);

GO
