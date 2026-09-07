/* ==================================================
   DATABASE: AuthDB
   SERVICE: AuthAPI
   ================================================== */
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AuthDB')
BEGIN
    CREATE DATABASE AuthDB;
END
GO

USE AuthDB;
GO

DROP TABLE IF EXISTS RefreshTokens;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Roles;

-- Bảng Roles
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

-- Bảng Users
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100),
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255),
    PhoneNumber VARCHAR(15),
    AvatarUrl NVARCHAR(500),
    RoleId INT NOT NULL,
    IsActive BIT DEFAULT 1,
    IsEmailVerified BIT DEFAULT 1,
    SecurityStamp NVARCHAR(100) NOT NULL DEFAULT (NEWID()),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- Bảng RefreshTokens
CREATE TABLE RefreshTokens (
    TokenId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Token VARCHAR(500) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BIT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

GO
INSERT INTO Roles (RoleName)
VALUES 
(N'admin'),
(N'customer'),
(N'organizer'),
(N'ticket_validator');

-- Password for all seed users is: Admin123@
-- Hash: AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==

INSERT INTO Users
(FullName, Email, PasswordHash, PhoneNumber, AvatarUrl, RoleId, IsActive, IsEmailVerified, CreatedAt)
VALUES
-- User 1: Admin
(N'Nguyễn Văn Admin', 'admin@auth.com', 
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000001',
 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=300&q=80',
 1, 1, 1, '2026-01-01 08:00'),

-- User 2: Customer 1
(N'Trần Thị Customer', 'customer@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000002',
 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=300&q=80',
 2, 1, 1, '2026-01-15 09:30'),

-- User 3: Organizer 1
(N'Trần Thị Organizer', 'organizer@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000003',
 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=300&q=80',
 3, 1, 1, '2026-01-10 10:00'),

-- User 4: Validator 1
(N'Phạm Văn Validator', 'validator@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000004',
 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=300&q=80',
 4, 1, 1, '2026-02-01 11:00'),

-- User 5: Customer 2
(N'Hoàng Minh Tuấn', 'customer2@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000005',
 'https://images.unsplash.com/photo-1522075469751-3a6694fb2f61?auto=format&fit=crop&w=300&q=80',
 2, 1, 1, '2026-02-15 14:20'),

-- User 6: Organizer 2
(N'Sài Gòn Event Hub', 'organizer2@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000006',
 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=300&q=80',
 3, 1, 1, '2026-03-01 16:45'),

-- User 7: Validator 2
(N'Nguyễn Văn Kiểm Vé', 'validator2@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000007',
 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&w=300&q=80',
 4, 1, 1, '2026-03-10 09:15'),

-- User 8: Customer 3
(N'Lê Thị Ngọc Bích', 'customer3@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000008',
 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&w=300&q=80',
 2, 1, 1, '2026-04-01 10:30'),

-- User 9: Customer 4
(N'Đặng Hoàng Nam', 'customer4@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000009',
 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=300&q=80',
 2, 1, 1, '2026-04-10 11:20'),

-- User 10: Customer 5
(N'Phạm Minh Anh', 'customer5@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000010',
 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=300&q=80',
 2, 1, 1, '2026-04-15 15:40'),

-- User 11: Organizer 3
(N'Vietnam Tech & Culture Media', 'organizer3@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000011',
 'https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=300&q=80',
 3, 1, 1, '2026-04-20 09:00'),

-- User 12: Validator 3
(N'Võ Văn Soát Vé', 'validator3@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000012',
 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=300&q=80',
 4, 1, 1, '2026-05-01 08:30'),

-- User 13: Customer 6
(N'Nguyễn Vũ Linh', 'customer6@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000013',
 'https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&w=300&q=80',
 2, 1, 1, '2026-05-10 10:15'),

-- User 14: Customer 7
(N'Bùi Thu Thảo', 'customer7@auth.com',
 'AQAAAAIAAYagAAAAEKjpYPOF7qP9MGTXAX3lWKoV5Z2M3migH8UmrUxGrq00QllDrr3rHg7/w26SvCi7SQ==', '0900000014',
 'https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=300&q=80',
 2, 1, 1, '2026-05-20 14:00');

GO
