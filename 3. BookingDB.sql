/* ==================================================
   DATABASE: BookingDB
   SERVICE: BookingAPI
   ================================================== */
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BookingDB')
BEGIN
    CREATE DATABASE BookingDB;
END
GO

USE BookingDB;
GO

DROP TABLE IF EXISTS Tickets;
DROP TABLE IF EXISTS OrderDetails;
DROP TABLE IF EXISTS Orders;

CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,

    CustomerId INT NOT NULL,      -- Refers to AuthDB.Users
    EventId INT NOT NULL,         -- Refers to EventsDB.Events

    -- Snapshot customer info
    FullName NVARCHAR(255) NULL,
    Email NVARCHAR(255) NULL,
    Phone NVARCHAR(50) NULL,

    -- Snapshot event info
    EventName NVARCHAR(MAX),
    PosterUrl NVARCHAR(MAX),

    OrderDate DATETIME DEFAULT GETDATE(),

    StartDate DATETIME,
    EndDate DATETIME,

    TotalAmount BIGINT NOT NULL,
    DiscountAmount BIGINT DEFAULT 0,
    FinalAmount BIGINT NOT NULL,

    PaymentMethod VARCHAR(50),
    TransactionId VARCHAR(100),

    Status VARCHAR(20) DEFAULT 'Pending',

    ExpiresAt DATETIME,

    UserVoucherId INT NULL
);

-- Bảng OrderDetails
CREATE TABLE OrderDetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    TicketTypeId INT NOT NULL, -- Refers to EventsDB.TicketTypes
    Quantity INT NOT NULL,
    UnitPrice BIGINT NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
);

CREATE TABLE Tickets (
    TicketId INT IDENTITY(1,1) PRIMARY KEY,
    TicketCode UNIQUEIDENTIFIER NOT NULL 
        DEFAULT NEWID() UNIQUE,
    OrderId INT NOT NULL,
    EventId INT NOT NULL,
    TicketTypeId INT NOT NULL,
    OwnerName NVARCHAR(100),
    Status VARCHAR(20) DEFAULT 'Active', 
    CheckedInAt DATETIME NULL,
    CheckedInBy INT NULL, -- Refers to AuthDB.Users (Validator)
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
);

USE BookingDB;
GO

-- 1. Create Orders
INSERT INTO Orders 
(CustomerId, EventId, FullName, Email, Phone, EventName, PosterUrl, OrderDate, StartDate, EndDate, TotalAmount, DiscountAmount, FinalAmount, PaymentMethod, TransactionId, Status, ExpiresAt, UserVoucherId)
VALUES 
-- Order 1: User 2 buys 2 tickets for Event 1 (Music Fest) -> Paid
(
  2, 1, N'Trần Thị Customer', 'customer@auth.com', '0900000002',
  N'Vietnam International Music Festival 2026',
  N'https://images.unsplash.com/photo-1470225620780-dba8ba36b745?auto=format&fit=crop&w=800&q=80',
  '2026-05-10 14:00', '2026-06-15 18:00', '2026-06-15 23:00',
  300000, 30000, 270000, 'VNPay', 'TXN_VNP_20260510_1001', 'Paid', NULL, 1
),
-- Order 2: User 5 buys 1 VIP ticket for Event 1 (Music Fest) -> Paid
(
  5, 1, N'Hoàng Minh Tuấn', 'customer2@auth.com', '0900000005',
  N'Vietnam International Music Festival 2026',
  N'https://images.unsplash.com/photo-1470225620780-dba8ba36b745?auto=format&fit=crop&w=800&q=80',
  '2026-05-12 16:30', '2026-06-15 18:00', '2026-06-15 23:00',
  350000, 0, 350000, 'Momo', 'TXN_MOM_20260512_1002', 'Paid', NULL, NULL
),
-- Order 3: User 9 buys 1 GA ticket for Event 1 (Music Fest) -> Paid
(
  9, 1, N'Đặng Hoàng Nam', 'customer4@auth.com', '0900000009',
  N'Vietnam International Music Festival 2026',
  N'https://images.unsplash.com/photo-1470225620780-dba8ba36b745?auto=format&fit=crop&w=800&q=80',
  '2026-05-14 09:15', '2026-06-15 18:00', '2026-06-15 23:00',
  150000, 0, 150000, 'VNPay', 'TXN_VNP_20260514_1003', 'Paid', NULL, NULL
),

-- Order 4: User 5 buys ticket for Event 2 (Tech Summit) -> Paid
(
  5, 2, N'Hoàng Minh Tuấn', 'customer2@auth.com', '0900000005',
  N'Vietnam Mobile & AI Tech Summit 2026',
  N'https://images.unsplash.com/photo-1540575467063-178a50c2df87?auto=format&fit=crop&w=800&q=80',
  '2026-06-10 09:00', '2026-07-05 08:30', '2026-07-05 17:30',
  500000, 50000, 450000, 'VNPay', 'TXN_VNP_20260610_1004', 'Paid', NULL, 2
),
-- Order 5: User 10 buys 1 VIP ticket for Event 2 (Tech Summit) -> Paid
(
  10, 2, N'Phạm Minh Anh', 'customer5@auth.com', '0900000010',
  N'Vietnam Mobile & AI Tech Summit 2026',
  N'https://images.unsplash.com/photo-1540575467063-178a50c2df87?auto=format&fit=crop&w=800&q=80',
  '2026-06-15 11:20', '2026-07-05 08:30', '2026-07-05 17:30',
  900000, 0, 900000, 'CreditCard', 'TXN_CC_20260615_1005', 'Paid', NULL, NULL
),

-- Order 6: User 2 buys ticket for Event 3 (Da Lat Acoustic) -> Paid
(
  2, 3, N'Trần Thị Customer', 'customer@auth.com', '0900000002',
  N'Da Lat Acoustic & Misty Night Festival 2026',
  N'https://images.unsplash.com/photo-1465847899084-d164df4dedc6?auto=format&fit=crop&w=800&q=80',
  '2026-04-15 11:00', '2026-05-10 18:30', '2026-05-10 22:00',
  120000, 0, 120000, 'Momo', 'TXN_MOM_20260415_1006', 'Paid', NULL, NULL
),
-- Order 7: User 13 buys VIP ticket for Event 3 (Da Lat Acoustic) -> Paid
(
  13, 3, N'Nguyễn Vũ Linh', 'customer6@auth.com', '0900000013',
  N'Da Lat Acoustic & Misty Night Festival 2026',
  N'https://images.unsplash.com/photo-1465847899084-d164df4dedc6?auto=format&fit=crop&w=800&q=80',
  '2026-04-18 14:00', '2026-05-10 18:30', '2026-05-10 22:00',
  300000, 0, 300000, 'VNPay', 'TXN_VNP_20260418_1007', 'Paid', NULL, NULL
),

-- Order 8: User 5 buys tickets for Event 4 (Food Fest) -> Paid
(
  5, 4, N'Hoàng Minh Tuấn', 'customer2@auth.com', '0900000005',
  N'Saigon Street Food Culture Festival 2026',
  N'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=800&q=80',
  '2026-06-01 10:00', '2026-06-20 10:00', '2026-06-22 22:00',
  100000, 0, 100000, 'VNPay', 'TXN_VNP_20260601_1008', 'Paid', NULL, NULL
),
-- Order 9: User 8 buys 1 ticket for Event 4 (Food Fest) -> Paid
(
  8, 4, N'Lê Thị Ngọc Bích', 'customer3@auth.com', '0900000008',
  N'Saigon Street Food Culture Festival 2026',
  N'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=800&q=80',
  '2026-06-05 16:20', '2026-06-20 10:00', '2026-06-22 22:00',
  50000, 0, 50000, 'Momo', 'TXN_MOM_20260605_1009', 'Paid', NULL, NULL
),

-- Order 10: User 2 buys 2 tickets for Event 5 (TODAY DEMO CHECKIN) -> Paid
(
  2, 5, N'Trần Thị Customer', 'customer@auth.com', '0900000002',
  N'Saigon Night Concert: Indie & Jazz Harmonies (Hôm nay - Demo Check-in)',
  N'https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?auto=format&fit=crop&w=800&q=80',
  '2026-07-10 15:00', '2026-07-22 08:00', '2026-07-22 23:30',
  500000, 100000, 400000, 'VNPay', 'TXN_VNP_20260710_1010', 'Paid', NULL, 3
),
-- Order 11: User 5 buys Couple ticket for Event 5 (TODAY DEMO CHECKIN) -> Paid
(
  5, 5, N'Hoàng Minh Tuấn', 'customer2@auth.com', '0900000005',
  N'Saigon Night Concert: Indie & Jazz Harmonies (Hôm nay - Demo Check-in)',
  N'https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?auto=format&fit=crop&w=800&q=80',
  '2026-07-12 10:00', '2026-07-22 08:00', '2026-07-22 23:30',
  550000, 0, 550000, 'Momo', 'TXN_MOM_20260712_1011', 'Paid', NULL, NULL
),
-- Order 12: User 14 buys Standard ticket for Event 5 (TODAY DEMO CHECKIN) -> Paid
(
  14, 5, N'Bùi Thu Thảo', 'customer7@auth.com', '0900000014',
  N'Saigon Night Concert: Indie & Jazz Harmonies (Hôm nay - Demo Check-in)',
  N'https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?auto=format&fit=crop&w=800&q=80',
  '2026-07-14 18:30', '2026-07-22 08:00', '2026-07-22 23:30',
  250000, 0, 250000, 'VNPay', 'TXN_VNP_20260714_1012', 'Paid', NULL, NULL
),

-- Order 13: User 8 buys tickets for Event 6 (TODAY DEMO CHECKIN) -> Paid
(
  8, 6, N'Lê Thị Ngọc Bích', 'customer3@auth.com', '0900000008',
  N'Vietnam - Japan Cultural & Food Festival 2026 (Hôm nay - Demo Check-in)',
  N'https://images.unsplash.com/photo-1528164344705-47542687990d?auto=format&fit=crop&w=800&q=80',
  '2026-07-15 14:00', '2026-07-22 09:00', '2026-07-23 21:00',
  160000, 0, 160000, 'VNPay', 'TXN_VNP_20260715_1013', 'Paid', NULL, NULL
),
-- Order 14: User 9 buys VIP Pass for Event 6 (TODAY DEMO CHECKIN) -> Paid
(
  9, 6, N'Đặng Hoàng Nam', 'customer4@auth.com', '0900000009',
  N'Vietnam - Japan Cultural & Food Festival 2026 (Hôm nay - Demo Check-in)',
  N'https://images.unsplash.com/photo-1528164344705-47542687990d?auto=format&fit=crop&w=800&q=80',
  '2026-07-16 10:15', '2026-07-22 09:00', '2026-07-23 21:00',
  220000, 0, 220000, 'Momo', 'TXN_MOM_20260716_1014', 'Paid', NULL, NULL
),
-- Order 15: User 5 cancelled order for Event 6 -> Cancelled
(
  5, 6, N'Hoàng Minh Tuấn', 'customer2@auth.com', '0900000005',
  N'Vietnam - Japan Cultural & Food Festival 2026',
  N'https://images.unsplash.com/photo-1528164344705-47542687990d?auto=format&fit=crop&w=800&q=80',
  '2026-07-14 09:00', '2026-07-22 09:00', '2026-07-23 21:00',
  80000, 0, 80000, 'Momo', 'TXN_MOM_CANCELLED_1015', 'Cancelled', NULL, NULL
),

-- Order 16: User 5 buys ticket for Event 7 (Da Nang Marathon) -> Paid
(
  5, 7, N'Hoàng Minh Tuấn', 'customer2@auth.com', '0900000005',
  N'Da Nang International Marathon & Light Fest 2026',
  N'https://images.unsplash.com/photo-1452626038306-9aae5e071dd3?auto=format&fit=crop&w=800&q=80',
  '2026-07-16 11:30', '2026-08-08 04:30', '2026-08-08 11:30',
  450000, 100000, 350000, 'CreditCard', 'TXN_CC_20260716_1016', 'Paid', NULL, 5
),
-- Order 17: User 10 buys Fun Run ticket for Event 7 (Da Nang Marathon) -> Paid
(
  10, 7, N'Phạm Minh Anh', 'customer5@auth.com', '0900000010',
  N'Da Nang International Marathon & Light Fest 2026',
  N'https://images.unsplash.com/photo-1452626038306-9aae5e071dd3?auto=format&fit=crop&w=800&q=80',
  '2026-07-17 15:00', '2026-08-08 04:30', '2026-08-08 11:30',
  200000, 0, 200000, 'VNPay', 'TXN_VNP_20260717_1017', 'Paid', NULL, NULL
),
-- Order 18: User 8 pending order for Event 7 -> Pending
(
  8, 7, N'Lê Thị Ngọc Bích', 'customer3@auth.com', '0900000008',
  N'Da Nang International Marathon & Light Fest 2026',
  N'https://images.unsplash.com/photo-1452626038306-9aae5e071dd3?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 07:30', '2026-08-08 04:30', '2026-08-08 11:30',
  200000, 0, 200000, 'VNPay', 'TXN_VNP_20260720_1018', 'Pending', '2026-07-20 09:30', NULL
),

-- Order 19: User 9 buys ticket for Event 8 (Startup Day) -> Paid
(
  9, 8, N'Đặng Hoàng Nam', 'customer4@auth.com', '0900000009',
  N'Startup & Innovation Venture Pitching Day 2026',
  N'https://images.unsplash.com/photo-1475721027785-f74eccf877e2?auto=format&fit=crop&w=800&q=80',
  '2026-07-18 10:00', '2026-08-12 13:00', '2026-08-12 18:00',
  150000, 0, 150000, 'VNPay', 'TXN_VNP_20260718_1019', 'Paid', NULL, NULL
),

-- Order 20: User 8 buys 2 tickets for Event 9 (Game Expo) -> Paid
(
  8, 9, N'Lê Thị Ngọc Bích', 'customer3@auth.com', '0900000008',
  N'Vietnam Game Expo & Esports Championship 2026',
  N'https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=800&q=80',
  '2026-07-18 16:00', '2026-08-15 09:00', '2026-08-16 21:00',
  200000, 30000, 170000, 'VNPay', 'TXN_VNP_20260718_1020', 'Paid', NULL, 6
),
-- Order 21: User 13 buys VIP Finals ticket for Event 9 (Game Expo) -> Paid
(
  13, 9, N'Nguyễn Vũ Linh', 'customer6@auth.com', '0900000013',
  N'Vietnam Game Expo & Esports Championship 2026',
  N'https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=800&q=80',
  '2026-07-19 14:20', '2026-08-15 09:00', '2026-08-16 21:00',
  350000, 0, 350000, 'Momo', 'TXN_MOM_20260719_1021', 'Paid', NULL, NULL
),

-- Order 22: User 10 buys 5KM Run Kit for Event 10 (Hanoi Run) -> Paid
(
  10, 10, N'Phạm Minh Anh', 'customer5@auth.com', '0900000010',
  N'Run For Green Hanoi Community Marathon 2026',
  N'https://images.unsplash.com/photo-1530549387789-4c1017266635?auto=format&fit=crop&w=800&q=80',
  '2026-07-19 09:00', '2026-08-25 05:00', '2026-08-25 11:00',
  180000, 0, 180000, 'VNPay', 'TXN_VNP_20260719_1022', 'Paid', NULL, NULL
),
-- Order 23: User 14 buys 10KM Run Kit for Event 10 (Hanoi Run) -> Paid
(
  14, 10, N'Bùi Thu Thảo', 'customer7@auth.com', '0900000014',
  N'Run For Green Hanoi Community Marathon 2026',
  N'https://images.unsplash.com/photo-1530549387789-4c1017266635?auto=format&fit=crop&w=800&q=80',
  '2026-07-19 11:30', '2026-08-25 05:00', '2026-08-25 11:00',
  280000, 0, 280000, 'Momo', 'TXN_MOM_20260719_1023', 'Paid', NULL, NULL
),

-- Order 24: User 2 buys Runway Ticket for Event 11 (Saigon Fashion Week) -> Paid
(
  2, 11, N'Trần Thị Customer', 'customer@auth.com', '0900000002',
  N'Saigon Fashion Week & Modern Design Summit 2026',
  N'https://images.unsplash.com/photo-1509631179647-0177331693ae?auto=format&fit=crop&w=800&q=80',
  '2026-07-19 16:00', '2026-08-28 18:00', '2026-08-30 22:00',
  300000, 0, 300000, 'VNPay', 'TXN_VNP_20260719_1024', 'Paid', NULL, NULL
),
-- Order 25: User 13 buys Front Row VIP for Event 11 (Saigon Fashion Week) -> Paid
(
  13, 11, N'Nguyễn Vũ Linh', 'customer6@auth.com', '0900000013',
  N'Saigon Fashion Week & Modern Design Summit 2026',
  N'https://images.unsplash.com/photo-1509631179647-0177331693ae?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 08:00', '2026-08-28 18:00', '2026-08-30 22:00',
  650000, 0, 650000, 'CreditCard', 'TXN_CC_20260720_1025', 'Paid', NULL, NULL
),

-- Order 26: User 9 buys Beach Party Pass for Event 12 (Nha Trang EDM) -> Paid
(
  9, 12, N'Đặng Hoàng Nam', 'customer4@auth.com', '0900000009',
  N'Nha Trang Sea Symphony & EDM Beach Party 2026',
  N'https://images.unsplash.com/photo-1516450360452-9312f5e86fc7?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 09:30', '2026-09-02 17:00', '2026-09-02 23:59',
  200000, 0, 200000, 'VNPay', 'TXN_VNP_20260720_1026', 'Paid', NULL, NULL
),
-- Order 27: User 14 buys VIP Lounge Pass for Event 12 (Nha Trang EDM) -> Paid
(
  14, 12, N'Bùi Thu Thảo', 'customer7@auth.com', '0900000014',
  N'Nha Trang Sea Symphony & EDM Beach Party 2026',
  N'https://images.unsplash.com/photo-1516450360452-9312f5e86fc7?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 10:00', '2026-09-02 17:00', '2026-09-02 23:59',
  850000, 0, 850000, 'Momo', 'TXN_MOM_20260720_1027', 'Paid', NULL, NULL
),

-- Order 28: User 8 buys Gallery Ticket for Event 13 (Art Exhibition) -> Paid
(
  8, 13, N'Lê Thị Ngọc Bích', 'customer3@auth.com', '0900000008',
  N'Contemporary Art Exhibition "Light & Identity"',
  N'https://images.unsplash.com/photo-1536924940846-227afb31e2a5?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 11:00', '2026-09-10 09:00', '2026-09-12 18:00',
  80000, 0, 80000, 'VNPay', 'TXN_VNP_20260720_1028', 'Paid', NULL, NULL
),

-- Order 29: User 5 buys Full Workshop Pass for Event 14 (Photography Workshop) -> Paid
(
  5, 14, N'Hoàng Minh Tuấn', 'customer2@auth.com', '0900000005',
  N'Mastering Portrait Photography & Post-Processing',
  N'https://images.unsplash.com/photo-1452587925148-ce544e77e70d?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 12:00', '2026-09-18 08:30', '2026-09-18 16:30',
  150000, 0, 150000, 'Momo', 'TXN_MOM_20260720_1029', 'Paid', NULL, NULL
),

-- Order 30: User 10 buys Student Pass for Event 15 (AI Workshop) -> Paid
(
  10, 15, N'Phạm Minh Anh', 'customer5@auth.com', '0900000010',
  N'Digital Era: AI Agents & Microservices Workshop',
  N'https://images.unsplash.com/photo-1531403009284-440f080d1e12?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 13:00', '2026-10-05 13:30', '2026-10-05 17:30',
  150000, 0, 150000, 'VNPay', 'TXN_VNP_20260720_1030', 'Paid', NULL, NULL
),

-- Order 31: User 9 buys Forum Pass for Event 16 (E-Commerce Forum) -> Paid
(
  9, 16, N'Đặng Hoàng Nam', 'customer4@auth.com', '0900000009',
  N'Vietnam E-Commerce & Retail Innovation Forum 2026',
  N'https://images.unsplash.com/photo-1556761175-5973dc0f32e7?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 14:00', '2026-10-12 08:00', '2026-10-12 17:00',
  400000, 0, 400000, 'CreditCard', 'TXN_CC_20260720_1031', 'Paid', NULL, NULL
),

-- Order 32: User 2 buys Balcony Pass for Event 17 (Autumn Symphony) -> Paid
(
  2, 17, N'Trần Thị Customer', 'customer@auth.com', '0900000002',
  N'Autumn Symphony: Classical Concert Series 2026',
  N'https://images.unsplash.com/photo-1514525253161-7a46d19cd819?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 14:30', '2026-10-25 19:30', '2026-10-25 22:00',
  350000, 50000, 300000, 'VNPay', 'TXN_VNP_20260720_1032', 'Paid', NULL, 7
),
-- Order 33: User 13 buys Orchestra VIP for Event 17 (Autumn Symphony) -> Paid
(
  13, 17, N'Nguyễn Vũ Linh', 'customer6@auth.com', '0900000013',
  N'Autumn Symphony: Classical Concert Series 2026',
  N'https://images.unsplash.com/photo-1514525253161-7a46d19cd819?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 15:00', '2026-10-25 19:30', '2026-10-25 22:00',
  800000, 0, 800000, 'Momo', 'TXN_MOM_20260720_1033', 'Paid', NULL, NULL
),

-- Order 34: User 5 buys ticket for Event 18 (Cloud Native Workshop) -> Paid
(
  5, 18, N'Hoàng Minh Tuấn', 'customer2@auth.com', '0900000005',
  N'Tech Masterclass: System Architecture & Cloud Native',
  N'https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 15:30', '2026-11-08 08:30', '2026-11-08 17:00',
  250000, 0, 250000, 'VNPay', 'TXN_VNP_20260720_1034', 'Paid', NULL, NULL
),

-- Order 35: User 8 buys ticket for Event 19 (Coffee Expo) -> Paid
(
  8, 19, N'Lê Thị Ngọc Bích', 'customer3@auth.com', '0900000008',
  N'Saigon Coffee & Bakery Cultural Expo 2026',
  N'https://images.unsplash.com/photo-1447933601403-0c6688de566e?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 16:00', '2026-11-15 08:00', '2026-11-17 21:00',
  60000, 0, 60000, 'Momo', 'TXN_MOM_20260720_1035', 'Paid', NULL, NULL
),
-- Order 36: User 14 buys ticket for Event 19 (Coffee Expo) -> Paid
(
  14, 19, N'Bùi Thu Thảo', 'customer7@auth.com', '0900000014',
  N'Saigon Coffee & Bakery Cultural Expo 2026',
  N'https://images.unsplash.com/photo-1447933601403-0c6688de566e?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 16:15', '2026-11-15 08:00', '2026-11-17 21:00',
  60000, 0, 60000, 'VNPay', 'TXN_VNP_20260720_1036', 'Paid', NULL, NULL
),

-- Order 37: User 9 buys ticket for Event 20 (Badminton Open) -> Paid
(
  9, 20, N'Đặng Hoàng Nam', 'customer4@auth.com', '0900000009',
  N'National Badminton Open Championship 2026',
  N'https://images.unsplash.com/photo-1626224583764-f87db24ac4ea?auto=format&fit=crop&w=800&q=80',
  '2026-07-20 16:30', '2026-11-20 08:00', '2026-11-22 20:00',
  100000, 0, 100000, 'Momo', 'TXN_MOM_20260720_1037', 'Paid', NULL, NULL
),

-- Order 38: User 2 buys ticket for Event 4 (Past - Food Fest), for review demo
(
  2, 4, N'Trần Thị Customer', 'customer@auth.com', '0900000002',
  N'Saigon Street Food Culture Festival 2026',
  N'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=800&q=80',
  '2026-06-02 09:30', '2026-06-20 10:00', '2026-06-22 22:00',
  50000, 0, 50000, 'VNPay', 'TXN_VNP_REVIEW_DEMO_1038', 'Paid', NULL, NULL
),

-- Order 39: User 5 buys ticket for Event 3 (Past - Da Lat Acoustic), for review demo
(
  5, 3, N'Hoàng Minh Tuấn', 'customer2@auth.com', '0900000005',
  N'Da Lat Acoustic & Misty Night Festival 2026',
  N'https://images.unsplash.com/photo-1465847899084-d164df4dedc6?auto=format&fit=crop&w=800&q=80',
  '2026-04-16 10:45', '2026-05-10 18:30', '2026-05-10 22:00',
  120000, 0, 120000, 'Momo', 'TXN_MOM_REVIEW_DEMO_1039', 'Paid', NULL, NULL
);

-- 2. Create Order Details
INSERT INTO OrderDetails (OrderId, TicketTypeId, Quantity, UnitPrice)
VALUES 
(1, 1, 2, 150000),   -- Order 1: 2 x GA (Event 1)
(2, 2, 1, 350000),   -- Order 2: 1 x VIP (Event 1)
(3, 1, 1, 150000),   -- Order 3: 1 x GA (Event 1)
(4, 3, 1, 500000),   -- Order 4: 1 x Standard (Event 2)
(5, 4, 1, 900000),   -- Order 5: 1 x VIP Executive (Event 2)
(6, 5, 1, 120000),   -- Order 6: 1 x Standard Seating (Event 3)
(7, 6, 1, 300000),   -- Order 7: 1 x Front Row VIP (Event 3)
(8, 7, 2, 50000),    -- Order 8: 2 x General Foodie (Event 4)
(9, 7, 1, 50000),    -- Order 9: 1 x General Foodie (Event 4)
(10, 8, 2, 250000),  -- Order 10: 2 x Standard Pass (Event 5)
(11, 9, 1, 550000),  -- Order 11: 1 x Couples Front Row (Event 5)
(12, 8, 1, 250000),  -- Order 12: 1 x Standard Pass (Event 5)
(13, 10, 2, 80000),  -- Order 13: 2 x Festival Entry (Event 6)
(14, 11, 1, 220000), -- Order 14: 1 x VIP Workshop (Event 6)
(15, 10, 1, 80000),  -- Order 15: 1 x Festival Entry (Event 6 - Cancelled)
(16, 13, 1, 450000), -- Order 16: 1 x 21KM Runner Kit (Event 7)
(17, 12, 1, 200000), -- Order 17: 1 x 5KM Fun Run (Event 7)
(18, 12, 1, 200000), -- Order 18: 1 x 5KM Fun Run (Event 7 - Pending)
(19, 14, 1, 150000), -- Order 19: 1 x Standard Delegate (Event 8)
(20, 16, 2, 100000), -- Order 20: 2 x 1-Day Expo Pass (Event 9)
(21, 17, 1, 350000), -- Order 21: 1 x eSports VIP Finals (Event 9)
(22, 18, 1, 180000), -- Order 22: 1 x 5KM Run Kit (Event 10)
(23, 19, 1, 280000), -- Order 23: 1 x 10KM Run Kit (Event 10)
(24, 20, 1, 300000), -- Order 24: 1 x Red Carpet Ticket (Event 11)
(25, 21, 1, 650000), -- Order 25: 1 x Front Row VIP (Event 11)
(26, 22, 1, 200000), -- Order 26: 1 x GA Beach Party (Event 12)
(27, 23, 1, 850000), -- Order 27: 1 x VIP Lounge Access (Event 12)
(28, 24, 1, 80000),  -- Order 28: 1 x Gallery Ticket (Event 13)
(29, 25, 1, 150000), -- Order 29: 1 x Full Workshop Pass (Event 14)
(30, 26, 1, 150000), -- Order 30: 1 x Student Discount Pass (Event 15)
(31, 28, 1, 400000), -- Order 31: 1 x All-Access Forum Pass (Event 16)
(32, 29, 1, 350000), -- Order 32: 1 x Balcony Pass (Event 17)
(33, 30, 1, 800000), -- Order 33: 1 x Orchestra VIP (Event 17)
(34, 31, 1, 250000), -- Order 34: 1 x Masterclass Pass (Event 18)
(35, 32, 1, 60000),  -- Order 35: 1 x Coffee Expo Ticket (Event 19)
(36, 32, 1, 60000),  -- Order 36: 1 x Coffee Expo Ticket (Event 19)
(37, 33, 1, 100000), -- Order 37: 1 x Badminton Spectator Pass (Event 20)
(38, 7, 1, 50000),   -- Order 38: 1 x General Foodie (Event 4 review demo)
(39, 5, 1, 120000);  -- Order 39: 1 x Standard Seating (Event 3 review demo)

-- 3. Generate Tickets (WITH EXPLICIT EASY GUID CODES FOR TODAY CHECKIN DEMO!)
INSERT INTO Tickets (TicketCode, OrderId, EventId, TicketTypeId, OwnerName, Status, CheckedInAt, CheckedInBy)
VALUES 
-- Past Events (1, 2, 3, 4) - Checked in by Validators
(NEWID(), 1, 1, 1, N'Trần Thị Customer', 'Used', '2026-06-15 18:30', 4),
(NEWID(), 1, 1, 1, N'Nguyễn Văn Bạn Thân', 'Used', '2026-06-15 18:45', 4),
(NEWID(), 2, 1, 2, N'Hoàng Minh Tuấn (VIP)', 'Used', '2026-06-15 18:15', 4),
(NEWID(), 3, 1, 1, N'Đặng Hoàng Nam', 'Used', '2026-06-15 19:00', 4),

(NEWID(), 4, 2, 3, N'Hoàng Minh Tuấn', 'Used', '2026-07-05 08:45', 4),
(NEWID(), 5, 2, 4, N'Phạm Minh Anh (VIP)', 'Used', '2026-07-05 08:30', 4),

(NEWID(), 6, 3, 5, N'Trần Thị Customer', 'Used', '2026-05-10 18:40', 4),
(NEWID(), 7, 3, 6, N'Nguyễn Vũ Linh (VIP)', 'Used', '2026-05-10 18:20', 4),

(NEWID(), 8, 4, 7, N'Hoàng Minh Tuấn', 'Used', '2026-06-20 11:15', 4),
(NEWID(), 8, 4, 7, N'Lê Văn Bạn Đi Ăn', 'Used', '2026-06-20 11:15', 4),
(NEWID(), 9, 4, 7, N'Lê Thị Ngọc Bích', 'Used', '2026-06-20 12:00', 4),

-- TODAY DEMO CHECKIN TICKETS (Event 5 & Event 6) WITH EASY-TO-COPY GUID CODES!
-- Validator: validator@auth.com (StaffId 4) -> Event 5
('11111111-1111-1111-1111-111111111111', 10, 5, 8, N'Trần Thị Customer (Demo Active Ticket #1)', 'Active', NULL, NULL),
('22222222-2222-2222-2222-222222222222', 10, 5, 8, N'Nguyễn Văn Đồng Nghiệp (Demo Active Ticket #2)', 'Active', NULL, NULL),
('55555555-5555-5555-5555-555555555555', 11, 5, 9, N'Hoàng Minh Tuấn & Bạn (Demo Active Ticket #3)', 'Active', NULL, NULL),
('66666666-6666-6666-6666-666666666666', 12, 5, 8, N'Bùi Thu Thảo (Demo Active Ticket #4)', 'Active', NULL, NULL),

-- Validator: validator2@auth.com (StaffId 7) -> Event 6
('33333333-3333-3333-3333-333333333333', 13, 6, 10, N'Lê Thị Ngọc Bích (Demo Active Ticket #5)', 'Active', NULL, NULL),
('44444444-4444-4444-4444-444444444444', 13, 6, 10, N'Phạm Văn Em Trai (Demo Active Ticket #6)', 'Active', NULL, NULL),
('77777777-7777-7777-7777-777777777777', 14, 6, 11, N'Đặng Hoàng Nam (Demo Active VIP Ticket #7)', 'Active', NULL, NULL),
(NEWID(), 15, 6, 10, N'Hoàng Minh Tuấn', 'Cancelled', NULL, NULL),

-- Upcoming Events (7 to 20) - Active Status
(NEWID(), 16, 7, 13, N'Hoàng Minh Tuấn (Runner)', 'Active', NULL, NULL),
(NEWID(), 17, 7, 12, N'Phạm Minh Anh', 'Active', NULL, NULL),
(NEWID(), 18, 7, 12, N'Lê Thị Ngọc Bích', 'Active', NULL, NULL),

(NEWID(), 19, 8, 14, N'Đặng Hoàng Nam', 'Active', NULL, NULL),

(NEWID(), 20, 9, 16, N'Lê Thị Ngọc Bích', 'Active', NULL, NULL),
(NEWID(), 20, 9, 16, N'Nguyễn Văn Gamer', 'Active', NULL, NULL),
(NEWID(), 21, 9, 17, N'Nguyễn Vũ Linh (eSports VIP)', 'Active', NULL, NULL),

(NEWID(), 22, 10, 18, N'Phạm Minh Anh', 'Active', NULL, NULL),
(NEWID(), 23, 10, 19, N'Bùi Thu Thảo', 'Active', NULL, NULL),

(NEWID(), 24, 11, 20, N'Trần Thị Customer', 'Active', NULL, NULL),
(NEWID(), 25, 11, 21, N'Nguyễn Vũ Linh (Front Row VIP)', 'Active', NULL, NULL),

(NEWID(), 26, 12, 22, N'Đặng Hoàng Nam', 'Active', NULL, NULL),
(NEWID(), 27, 12, 23, N'Bùi Thu Thảo (VIP Lounge)', 'Active', NULL, NULL),

(NEWID(), 28, 13, 24, N'Lê Thị Ngọc Bích', 'Active', NULL, NULL),
(NEWID(), 29, 14, 25, N'Hoàng Minh Tuấn', 'Active', NULL, NULL),
(NEWID(), 30, 15, 26, N'Phạm Minh Anh', 'Active', NULL, NULL),
(NEWID(), 31, 16, 28, N'Đặng Hoàng Nam', 'Active', NULL, NULL),
(NEWID(), 32, 17, 29, N'Trần Thị Customer', 'Active', NULL, NULL),
(NEWID(), 33, 17, 30, N'Nguyễn Vũ Linh (Orchestra VIP)', 'Active', NULL, NULL),
(NEWID(), 34, 18, 31, N'Hoàng Minh Tuấn', 'Active', NULL, NULL),
(NEWID(), 35, 19, 32, N'Lê Thị Ngọc Bích', 'Active', NULL, NULL),
(NEWID(), 36, 19, 32, N'Bùi Thu Thảo', 'Active', NULL, NULL),
(NEWID(), 37, 20, 33, N'Đặng Hoàng Nam', 'Active', NULL, NULL),

-- Review demo tickets for customer@auth.com and customer2@auth.com
(NEWID(), 38, 4, 7, N'Trần Thị Customer (Foodie)', 'Used', '2026-06-20 11:05', 4),
(NEWID(), 39, 3, 5, N'Hoàng Minh Tuấn (Acoustic)', 'Used', '2026-05-10 18:55', 4);

GO
