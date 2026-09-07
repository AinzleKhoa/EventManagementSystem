/* ==================================================
   DATABASE: SocialDB
   SERVICE: SocialAPI
   ================================================== */
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SocialDB')
BEGIN
    CREATE DATABASE SocialDB;
END
GO

USE SocialDB;
GO

DROP TABLE IF EXISTS Reviews;
DROP TABLE IF EXISTS Wishlists;

-- Bảng Wishlists
CREATE TABLE Wishlists (
    WishlistId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,  -- Refers to AuthDB.Users
    EventId INT NOT NULL, -- Refers to EventsDB.Events
    AddedAt DATETIME DEFAULT GETDATE(),
    UNIQUE(UserId, EventId)
);

-- Bảng Reviews
CREATE TABLE Reviews (
    ReviewId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,  -- Refers to AuthDB.Users
    EventId INT NOT NULL, -- Refers to EventsDB.Events
    Rating INT CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(1000),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UNIQUE(UserId, EventId)
);

USE SocialDB;
GO

-- 1. Seed Wishlists
INSERT INTO Wishlists (UserId, EventId, AddedAt)
VALUES 
(2, 5, '2026-07-01 10:00'),
(2, 6, '2026-07-02 11:30'),
(2, 11, '2026-07-05 15:20'),
(2, 17, '2026-07-10 09:00'),

(5, 7, '2026-07-10 09:00'),
(5, 9, '2026-07-12 14:00'),
(5, 18, '2026-07-15 11:00'),

(8, 7, '2026-07-15 16:45'),
(8, 12, '2026-07-18 18:00'),
(8, 19, '2026-07-19 14:00'),

(9, 8, '2026-07-16 10:00'),
(9, 16, '2026-07-18 15:30'),
(9, 20, '2026-07-19 09:00'),

(10, 10, '2026-07-17 08:30'),
(10, 15, '2026-07-19 11:00'),

(13, 11, '2026-07-18 16:00'),
(13, 17, '2026-07-19 13:00'),

(14, 12, '2026-07-19 14:30'),
(14, 19, '2026-07-20 09:00');

-- 2. Seed Reviews (For Past Events 1, 2, 3, 4)
INSERT INTO Reviews (UserId, EventId, Rating, Comment, CreatedAt)
VALUES 
-- Reviews for Event 1 (Music Fest)
(2, 1, 5, N'Sự kiện quá tuyệt vời! Âm thanh, ánh sáng sống động, dàn nghệ sĩ trình diễn cực sung.', '2026-06-16 10:00'),
(5, 1, 4, N'Không khí rất cuồng nhiệt. Tuy nhiên lối vào bãi giữ xe hơi đông lúc tan tầm.', '2026-06-16 11:30'),
(9, 1, 5, N'Indie band trình diễn đỉnh cao. Đáng từng đồng vé bỏ ra!', '2026-06-16 14:00'),

-- Reviews for Event 2 (Tech Summit)
(5, 2, 5, N'Nội dung hội thảo rất thực tế, các diễn giả chia sẻ chuyên sâu về AI và Cloud.', '2026-07-06 09:15'),
(10, 2, 4, N'Tài liệu trình bày phong phú, mạng wifi hội trường cần nâng cấp thêm.', '2026-07-06 14:20'),

-- Reviews for Events 3 and 4
-- User 2 and User 5 are intentionally not seeded here, so customer@auth.com
-- and customer2@auth.com can demo creating reviews from the UI.
(13, 3, 5, N'Thời tiết Đà Lạt se lạnh nghe nhạc acoustic rất chill!', '2026-05-11 10:15'),
(8, 3, 5, N'Không gian Đà Lạt rất đẹp, âm nhạc nhẹ nhàng và sân khấu được chuẩn bị chỉn chu.', '2026-05-11 09:00'),
(8, 4, 4, N'Không gian lễ hội trang trí đẹp, các món ăn vùng miền rất chuẩn vị!', '2026-06-23 14:30'),
(13, 4, 4, N'Lễ hội ẩm thực có nhiều món ngon, khu vực check-in và gian hàng khá nhộn nhịp.', '2026-06-23 10:30');

GO
