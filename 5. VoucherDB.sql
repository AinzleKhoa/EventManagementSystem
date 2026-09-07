/* ==================================================
   DATABASE: VoucherDB
   SERVICE: VoucherAPI
   ================================================== */
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'VoucherDB')
BEGIN
    CREATE DATABASE VoucherDB;
END
GO

USE VoucherDB;
GO
DROP TABLE IF EXISTS UserVouchers;
DROP TABLE IF EXISTS Vouchers;

-- Bảng Vouchers
CREATE TABLE Vouchers (
    VoucherId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL, -- Refers to EventsDB.Events
    Code VARCHAR(50) NOT NULL,
    DiscountType VARCHAR(10) CHECK (DiscountType IN ('Percent', 'Amount')), 
    DiscountValue DECIMAL(18, 2) NOT NULL,
    Quantity INT NOT NULL,
    UsedCount INT DEFAULT 0,
    ValidFrom DATETIME,
    ValidTo DATETIME,
    IsActive BIT DEFAULT 1
);

CREATE TABLE UserVouchers (
    Id INT IDENTITY PRIMARY KEY,
    VoucherId INT NOT NULL,
    UserId INT NOT NULL, -- Refers to AuthDB.Users
    Quantity INT DEFAULT 1,
    CONSTRAINT FK_UserVoucher_Voucher FOREIGN KEY (VoucherId) REFERENCES Vouchers(VoucherId)
);

USE VoucherDB;
GO

INSERT INTO Vouchers (EventId, Code, DiscountType, DiscountValue, Quantity, UsedCount, ValidFrom, ValidTo, IsActive)
VALUES
-- Voucher 1: 10% off for Music Festival (Event 1 - Past)
(1, 'SUMMER10', 'Percent', 10.00, 100, 5, '2026-05-01 00:00', '2026-06-15 23:59', 1),

-- Voucher 2: 50k off for AI Tech Summit (Event 2 - Past)
(2, 'TECH50K', 'Amount', 50000.00, 50, 2, '2026-06-01 00:00', '2026-07-05 23:59', 1),

-- Voucher 3: 20% off for Saigon Jazz Night (Event 5 - Near Future)
(5, 'JAZZ20', 'Percent', 20.00, 50, 3, '2026-07-01 00:00', '2026-07-28 23:59', 1),

-- Voucher 4: 100k off for Da Nang Marathon (Event 7 - Near Future)
(7, 'RUN100K', 'Amount', 100000.00, 30, 2, '2026-07-15 00:00', '2026-08-07 23:59', 1),

-- Voucher 5: 15% off for Game Expo (Event 9 - Future)
(9, 'GAMEEXPO15', 'Percent', 15.00, 200, 10, '2026-07-15 00:00', '2026-08-16 23:59', 1),

-- Voucher 6: 50k off for Autumn Symphony (Event 17 - Far Future)
(17, 'AUTUMN50', 'Amount', 50000.00, 100, 1, '2026-09-01 00:00', '2026-10-25 23:59', 1),

-- Voucher 7: Expired Voucher for Testing Validation (Event 6 - Japan Fest)
(6, 'EXPIRED30', 'Percent', 30.00, 20, 5, '2026-06-01 00:00', '2026-07-01 00:00', 1),

-- Voucher 8: 10% off for System Architecture Workshop (Event 18)
(18, 'CLOUD10', 'Percent', 10.00, 40, 0, '2026-08-01 00:00', '2026-11-08 23:59', 1),

-- Voucher 9: 20k off for Coffee Expo (Event 19)
(19, 'COFFEE20K', 'Amount', 20000.00, 150, 0, '2026-08-01 00:00', '2026-11-15 23:59', 1);

INSERT INTO UserVouchers (VoucherId, UserId, Quantity)
VALUES 
(1, 2, 1), -- Customer 1 holds Voucher 1
(2, 2, 1), -- Customer 1 holds Voucher 2
(3, 2, 1), -- Customer 1 holds Voucher 3
(3, 5, 1), -- Customer 2 holds Voucher 3
(4, 5, 1), -- Customer 2 holds Voucher 4
(5, 8, 1), -- Customer 3 holds Voucher 5
(6, 2, 1), -- Customer 1 holds Voucher 6
(8, 10, 1),-- Customer 5 holds Voucher 8
(9, 14, 1);-- Customer 7 holds Voucher 9

GO