/* ==================================================
   DATABASE: WithdrawalDB
   SERVICE: WithdrawalAPI
   ================================================== */
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WithdrawalDB')
BEGIN
    CREATE DATABASE WithdrawalDB;
END
GO

USE WithdrawalDB;
GO

DROP TABLE IF EXISTS WithdrawalRequests;

CREATE TABLE WithdrawalRequests (
    WithdrawalId INT IDENTITY(1,1) PRIMARY KEY,
    OrganizerId INT NOT NULL, -- Refers to AuthDB.Users
    EventId INT NULL,         -- Refers to EventsDB.Events (Optional)
    Amount DECIMAL(18,2) NOT NULL,
    BankName NVARCHAR(100) NOT NULL,
    BankAccountNumber NVARCHAR(50) NOT NULL,
    BankAccountName NVARCHAR(100) NOT NULL,
    Status VARCHAR(20) DEFAULT 'Pending',
    RequestedAt DATETIME DEFAULT GETDATE(),
    ProcessedAt DATETIME NULL,
    RejectReason NVARCHAR(500)
);

USE WithdrawalDB;
GO

INSERT INTO WithdrawalRequests (OrganizerId, EventId, Amount, BankName, BankAccountNumber, BankAccountName, Status, RequestedAt, ProcessedAt, RejectReason)
VALUES
-- Request 1: Approved withdrawal for Organizer 1 (User 3, Event 1)
(3, 1, 5000000.00, 'Vietcombank', '0123456789', N'TRẦN THỊ ORGANIZER', 'Approved', '2026-06-17 10:00', '2026-06-18 14:00', NULL),

-- Request 2: Approved withdrawal for Organizer 1 (User 3, Event 2)
(3, 2, 4500000.00, 'Techcombank', '9876543210', N'TRẦN THỊ ORGANIZER', 'Approved', '2026-07-07 11:00', '2026-07-08 09:30', NULL),

-- Request 3: Pending withdrawal request for Organizer 1 (User 3, Event 3)
(3, 3, 2000000.00, 'TPBank', '1122334455', N'TRẦN THỊ ORGANIZER', 'Pending', '2026-07-15 16:00', NULL, NULL),

-- Request 4: Rejected withdrawal request for Organizer 2 (User 6, Event 4)
(6, 4, 3000000.00, 'MBBank', '9988776655', N'SÀI GÒN EVENT HUB', 'Rejected', '2026-07-10 14:00', '2026-07-11 10:00', N'Doanh thu chưa đối soát xong với cổng thanh toán VNPay'),

-- Request 5: Approved withdrawal request for Organizer 3 (User 11, Event 15)
(11, 15, 6000000.00, 'ACB', '5544332211', N'VIETNAM TECH & CULTURE MEDIA', 'Approved', '2026-07-19 09:00', '2026-07-19 15:00', NULL);

GO