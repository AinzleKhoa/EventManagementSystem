/* ==================================================
   DATABASE: NotificationDB
   SERVICE: NotificationAPI
   ================================================== */
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'NotificationDB')
BEGIN
    CREATE DATABASE NotificationDB;
END
GO

USE NotificationDB;
GO

DROP TABLE IF EXISTS Notifications;

-- Bảng Notifications
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL, -- Refers to AuthDB.Users
    Title NVARCHAR(200),
    Message NVARCHAR(MAX),
    Type VARCHAR(50), 
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

USE NotificationDB;
GO

INSERT INTO Notifications (UserId, Title, Message, Type, IsRead, CreatedAt)
VALUES
-- Notifications for User 2 (Customer 1)
(2, N'Thanh toán thành công', N'Bạn đã thanh toán thành công đơn hàng vé sự kiện Vietnam International Music Festival 2026.', 'Order', 1, '2026-05-10 14:05'),
(2, N'Thanh toán thành công', N'Bạn đã thanh toán thành công 2 vé cho sự kiện Saigon Night Concert: Indie & Jazz Harmonies.', 'Order', 1, '2026-07-10 15:05'),
(2, N'Nhắc nhở sự kiện', N'Sự kiện Saigon Night Concert: Indie & Jazz Harmonies sẽ diễn ra vào 28/07/2026.', 'Reminder', 0, '2026-07-20 08:00'),

-- Notifications for User 5 (Customer 2)
(5, N'Thanh toán thành công', N'Bạn đã thanh toán thành công vé tham dự Vietnam Mobile & AI Tech Summit 2026.', 'Order', 1, '2026-06-10 09:05'),
(5, N'Thông báo hoàn tiền', N'Đơn hàng #15 của bạn đã được hủy và hoàn tiền thành công về ví điện tử Momo.', 'Order', 1, '2026-07-19 16:30'),
(5, N'Nhắc nhở sự kiện', N'Giải chạy Da Nang International Marathon & Light Fest 2026 sẽ diễn ra vào 08/08/2026.', 'Reminder', 0, '2026-07-20 08:00'),

-- Notifications for User 8 (Customer 3)
(8, N'Thanh toán thành công', N'Bạn đã thanh toán thành công 2 vé tham dự Vietnam - Japan Cultural & Food Festival 2026.', 'Order', 1, '2026-07-15 14:05'),
(8, N'Đơn hàng đang chờ thanh toán', N'Đơn hàng mua vé Da Nang International Marathon 2026 đang chờ thanh toán trước 09:30.', 'Order', 0, '2026-07-20 07:30'),

-- Notifications for User 9 (Customer 4)
(9, N'Thanh toán thành công', N'Bạn đã thanh toán thành công vé tham dự Vietnam Game Expo 2026.', 'Order', 1, '2026-07-18 10:05'),

-- Notifications for User 10 (Customer 5)
(10, N'Thanh toán thành công', N'Bạn đã thanh toán thành công vé VIP cho Vietnam Mobile & AI Tech Summit 2026.', 'Order', 1, '2026-06-15 11:25'),

-- Notifications for User 13 (Customer 6)
(13, N'Thanh toán thành công', N'Bạn đã thanh toán thành công vé eSports VIP Finals Arena cho Vietnam Game Expo 2026.', 'Order', 1, '2026-07-19 14:25'),

-- Notifications for User 14 (Customer 7)
(14, N'Thanh toán thành công', N'Bạn đã thanh toán thành công vé tham dự Saigon Night Concert 2026.', 'Order', 1, '2026-07-14 18:35'),

-- Notifications for User 3 (Organizer 1)
(3, N'Sự kiện được phê duyệt', N'Sự kiện "Vietnam International Music Festival 2026" của bạn đã được Admin phê duyệt.', 'System', 1, '2026-04-02 09:00'),
(3, N'Đơn hàng mới', N'Khách hàng Trần Thị Customer vừa mua 2 vé xem Saigon Night Concert.', 'System', 0, '2026-07-10 15:01'),
(3, N'Rút tiền thành công', N'Yêu cầu rút 5,000,000 VNĐ cho sự kiện Music Festival đã được Admin phê duyệt chuyển khoản.', 'Withdrawal', 1, '2026-06-18 14:05'),

-- Notifications for User 6 (Organizer 2)
(6, N'Sự kiện bị từ chối', N'Sự kiện "Off-Road Motorcross Extreme Championship" bị từ chối do chưa đảm bảo an toàn PCCC.', 'System', 1, '2026-07-19 14:30'),
(6, N'Yêu cầu rút tiền bị từ chối', N'Yêu cầu rút 3,000,000 VNĐ bị từ chối. Lý do: Doanh thu chưa đối soát xong với VNPay.', 'Withdrawal', 0, '2026-07-11 10:05'),

-- Notifications for User 11 (Organizer 3)
(11, N'Sự kiện được phê duyệt', N'Sự kiện "Digital Era: AI Agents & Microservices Workshop" của bạn đã được Admin phê duyệt.', 'System', 1, '2026-07-15 14:30'),

-- Notifications for User 1 (Admin)
(1, N'Sự kiện chờ phê duyệt', N'Sự kiện mới "Global Study Abroad & Tech Career Expo 2026" vừa gửi yêu cầu duyệt.', 'System', 0, '2026-07-18 10:05'),
(1, N'Yêu cầu rút tiền mới', N'Ban tổ chức Trần Thị Organizer vừa gửi yêu cầu rút 2,000,000 VNĐ từ sự kiện Đà Lạt Acoustic.', 'Withdrawal', 0, '2026-07-15 16:05');

GO