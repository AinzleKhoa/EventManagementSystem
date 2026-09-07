/* ==================================================
   DATABASE: ChatDB
   SERVICE: ChatAPI
   ================================================== */
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ChatDB')
BEGIN
    CREATE DATABASE ChatDB;
END
GO

USE ChatDB;
GO

DROP TABLE IF EXISTS Messages;
DROP TABLE IF EXISTS ConversationParticipants;
DROP TABLE IF EXISTS Conversations;

-- Bảng Conversations
CREATE TABLE Conversations (
    ConversationId INT IDENTITY(1,1) PRIMARY KEY,
    Type VARCHAR(20) NOT NULL CHECK (Type IN ('Private', 'Group')), 
    Title NVARCHAR(200) NULL, 
    ImageUrl NVARCHAR(500) NULL,
    EventId INT NULL, -- Refers to EventsDB.Events
    LastMessageAt DATETIME DEFAULT GETDATE(),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Bảng ConversationParticipants
CREATE TABLE ConversationParticipants (
    ConversationId INT NOT NULL,
    UserId INT NOT NULL, -- Refers to AuthDB.Users
    Role VARCHAR(20) DEFAULT 'Member', 
    JoinedAt DATETIME DEFAULT GETDATE(),
    IsMuted BIT DEFAULT 0, 
    LastReadMessageId INT NULL, 
    PRIMARY KEY (ConversationId, UserId),
    FOREIGN KEY (ConversationId) REFERENCES Conversations(ConversationId)
);

-- Bảng Messages
CREATE TABLE Messages (
    MessageId INT IDENTITY(1,1) PRIMARY KEY,
    ConversationId INT NOT NULL,
    SenderId INT NOT NULL, -- Refers to AuthDB.Users
    SenderName NVARCHAR(255) NULL,
    Content NVARCHAR(MAX), 
    MessageType VARCHAR(20) DEFAULT 'Text',
    AttachmentUrl NVARCHAR(500) NULL, 
    SentAt DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0, 
    FOREIGN KEY (ConversationId) REFERENCES Conversations(ConversationId)
);

USE ChatDB;
GO

-- 1. Create Group Conversations for all 20 Approved Events (ConversationId 1..20)
INSERT INTO Conversations (Type, Title, ImageUrl, EventId, LastMessageAt, CreatedAt)
VALUES 
-- Conv 1: Event 1 (Music Fest)
('Group', N'Hỗ trợ: Vietnam International Music Festival 2026', 'https://images.unsplash.com/photo-1470225620780-dba8ba36b745?auto=format&fit=crop&w=300&q=80', 1, '2026-06-15 17:30', '2026-05-01 10:00'),

-- Conv 2: Event 2 (Tech Summit)
('Group', N'Hội thảo: Vietnam Mobile & AI Tech Summit 2026', 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?auto=format&fit=crop&w=300&q=80', 2, '2026-07-05 16:30', '2026-06-01 09:00'),

-- Conv 3: Event 3 (Da Lat Acoustic)
('Group', N'Đêm nhạc: Da Lat Acoustic & Misty Night 2026', 'https://images.unsplash.com/photo-1465847899084-d164df4dedc6?auto=format&fit=crop&w=300&q=80', 3, '2026-05-10 17:45', '2026-04-10 11:00'),

-- Conv 4: Event 4 (Food Fest)
('Group', N'Ẩm thực: Saigon Street Food Culture Festival 2026', 'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=300&q=80', 4, '2026-06-20 09:30', '2026-05-20 10:00'),

-- Conv 5: Event 5 (Saigon Night Concert)
('Group', N'Hỏi đáp: Saigon Night Concert 2026', 'https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?auto=format&fit=crop&w=300&q=80', 5, '2026-07-20 08:15', '2026-07-01 09:00'),

-- Conv 6: Event 6 (Japan Fest)
('Group', N'Lễ hội: Vietnam - Japan Cultural & Food Festival', 'https://images.unsplash.com/photo-1528164344705-47542687990d?auto=format&fit=crop&w=300&q=80', 6, '2026-07-19 18:20', '2026-07-10 09:00'),

-- Conv 7: Event 7 (Da Nang Marathon)
('Group', N'Giải chạy: Da Nang International Marathon 2026', 'https://images.unsplash.com/photo-1452626038306-9aae5e071dd3?auto=format&fit=crop&w=300&q=80', 7, '2026-07-19 14:00', '2026-07-15 10:00'),

-- Conv 8: Event 8 (Startup Pitching)
('Group', N'Diễn đàn: Startup & Innovation Venture Pitching 2026', 'https://images.unsplash.com/photo-1475721027785-f74eccf877e2?auto=format&fit=crop&w=300&q=80', 8, '2026-07-18 11:30', '2026-07-18 10:00'),

-- Conv 9: Event 9 (Game Expo)
('Group', N'Gaming: Vietnam Game Expo & Esports 2026', 'https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=300&q=80', 9, '2026-07-19 16:45', '2026-07-15 15:00'),

-- Conv 10: Event 10 (Hanoi Run)
('Group', N'Marathon: Run For Green Hanoi 2026', 'https://images.unsplash.com/photo-1530549387789-4c1017266635?auto=format&fit=crop&w=300&q=80', 10, '2026-07-19 12:10', '2026-07-19 09:00'),

-- Conv 11: Event 11 (Saigon Fashion Week)
('Group', N'Thời trang: Saigon Fashion Week 2026', 'https://images.unsplash.com/photo-1509631179647-0177331693ae?auto=format&fit=crop&w=300&q=80', 11, '2026-07-20 09:15', '2026-07-19 16:00'),

-- Conv 12: Event 12 (Nha Trang EDM)
('Group', N'Âm nhạc: Nha Trang Sea Symphony & EDM Party', 'https://images.unsplash.com/photo-1516450360452-9312f5e86fc7?auto=format&fit=crop&w=300&q=80', 12, '2026-07-20 10:30', '2026-07-20 09:30'),

-- Conv 13: Event 13 (Art Exhibition)
('Group', N'Triển lãm: Contemporary Art "Light & Identity"', 'https://images.unsplash.com/photo-1536924940846-227afb31e2a5?auto=format&fit=crop&w=300&q=80', 13, '2026-07-20 11:15', '2026-07-20 11:00'),

-- Conv 14: Event 14 (Photography Workshop)
('Group', N'Workshop: Mastering Portrait Photography', 'https://images.unsplash.com/photo-1452587925148-ce544e77e70d?auto=format&fit=crop&w=300&q=80', 14, '2026-07-20 12:20', '2026-07-20 12:00'),

-- Conv 15: Event 15 (AI Workshop)
('Group', N'Workshop: AI Agents & Microservices 2026', 'https://images.unsplash.com/photo-1531403009284-440f080d1e12?auto=format&fit=crop&w=300&q=80', 15, '2026-07-20 13:40', '2026-07-20 13:00'),

-- Conv 16: Event 16 (E-Commerce Forum)
('Group', N'Diễn đàn: Vietnam E-Commerce & Retail 2026', 'https://images.unsplash.com/photo-1556761175-5973dc0f32e7?auto=format&fit=crop&w=300&q=80', 16, '2026-07-20 14:15', '2026-07-20 14:00'),

-- Conv 17: Event 17 (Autumn Symphony)
('Group', N'Hòa nhạc: Autumn Symphony Classical 2026', 'https://images.unsplash.com/photo-1514525253161-7a46d19cd819?auto=format&fit=crop&w=300&q=80', 17, '2026-07-20 15:30', '2026-07-20 14:30'),

-- Conv 18: Event 18 (Cloud Native Workshop)
('Group', N'Workshop: System Architecture & Cloud Native', 'https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?auto=format&fit=crop&w=300&q=80', 18, '2026-07-20 15:45', '2026-07-20 15:30'),

-- Conv 19: Event 19 (Coffee Expo)
('Group', N'Lễ hội: Saigon Coffee & Bakery Cultural Expo', 'https://images.unsplash.com/photo-1447933601403-0c6688de566e?auto=format&fit=crop&w=300&q=80', 19, '2026-07-20 16:30', '2026-07-20 16:00'),

-- Conv 20: Event 20 (Badminton Open)
('Group', N'Giải đấu: National Badminton Open 2026', 'https://images.unsplash.com/photo-1626224583764-f87db24ac4ea?auto=format&fit=crop&w=300&q=80', 20, '2026-07-20 16:45', '2026-07-20 16:30'),

-- Conv 21: Private Chat between Customer 1 (User 2) & Organizer 1 (User 3)
('Private', NULL, NULL, NULL, '2026-07-18 14:30', '2026-07-18 14:00'),

-- Conv 22: Private Chat between Customer 2 (User 5) & Admin (User 1)
('Private', NULL, NULL, NULL, '2026-07-19 16:45', '2026-07-19 16:00'),

-- Conv 23: Private Chat between Customer 4 (User 9) & Organizer 3 (User 11)
('Private', NULL, NULL, NULL, '2026-07-20 17:00', '2026-07-20 16:30');

-- 2. Add Participants (Organizer = Admin, Ticket Buyers = Member)
INSERT INTO ConversationParticipants (ConversationId, UserId, Role, JoinedAt)
VALUES 
-- Conversation 1 (Event 1): Organizer 3, Customers 2, 5, 9
(1, 3, 'Admin', '2026-05-01 10:00'),
(1, 2, 'Member', '2026-05-10 14:00'),
(1, 5, 'Member', '2026-05-12 16:30'),
(1, 9, 'Member', '2026-05-14 09:15'),

-- Conversation 2 (Event 2): Organizer 3, Customers 5, 10
(2, 3, 'Admin', '2026-06-01 09:00'),
(2, 5, 'Member', '2026-06-10 09:00'),
(2, 10, 'Member', '2026-06-15 11:20'),

-- Conversation 3 (Event 3): Organizer 3, Customers 2, 13
(3, 3, 'Admin', '2026-04-10 11:00'),
(3, 2, 'Member', '2026-04-15 11:00'),
(3, 13, 'Member', '2026-04-18 14:00'),

-- Conversation 4 (Event 4): Organizer 3, Customers 5, 8
(4, 3, 'Admin', '2026-05-20 10:00'),
(4, 5, 'Member', '2026-06-01 10:00'),
(4, 8, 'Member', '2026-06-05 16:20'),

-- Conversation 5 (Event 5): Organizer 3, Customers 2, 5, 14
(5, 3, 'Admin', '2026-07-01 09:00'),
(5, 2, 'Member', '2026-07-10 15:00'),
(5, 5, 'Member', '2026-07-12 10:00'),
(5, 14, 'Member', '2026-07-14 18:30'),

-- Conversation 6 (Event 6): Organizer 3, Customers 8, 9
(6, 3, 'Admin', '2026-07-10 09:00'),
(6, 8, 'Member', '2026-07-15 14:00'),
(6, 9, 'Member', '2026-07-16 10:15'),

-- Conversation 7 (Event 7): Organizer 6, Customers 5, 10
(7, 6, 'Admin', '2026-07-15 10:00'),
(7, 5, 'Member', '2026-07-16 11:30'),
(7, 10, 'Member', '2026-07-17 15:00'),

-- Conversation 8 (Event 8): Organizer 6, Customer 9
(8, 6, 'Admin', '2026-07-18 10:00'),
(8, 9, 'Member', '2026-07-18 10:00'),

-- Conversation 9 (Event 9): Organizer 6, Customers 8, 13
(9, 6, 'Admin', '2026-07-15 15:00'),
(9, 8, 'Member', '2026-07-18 16:00'),
(9, 13, 'Member', '2026-07-19 14:20'),

-- Conversation 10 (Event 10): Organizer 6, Customers 10, 14
(10, 6, 'Admin', '2026-07-19 09:00'),
(10, 10, 'Member', '2026-07-19 09:00'),
(10, 14, 'Member', '2026-07-19 11:30'),

-- Conversation 11 (Event 11): Organizer 6, Customers 2, 13
(11, 6, 'Admin', '2026-07-19 16:00'),
(11, 2, 'Member', '2026-07-19 16:00'),
(11, 13, 'Member', '2026-07-20 08:00'),

-- Conversation 12 (Event 12): Organizer 3, Customers 9, 14
(12, 3, 'Admin', '2026-07-20 09:30'),
(12, 9, 'Member', '2026-07-20 09:30'),
(12, 14, 'Member', '2026-07-20 10:00'),

-- Conversation 13 (Event 13): Organizer 3, Customer 8
(13, 3, 'Admin', '2026-07-20 11:00'),
(13, 8, 'Member', '2026-07-20 11:00'),

-- Conversation 14 (Event 14): Organizer 3, Customer 5
(14, 3, 'Admin', '2026-07-20 12:00'),
(14, 5, 'Member', '2026-07-20 12:00'),

-- Conversation 15 (Event 15): Organizer 11, Customer 10
(15, 11, 'Admin', '2026-07-20 13:00'),
(15, 10, 'Member', '2026-07-20 13:00'),

-- Conversation 16 (Event 16): Organizer 11, Customer 9
(16, 11, 'Admin', '2026-07-20 14:00'),
(16, 9, 'Member', '2026-07-20 14:00'),

-- Conversation 17 (Event 17): Organizer 3, Customers 2, 13
(17, 3, 'Admin', '2026-07-20 14:30'),
(17, 2, 'Member', '2026-07-20 14:30'),
(17, 13, 'Member', '2026-07-20 15:00'),

-- Conversation 18 (Event 18): Organizer 11, Customer 5
(18, 11, 'Admin', '2026-07-20 15:30'),
(18, 5, 'Member', '2026-07-20 15:30'),

-- Conversation 19 (Event 19): Organizer 6, Customers 8, 14
(19, 6, 'Admin', '2026-07-20 16:00'),
(19, 8, 'Member', '2026-07-20 16:00'),
(19, 14, 'Member', '2026-07-20 16:15'),

-- Conversation 20 (Event 20): Organizer 11, Customer 9
(20, 11, 'Admin', '2026-07-20 16:30'),
(20, 9, 'Member', '2026-07-20 16:30'),

-- Conversation 21 (Private): Customer 1 (User 2) & Organizer 1 (User 3)
(21, 2, 'Member', '2026-07-18 14:00'),
(21, 3, 'Member', '2026-07-18 14:00'),

-- Conversation 22 (Private): Customer 2 (User 5) & Admin (User 1)
(22, 5, 'Member', '2026-07-19 16:00'),
(22, 1, 'Admin', '2026-07-19 16:00'),

-- Conversation 23 (Private): Customer 4 (User 9) & Organizer 3 (User 11)
(23, 9, 'Member', '2026-07-20 16:30'),
(23, 11, 'Member', '2026-07-20 16:30');

-- 3. Add Messages for Event & Private Conversations
INSERT INTO Messages (ConversationId, SenderId, SenderName, Content, MessageType, AttachmentUrl, SentAt)
VALUES 
-- Conversation 1 Messages
(1, 2, N'Trần Thị Customer', N'Cho mình hỏi sự kiện có bãi giữ xe ô tô không ạ?', 'Text', NULL, '2026-06-14 15:00'),
(1, 3, N'Trần Thị Organizer', N'Chào bạn, bãi giữ xe ô tô nằm ngay tại tuyến đường Hàm Nghi gần sát phố đi bộ nhé!', 'Text', NULL, '2026-06-14 15:15'),
(1, 5, N'Hoàng Minh Tuấn', N'Mấy giờ thì bắt đầu mở cổng soát vé vậy BTC?', 'Text', NULL, '2026-06-15 17:00'),
(1, 3, N'Trần Thị Organizer', N'Cổng soát vé mở lúc 17:30 nhé mọi người, hãy chuẩn bị sẵn mã QR trên ứng dụng!', 'Text', NULL, '2026-06-15 17:30'),
(1, 9, N'Đặng Hoàng Nam', N'Chương trình đêm nay thực sự rất hoành tráng! Cảm ơn BTC!', 'Text', NULL, '2026-06-15 22:30'),

-- Conversation 2 Messages
(2, 5, N'Hoàng Minh Tuấn', N'Diễn giả phiên AI có chia sẻ slide tài liệu sau hội thảo không BTC?', 'Text', NULL, '2026-07-04 16:00'),
(2, 3, N'Trần Thị Organizer', N'Có nhé bạn, toàn bộ slide sẽ được gửi qua email đăng ký của các đại biểu.', 'Text', NULL, '2026-07-04 16:30'),
(2, 10, N'Phạm Minh Anh', N'Khu vực check-in VIP ở cửa số mấy vậy ạ?', 'Text', NULL, '2026-07-05 08:00'),

-- Conversation 3 Messages
(3, 2, N'Trần Thị Customer', N'Đêm nhạc acoustic ở Đà Lạt có trang bị lều sưởi ấm không ạ?', 'Text', NULL, '2026-05-09 20:00'),
(3, 3, N'Trần Thị Organizer', N'BTC có bố trí khu vực quầy trà nóng và lò sưởi ngoài trời cho khán giả nhé.', 'Text', NULL, '2026-05-09 20:30'),

-- Conversation 4 Messages
(4, 8, N'Lê Thị Ngọc Bích', N'Các gian hàng ăn uống có nhận thanh toán chuyển khoản qua mã QR không BTC?', 'Text', NULL, '2026-06-19 14:00'),
(4, 3, N'Trần Thị Organizer', N'100% gian hàng đều chấp nhận mã QR thanh toán nhé bạn!', 'Text', NULL, '2026-06-19 14:20'),

-- Conversation 5 Messages
(5, 2, N'Trần Thị Customer', N'Đêm nhạc có quy định về trang phục (dresscode) không ạ?', 'Text', NULL, '2026-07-19 20:00'),
(5, 3, N'Trần Thị Organizer', N'Khuyến khích trang phục lịch sự hoặc Vintage Jazz nhé bạn!', 'Text', NULL, '2026-07-20 08:15'),
(5, 14, N'Bùi Thu Thảo', N'Cho mình hỏi khán giả có được mang camera chuyên nghiệp vào chụp không?', 'Text', NULL, '2026-07-20 09:00'),

-- Conversation 6 Messages
(6, 8, N'Lê Thị Ngọc Bích', N'Lễ hội ẩm thực Nhật Bản có cuộc thi Cosplay vào khung giờ nào vậy ạ?', 'Text', NULL, '2026-07-18 10:00'),
(6, 3, N'Trần Thị Organizer', N'Vòng chung kết Cosplay sẽ bắt đầu lúc 15:00 ngày 30/07 nhé bạn!', 'Text', NULL, '2026-07-18 10:30'),

-- Conversation 7 Messages
(7, 5, N'Hoàng Minh Tuấn', N'Khu vực nhận Racekit giải chạy Đà Nẵng ở đâu vậy BTC?', 'Text', NULL, '2026-07-18 15:00'),
(7, 6, N'Sài Gòn Event Hub', N'Chào bạn, điểm phát Racekit đặt tại Công viên Biển Đông từ 8h00 - 20h00 ngày 07/08 nhé!', 'Text', NULL, '2026-07-18 15:30'),

-- Conversation 8 Messages
(8, 9, N'Đặng Hoàng Nam', N'Sự kiện Pitching Day có dành thời gian 1-on-1 networking với Quỹ đầu tư không ạ?', 'Text', NULL, '2026-07-18 11:00'),
(8, 6, N'Sài Gòn Event Hub', N'Có phiên Matchmaking riêng vào cuối chương trình nhé bạn!', 'Text', NULL, '2026-07-18 11:30'),

-- Conversation 9 Messages
(9, 13, N'Nguyễn Vũ Linh', N'Vé VIP Finals Arena có kèm theo quà tặng Merchandise độc quyền không BTC?', 'Text', NULL, '2026-07-19 15:00'),
(9, 6, N'Sài Gòn Event Hub', N'Có nhé! Khán giả VIP sẽ nhận được Áo thun Game Expo + Nón lưỡi trai cao cấp!', 'Text', NULL, '2026-07-19 16:00'),

-- Conversation 10 Messages
(10, 10, N'Phạm Minh Anh', N'Cho mình hỏi áo chạy bộ Hanoi Green Run có đủ size S, M, L không ạ?', 'Text', NULL, '2026-07-19 10:00'),
(10, 6, N'Sài Gòn Event Hub', N'Đầy đủ size nhé bạn, bạn chọn size trực tiếp khi nhận Bib chạy!', 'Text', NULL, '2026-07-19 10:30'),

-- Conversation 11 Messages
(11, 2, N'Trần Thị Customer', N'Show thời trang có thảm đỏ đón khách từ mấy giờ ạ?', 'Text', NULL, '2026-07-20 08:30'),
(11, 6, N'Sài Gòn Event Hub', N'Thảm đỏ mở đón khách từ 17:00, show trình diễn chính thức bắt đầu lúc 18:30 nhé!', 'Text', NULL, '2026-07-20 09:15'),

-- Conversation 12 Messages
(12, 9, N'Đặng Hoàng Nam', N'EDM Beach Party Nha Trang có giới hạn độ tuổi tham gia không?', 'Text', NULL, '2026-07-20 10:00'),
(12, 3, N'Trần Thị Organizer', N'Sự kiện dành cho khán giả từ 18 tuổi trở lên nhé bạn!', 'Text', NULL, '2026-07-20 10:30'),

-- Conversation 15 Messages
(15, 10, N'Phạm Minh Anh', N'Workshop AI Agents có yêu cầu mang theo laptop cá nhân để thực hành không?', 'Text', NULL, '2026-07-20 13:15'),
(15, 11, N'Vietnam Tech & Culture Media', N'Có nhé bạn, các bạn nên chuẩn bị sẵn VS Code và Node.js/Python environment!', 'Text', NULL, '2026-07-20 13:40'),

-- Conversation 17 Messages
(17, 13, N'Nguyễn Vũ Linh', N'Cho mình hỏi vị trí ghế VIP Orchestra có góc nhìn trực diện sân khấu không?', 'Text', NULL, '2026-07-20 15:10'),
(17, 3, N'Trần Thị Organizer', N'Hàng ghế VIP Orchestra nằm ở 5 hàng đầu tiên trung tâm Nhà hát Lớn nhé bạn!', 'Text', NULL, '2026-07-20 15:30'),

-- Conversation 21 (Private)
(21, 2, N'Trần Thị Customer', N'Dạ chào BTC, công ty mình muốn mua số lượng lớn vé có chiết khấu thêm không?', 'Text', NULL, '2026-07-18 14:00'),
(21, 3, N'Trần Thị Organizer', N'Chào bạn, với đoàn trên 10 người BTC có gói ưu đãi giảm 15%. Bạn cho mình xin SĐT nhé.', 'Text', NULL, '2026-07-18 14:30'),

-- Conversation 22 (Private)
(22, 5, N'Hoàng Minh Tuấn', N'Chào Admin, đơn hàng bị hủy của mình đã được hoàn tiền qua Momo chưa ạ?', 'Text', NULL, '2026-07-19 16:00'),
(22, 1, N'Nguyễn Văn Admin', N'Hệ thống đã tự động gửi lệnh hoàn tiền về ví Momo của bạn lúc 16:30 rồi nhé!', 'Text', NULL, '2026-07-19 16:45'),

-- Conversation 23 (Private)
(23, 9, N'Đặng Hoàng Nam', N'Chào BTC Tech Media, mình muốn đăng ký làm diễn giả phụ cho phiên Microservices được không?', 'Text', NULL, '2026-07-20 16:30'),
(23, 11, N'Vietnam Tech & Culture Media', N'Chào bạn! Rất hoan nghênh bạn, bạn gửi cho bên mình xin thông tin Profile và Slide đề xuất nhé!', 'Text', NULL, '2026-07-20 17:00');

GO
