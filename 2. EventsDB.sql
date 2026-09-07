/* ==================================================
   DATABASE: EventsDB
   SERVICE: EventsAPI
   ================================================== */

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'EventsDB')
BEGIN
    CREATE DATABASE EventsDB;
END
GO

USE EventsDB;
GO

/* ==================================================
   DROP TABLE (Correct order to avoid FK conflict)
   ================================================== */

DROP TABLE IF EXISTS EventStaff;
DROP TABLE IF EXISTS TicketTypes;
DROP TABLE IF EXISTS Events;
GO

/* ==================================================
   TABLE: Events
   ================================================== */

CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,

    OrganizerId INT NOT NULL, -- Refers to AuthDB.Users(UserId)
    CategoryId INT NOT NULL,  -- Refers to CategoryDB.Categories(CategoryId)

    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),

    PosterUrl NVARCHAR(500),

    Location NVARCHAR(200),
    [Longitude] FLOAT NULL,
    [Latitude] FLOAT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,

    Status VARCHAR(20) DEFAULT 'Draft',

    RejectedReason NVARCHAR(500),

    CreatedAt DATETIME DEFAULT GETDATE(),

    ConversationId INT NULL
);
GO

/* ==================================================
   TABLE: TicketTypes
   ================================================== */

CREATE TABLE TicketTypes (
    TicketTypeId INT IDENTITY(1,1) PRIMARY KEY,

    EventId INT NOT NULL,

    TypeName NVARCHAR(100) NOT NULL,

    Price BIGINT NOT NULL DEFAULT 0,

    Quantity INT NOT NULL CHECK (Quantity >= 0),

    SoldQuantity INT DEFAULT 0,

    ColorCode VARCHAR(20),

    SaleStartDate DATETIME NULL,

    SaleEndDate DATETIME NULL,

    CONSTRAINT FK_TicketTypes_Event
        FOREIGN KEY (EventId)
        REFERENCES Events(EventId)
        ON DELETE CASCADE
);
GO

/* ==================================================
   TABLE: EventStaff
   ================================================== */

CREATE TABLE EventStaff
(
    EventStaffId INT IDENTITY(1,1) PRIMARY KEY,

    EventId INT NOT NULL,

    StaffId INT NOT NULL,

    StaffEmail NVARCHAR(255) NULL,

    ValidatorRole NVARCHAR(50) NULL,

    AssignedAt DATETIME DEFAULT GETDATE(),

    ExpiredAt DATETIME NULL,

    IsActive BIT DEFAULT 1,

    CONSTRAINT FK_EventStaff_Event
        FOREIGN KEY (EventId)
        REFERENCES Events(EventId)
);
GO

/* ==================================================
   UNIQUE INDEX
   ================================================== */

CREATE UNIQUE NONCLUSTERED INDEX UQ_Event_Staff_Email
ON EventStaff(EventId, StaffEmail);
GO

/* ==================================================
   SEED DATA: Events (25 Events)
   ================================================== */

INSERT INTO Events
(OrganizerId, CategoryId, Title, Description, PosterUrl, Location, Latitude, Longitude, StartDate, EndDate, Status, RejectedReason, CreatedAt, ConversationId)
VALUES
-- 1: Past (Music) - Conversation 1
(
  3, 1,
  N'Vietnam International Music Festival 2026',
  N'An outdoor mega music festival bringing together top Indie and Pop bands along with international guest artists.',
  N'https://images.unsplash.com/photo-1470225620780-dba8ba36b745?auto=format&fit=crop&w=800&q=80',
  N'Nguyen Hue Walking Street, District 1, Ho Chi Minh City',
  10.7738, 106.7042,
  '2026-06-15 18:00', '2026-06-15 23:00', 'Approved', NULL, '2026-04-01 10:00', 1
),
-- 2: Past (Conference) - Conversation 2
(
  3, 2,
  N'Vietnam Mobile & AI Tech Summit 2026',
  N'A major technology conference covering Applied AI, Cloud Computing, and Mobile Application Trends for 2026.',
  N'https://images.unsplash.com/photo-1540575467063-178a50c2df87?auto=format&fit=crop&w=800&q=80',
  N'National Convention Center, Nam Tu Liem, Hanoi',
  21.0068, 105.7836,
  '2026-07-05 08:30', '2026-07-05 17:30', 'Approved', NULL, '2026-05-01 09:00', 2
),
-- 3: Past (Music) - Conversation 3
(
  3, 1,
  N'Da Lat Acoustic & Misty Night Festival 2026',
  N'An intimate acoustic night amidst the misty pine forests of Da Lat featuring soothing melodies and soulful vocals.',
  N'https://images.unsplash.com/photo-1465847899084-d164df4dedc6?auto=format&fit=crop&w=800&q=80',
  N'Lam Vien Square, Ward 10, Da Lat City, Lam Dong',
  11.9363, 108.4452,
  '2026-05-10 18:30', '2026-05-10 22:00', 'Approved', NULL, '2026-03-15 14:00', 3
),
-- 4: Past (Entertainment) - Conversation 4
(
  3, 6,
  N'Saigon Street Food Culture Festival 2026',
  N'A vibrant street food festival gathering over 100 authentic regional culinary stalls and Asian food culture exchanges.',
  N'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=800&q=80',
  N'23 September Park, District 1, Ho Chi Minh City',
  10.7686, 106.6934,
  '2026-06-20 10:00', '2026-06-22 22:00', 'Approved', NULL, '2026-04-10 11:30', 4
),

-- 5: TODAY EVENT (Music - 22/07/2026 DEMO CHECKIN) - Conversation 5
(
  3, 1,
  N'Saigon Night Concert: Indie & Jazz Harmonies (Hôm nay - Demo Check-in)',
  N'An emotional and cozy music night filled with melodic Indie Pop and Jazz tunes taking place TODAY in the heart of Saigon.',
  N'https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?auto=format&fit=crop&w=800&q=80',
  N'Municipal Theatre of Ho Chi Minh City, District 1, Ho Chi Minh City',
  10.7766, 106.7032,
  '2026-07-22 08:00', '2026-07-22 23:30', 'Approved', NULL, '2026-06-01 08:00', 5
),
-- 6: TODAY / NEAR FUTURE EVENT (Entertainment - 22/07/2026 - 23/07/2026 DEMO CHECKIN) - Conversation 6
(
  3, 6,
  N'Vietnam - Japan Cultural & Food Festival 2026',
  N'A cross-cultural celebration featuring Cosplay performances, authentic Sushi & Ramen stalls, and Origami art workshops.',
  N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRZ6xpTpQyPVQvboJrOhGUyo_zI3IVKUFGKrQNX9G1Ccw&s=10',
  N'Nguyen Du Stadium, 116 Nguyen Du Street, District 1, Ho Chi Minh City',
  10.7731, 106.6924,
  '2026-07-22 09:00', '2026-07-23 21:00', 'Approved', NULL, '2026-06-10 09:00', 6
),
-- 7: Near Future (Sports - 08/08/2026) - Conversation 7
(
  6, 5,
  N'Da Nang International Marathon & Light Fest 2026',
  N'An international marathon along My Khe coastline and iconic bridges of Da Nang combined with a spectacular light show.',
  N'https://images.unsplash.com/photo-1452626038306-9aae5e071dd3?auto=format&fit=crop&w=800&q=80',
  N'East Sea Park, Vo Nguyen Giap Street, Son Tra, Da Nang',
  16.0682, 108.2467,
  '2026-08-08 04:30', '2026-08-08 11:30', 'Approved', NULL, '2026-06-15 10:00', 7
),
-- 8: Future (Conference - 12/08/2026) - Conversation 8
(
  6, 2,
  N'Startup & Innovation Venture Pitching Day 2026',
  N'A high-stakes investment networking event for 50+ promising tech startups to pitch to top regional venture capitalists.',
  N'https://images.unsplash.com/photo-1475721027785-f74eccf877e2?auto=format&fit=crop&w=800&q=80',
  N'Dreamplex Co-working Space, District 1, Ho Chi Minh City',
  10.7825, 106.7051,
  '2026-08-12 13:00', '2026-08-12 18:00', 'Approved', NULL, '2026-06-20 14:00', 8
),
-- 9: Future (Entertainment - 15/08/2026) - Conversation 9
(
  6, 6,
  N'Vietnam Game Expo & Esports Championship 2026',
  N'The premier gaming convention and eSports tournament featuring professional teams competing live.',
  N'https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=800&q=80',
  N'SECC - Saigon Exhibition and Convention Center, District 7, Ho Chi Minh City',
  10.7297, 106.7214,
  '2026-08-15 09:00', '2026-08-16 21:00', 'Approved', NULL, '2026-06-25 15:00', 9
),
-- 10: Future (Sports - 25/08/2026) - Conversation 10
(
  6, 5,
  N'Run For Green Hanoi Community Marathon 2026',
  N'A public charity marathon aimed at raising funds for urban tree planting and environmental sustainability in Hanoi.',
  N'https://images.unsplash.com/photo-1530549387789-4c1017266635?auto=format&fit=crop&w=800&q=80',
  N'Hoan Kiem Lake Walking Street, Hoan Kiem District, Hanoi',
  21.0285, 105.8542,
  '2026-08-25 05:00', '2026-08-25 11:00', 'Approved', NULL, '2026-07-01 08:00', 10
),
-- 11: Future (Exhibition - 28/08/2026) - Conversation 11
(
  6, 4,
  N'Saigon Fashion Week & Modern Design Summit 2026',
  N'An elegant runway showcase highlighting modern couture combined with traditional Vietnamese brocade and silk.',
  N'https://images.unsplash.com/photo-1509631179647-0177331693ae?auto=format&fit=crop&w=800&q=80',
  N'GEM Center, 8 Nguyen Binh Khiem, District 1, Ho Chi Minh City',
  10.7871, 106.7029,
  '2026-08-28 18:00', '2026-08-30 22:00', 'Approved', NULL, '2026-07-05 09:00', 11
),

-- 12: Mid Future (Music - 02/09/2026) - Conversation 12
(
  3, 1,
  N'Nha Trang Sea Symphony & EDM Beach Party 2026',
  N'An energetic beachside EDM festival welcoming the autumn vibe on Nha Trang shore with top international DJs.',
  N'https://images.unsplash.com/photo-1516450360452-9312f5e86fc7?auto=format&fit=crop&w=800&q=80',
  N'Tran Phu Beach, Loc Tho Ward, Nha Trang City, Khanh Hoa',
  12.2388, 109.1966,
  '2026-09-02 17:00', '2026-09-02 23:59', 'Approved', NULL, '2026-07-08 10:00', 12
),
-- 13: Mid Future (Exhibition - 10/09/2026) - Conversation 13
(
  3, 4,
  N'Contemporary Art Exhibition "Light & Identity"',
  N'An immersive art gallery featuring contemporary paintings, light installations, and sculptures by young artists.',
  N'https://images.unsplash.com/photo-1536924940846-227afb31e2a5?auto=format&fit=crop&w=800&q=80',
  N'Ho Chi Minh City Museum of Fine Arts, District 1, Ho Chi Minh City',
  10.7698, 106.6988,
  '2026-09-10 09:00', '2026-09-12 18:00', 'Approved', NULL, '2026-07-10 11:00', 13
),
-- 14: Mid Future (Workshop - 18/09/2026) - Conversation 14
(
  3, 3,
  N'Mastering Portrait Photography & Post-Processing',
  N'A hands-on masterclass covering outdoor portrait lighting techniques, composition, and professional color grading.',
  N'https://images.unsplash.com/photo-1452587925148-ce544e77e70d?auto=format&fit=crop&w=800&q=80',
  N'Can Tho Heritage House, Binh Thuy District, Can Tho',
  10.0572, 105.7531,
  '2026-09-18 08:30', '2026-09-18 16:30', 'Approved', NULL, '2026-07-12 13:00', 14
),

-- 15: Far Future (Workshop - 05/10/2026) - Conversation 15
(
  11, 3,
  N'Digital Era: AI Agents & Microservices Workshop',
  N'An intensive technical workshop focusing on AI Agent integration and Microservices Architecture in enterprise software.',
  N'https://images.unsplash.com/photo-1531403009284-440f080d1e12?auto=format&fit=crop&w=800&q=80',
  N'FPT Complex Da Nang, Ngu Hanh Son District, Da Nang',
  15.9867, 108.2612,
  '2026-10-05 13:30', '2026-10-05 17:30', 'Approved', NULL, '2026-07-15 14:00', 15
),
-- 16: Far Future (Conference - 12/10/2026) - Conversation 16
(
  11, 2,
  N'Vietnam E-Commerce & Retail Innovation Forum 2026',
  N'An industry forum covering E-Commerce trends, Smart Logistics, and Omnichannel Customer Experience strategies.',
  N'https://images.unsplash.com/photo-1556761175-5973dc0f32e7?auto=format&fit=crop&w=800&q=80',
  N'Lotte Hotel Hanoi, Ba Dinh District, Hanoi',
  21.0322, 105.8118,
  '2026-10-12 08:00', '2026-10-12 17:00', 'Approved', NULL, '2026-07-16 15:00', 16
),
-- 17: Far Future (Music - 25/10/2026) - Conversation 17
(
  3, 1,
  N'Autumn Symphony: Classical Concert Series 2026',
  N'A grand autumn orchestra performing timeless symphonic masterpieces by Mozart, Beethoven, and classical composers.',
  N'https://images.unsplash.com/photo-1514525253161-7a46d19cd819?auto=format&fit=crop&w=800&q=80',
  N'Hanoi Opera House, Hoan Kiem District, Hanoi',
  21.0244, 105.8574,
  '2026-10-25 19:30', '2026-10-25 22:00', 'Approved', NULL, '2026-07-17 16:00', 17
),
-- 18: Far Future (Workshop - 08/11/2026) - Conversation 18
(
  11, 3,
  N'Tech Masterclass: System Architecture & Cloud Native',
  N'Deep dive into Kubernetes, Distributed Systems, CQRS, and High Availability System Architecture.',
  N'https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?auto=format&fit=crop&w=800&q=80',
  N'Saigon Innovation Hub, District 3, Ho Chi Minh City',
  10.7812, 106.6895,
  '2026-11-08 08:30', '2026-11-08 17:00', 'Approved', NULL, '2026-07-18 09:00', 18
),
-- 19: Far Future (Entertainment - 15/11/2026) - Conversation 19
(
  6, 6,
  N'Saigon Coffee & Bakery Cultural Expo 2026',
  N'A aromatic celebration of Vietnamese coffee culture, specialty roasts, artisanal pastries, and barista championships.',
  N'https://images.unsplash.com/photo-1447933601403-0c6688de566e?auto=format&fit=crop&w=800&q=80',
  N'Youth Cultural House HCMC, District 1, Ho Chi Minh City',
  10.7811, 106.6985,
  '2026-11-15 08:00', '2026-11-17 21:00', 'Approved', NULL, '2026-07-18 10:00', 19
),
-- 20: Far Future (Sports - 20/11/2026) - Conversation 20
(
  11, 5,
  N'National Badminton Open Championship 2026',
  N'The annual national badminton tournament featuring top professional players competing across singles and doubles categories.',
  N'https://images.unsplash.com/photo-1626224583764-f87db24ac4ea?auto=format&fit=crop&w=800&q=80',
  N'Phu Tho Indoor Stadium, District 11, Ho Chi Minh City',
  10.7681, 106.6579,
  '2026-11-20 08:00', '2026-11-22 20:00', 'Approved', NULL, '2026-07-18 11:00', 20
),

-- 21: Pending Moderation (Education)
(
  3, 7,
  N'Global Study Abroad & Tech Career Expo 2026',
  N'An educational fair providing scholarship advisory for US, European, and Australian universities and IT career roadmaps.',
  N'https://images.unsplash.com/photo-1523240795612-9a054b0db644?auto=format&fit=crop&w=800&q=80',
  N'Landmark 81, Binh Thanh District, Ho Chi Minh City',
  10.7951, 106.7218,
  '2026-10-20 08:00', '2026-10-20 16:30', 'Pending', NULL, '2026-07-18 10:00', NULL
),
-- 22: Pending Moderation (Conference)
(
  6, 2,
  N'Vietnam Blockchain & Web3 Summit 2026',
  N'A technical conference exploring blockchain scaling, smart contract auditing, and Decentralized Finance (DeFi) ecosystems.',
  N'https://images.unsplash.com/photo-1639762681485-074b7f938ba0?auto=format&fit=crop&w=800&q=80',
  N'Pullman Saigon Centre, District 1, Ho Chi Minh City',
  10.7634, 106.6917,
  '2026-11-05 08:30', '2026-11-05 17:00', 'Pending', NULL, '2026-07-19 11:00', NULL
),
-- 23: Rejected Demo Event (Sports)
(
  3, 5,
  N'Off-Road Motorcross Extreme Championship 2026',
  N'An extreme off-road dirt bike racing tournament designed for semi-professional thrill-seeking riders.',
  N'https://images.unsplash.com/photo-1568605117036-5fe5e7bab0b7?auto=format&fit=crop&w=800&q=80',
  N'Vung Tau Tourist Park, Vung Tau City, Ba Ria - Vung Tau',
  10.3460, 107.0843,
  '2026-11-15 08:00', '2026-11-15 17:00', 'Rejected', N'Địa điểm chưa đảm bảo an toàn phòng cháy chữa cháy và di tản emergency.', '2026-07-19 14:00', NULL
),
-- 24: Draft Event (Workshop)
(
  3, 3,
  N'AI Coding & Automation Masterclass 2026',
  N'A practical draft workshop on modern software development workflows using Generative AI tools and autonomous coding assistants.',
  N'https://images.unsplash.com/photo-1531403009284-440f080d1e12?auto=format&fit=crop&w=800&q=80',
  N'Innovation Hub Saigon, District 3, Ho Chi Minh City',
  10.7800, 106.6900,
  '2026-12-01 09:00', '2026-12-01 17:00', 'Draft', NULL, '2026-07-20 08:00', NULL
),
-- 25: Draft Event (Community)
(
  11, 8,
  N'Green Living & Sustainable Energy Workshop',
  N'Community sharing session on eco-friendly lifestyle, solar rooftop setups, and green urban mobility soluciones.',
  N'https://images.unsplash.com/photo-1497435334941-8c899ee9e8e9?auto=format&fit=crop&w=800&q=80',
  N'Danang Green Hub, Hai Chau District, Da Nang',
  16.0600, 108.2200,
  '2026-12-10 09:00', '2026-12-10 16:00', 'Draft', NULL, '2026-07-20 09:00', NULL
);
GO

/* ==================================================
   SEED DATA: TicketTypes
   ================================================== */

INSERT INTO TicketTypes 
(EventId, TypeName, Price, Quantity, SoldQuantity, ColorCode, SaleStartDate, SaleEndDate)
VALUES

-- Event 1 (Past)
(1, N'General Admission Pass', 150000, 500, 3, '#4CAF50', '2026-05-01 00:00', '2026-06-15 17:00'),
(1, N'VIP FanZone Pass', 350000, 100, 1, '#FFC107', '2026-05-01 00:00', '2026-06-15 17:00'),

-- Event 2 (Past)
(2, N'Standard Delegate Pass', 500000, 300, 2, '#2196F3', '2026-06-01 00:00', '2026-07-05 08:30'),
(2, N'VIP Executive Pass', 900000, 100, 1, '#9C27B0', '2026-06-01 00:00', '2026-07-05 08:30'),

-- Event 3 (Past)
(3, N'Standard Seating Ticket', 120000, 200, 2, '#10B981', '2026-04-10 00:00', '2026-05-10 18:00'),
(3, N'Front Row VIP Ticket', 300000, 50, 1, '#F59E0B', '2026-04-10 00:00', '2026-05-10 18:00'),

-- Event 4 (Past)
(4, N'General Foodie Pass', 50000, 400, 3, '#3B82F6', '2026-05-20 00:00', '2026-06-20 10:00'),

-- Event 5 (TODAY DEMO CHECKIN)
(5, N'Standard Pass (Includes 1 Drink)', 250000, 120, 4, '#6366F1', '2026-07-01 00:00', '2026-07-22 19:00'),
(5, N'Couples Front Row Seat', 550000, 40, 2, '#EC4899', '2026-07-01 00:00', '2026-07-22 19:00'),

-- Event 6 (TODAY DEMO CHECKIN)
(6, N'Festival Entry Pass', 80000, 600, 3, '#06B6D4', '2026-07-10 00:00', '2026-07-23 09:00'),
(6, N'VIP Experience & Workshop Pass', 220000, 150, 1, '#8B5CF6', '2026-07-10 00:00', '2026-07-23 09:00'),

-- Event 7 (Near Future - Sports)
(7, N'5KM Fun Run Entry', 200000, 500, 2, '#10B981', '2026-07-15 00:00', '2026-08-07 23:59'),
(7, N'21KM Half Marathon Runner Kit', 450000, 300, 2, '#EF4444', '2026-07-15 00:00', '2026-08-07 23:59'),

-- Event 8 (Future)
(8, N'Standard Delegate Ticket', 150000, 200, 1, '#6B7280', '2026-07-20 00:00', '2026-08-12 12:00'),
(8, N'Investor VIP & Networking Gala', 1200000, 30, 0, '#F59E0B', '2026-07-20 00:00', '2026-08-12 12:00'),

-- Event 9 (Future)
(9, N'1-Day Expo Pass', 100000, 1000, 3, '#10B981', '2026-07-15 00:00', '2026-08-16 12:00'),
(9, N'eSports VIP Finals Arena Seat', 350000, 200, 1, '#F59E0B', '2026-07-15 00:00', '2026-08-16 12:00'),

-- Event 10 (Future)
(10, N'5KM Run Kit (Includes T-Shirt)', 180000, 500, 2, '#3B82F6', '2026-07-20 00:00', '2026-08-24 23:59'),
(10, N'10KM Run Kit (Includes Medal)', 280000, 300, 1, '#8B5CF6', '2026-07-20 00:00', '2026-08-24 23:59'),

-- Event 11 (Future)
(11, N'Red Carpet Runway Ticket', 300000, 150, 2, '#EC4899', '2026-08-01 00:00', '2026-08-28 17:00'),
(11, N'Front Row VIP Seat', 650000, 50, 1, '#8B5CF6', '2026-08-01 00:00', '2026-08-28 17:00'),

-- Event 12 (Mid Future)
(12, N'GA Beach Party Pass', 200000, 800, 2, '#06B6D4', '2026-08-10 00:00', '2026-09-02 16:00'),
(12, N'VIP Lounge & Freeflow Access', 850000, 100, 1, '#F59E0B', '2026-08-10 00:00', '2026-09-02 16:00'),

-- Event 13 (Mid Future)
(13, N'General Admission Gallery Ticket', 80000, 300, 2, '#06B6D4', '2026-08-01 00:00', '2026-09-12 17:00'),

-- Event 14 (Mid Future)
(14, N'Full Workshop Pass', 150000, 100, 1, '#10B981', '2026-08-15 00:00', '2026-09-18 08:00'),

-- Event 15 (Far Future)
(15, N'Student Discount Workshop Pass', 150000, 150, 1, '#84CC16', '2026-08-15 00:00', '2026-10-05 13:00'),
(15, N'Professional & Corporate Pass', 450000, 50, 1, '#D97706', '2026-08-15 00:00', '2026-10-05 13:00'),

-- Event 16 (Far Future)
(16, N'All-Access Forum Pass', 400000, 250, 1, '#3B82F6', '2026-09-01 00:00', '2026-10-12 08:00'),

-- Event 17 (Far Future)
(17, N'Balcony Seating Pass', 350000, 200, 2, '#8B5CF6', '2026-09-15 00:00', '2026-10-25 19:00'),
(17, N'Orchestra VIP Seat', 800000, 80, 1, '#F59E0B', '2026-09-15 00:00', '2026-10-25 19:00'),

-- Event 18 (Far Future)
(18, N'Masterclass General Pass', 250000, 150, 1, '#2563EB', '2026-09-01 00:00', '2026-11-08 08:00'),

-- Event 19 (Far Future)
(19, N'Coffee Expo General Ticket', 60000, 500, 3, '#7C2D12', '2026-09-15 00:00', '2026-11-15 08:00'),

-- Event 20 (Far Future)
(20, N'Badminton Tournament Spectator Pass', 100000, 400, 1, '#059669', '2026-09-20 00:00', '2026-11-20 08:00'),

-- Event 21 (Pending)
(21, N'Free Registration Ticket', 0, 200, 0, '#6B7280', '2026-09-01 00:00', '2026-10-20 08:00'),

-- Event 22 (Pending)
(22, N'Blockchain Delegate Pass', 500000, 150, 0, '#6366F1', '2026-09-20 00:00', '2026-11-05 08:30'),

-- Event 23 (Rejected)
(23, N'Vé Đua Xe Thường', 200000, 100, 0, '#EF4444', '2026-10-01 00:00', '2026-11-15 08:00'),

-- Event 24 (Draft)
(24, N'Workshop Early Bird Pass', 300000, 50, 0, '#10B981', '2026-10-01 00:00', '2026-12-01 08:00'),

-- Event 25 (Draft)
(25, N'Green Energy Free Ticket', 0, 100, 0, '#16A34A', '2026-10-15 00:00', '2026-12-10 08:00');
GO

/* ==================================================
   SEED DATA: EventStaff (Validator Assignments)
   ================================================== */

INSERT INTO EventStaff
(EventId, StaffId, StaffEmail, ValidatorRole, ExpiredAt, IsActive)
VALUES
(1, 4, 'validator@auth.com', 'Validator', '2026-06-15 23:59', 1),
(2, 4, 'validator@auth.com', 'Validator', '2026-07-05 23:59', 1),
(3, 4, 'validator@auth.com', 'Validator', '2026-05-10 23:59', 1),
(4, 4, 'validator@auth.com', 'Validator', '2026-06-22 23:59', 1),
-- Event 5 (TODAY): Staff 4 (validator@auth.com) assigned today!
(5, 4, 'validator@auth.com', 'Validator', '2026-07-22 23:59', 1),
-- Event 6 (TODAY): Staff 7 (validator2@auth.com) assigned today!
(6, 7, 'validator2@auth.com', 'Validator', '2026-07-23 23:59', 1),

(7, 7, 'validator2@auth.com', 'Validator', '2026-08-08 23:59', 1),
(8, 7, 'validator2@auth.com', 'Validator', '2026-08-12 23:59', 1),
(9, 7, 'validator2@auth.com', 'Validator', '2026-08-16 23:59', 1),
(10, 7, 'validator2@auth.com', 'Validator', '2026-08-25 23:59', 1),
(11, 12, 'validator3@auth.com', 'Validator', '2026-08-30 23:59', 1),
(12, 12, 'validator3@auth.com', 'Validator', '2026-09-02 23:59', 1),
(15, 12, 'validator3@auth.com', 'Validator', '2026-10-05 23:59', 1),
(18, 12, 'validator3@auth.com', 'Validator', '2026-11-08 23:59', 1);
GO

