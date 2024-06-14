-- Create Users Table
CREATE TABLE Users (
    UserId int NOT NULL IDENTITY PRIMARY KEY,
    Nickname nvarchar(256) NULL,
    Email nvarchar(256) NOT NULL,
    EmailConfirmed bit NOT NULL,
    PasswordHash nvarchar(max) NULL,
    SecurityStamp nvarchar(max) NULL,
    FullName nvarchar(max) NULL,
    Avatar nvarchar(max) NULL,
    EmailConfirmationToken nvarchar(max) NULL,
    ResetPasswordToken nvarchar(max) NULL,
    RefreshToken nvarchar(max) NULL,
    CONSTRAINT IX_Users_Email UNIQUE (Email)
);

-- Create Rooms Table
CREATE TABLE Rooms (
    RoomId int NOT NULL IDENTITY PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    AdminId int NOT NULL,
    CONSTRAINT FK_Rooms_Users FOREIGN KEY (AdminId) REFERENCES Users (UserId) ON DELETE CASCADE,
    CONSTRAINT IX_Rooms_Name UNIQUE (Name)
);

CREATE TABLE RoomUsers (
    UserId int NOT NULL,
    RoomId int NOT NULL,
    IsModerator bit NOT NULL DEFAULT 0,
    IsMember bit NOT NULL DEFAULT 1,
    CONSTRAINT PK_RoomUsers PRIMARY KEY (UserId, RoomId),
    CONSTRAINT FK_RoomUsers_Users FOREIGN KEY (UserId) REFERENCES Users (UserId) ON DELETE NO ACTION, -- Changed to NO ACTION
    CONSTRAINT FK_RoomUsers_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms (RoomId) ON DELETE CASCADE -- Keep cascade delete for rooms
);



-- Create Messages Table
CREATE TABLE Messages (
    MessageId int NOT NULL IDENTITY PRIMARY KEY,
    Content nvarchar(500) NOT NULL,
    Timestamp datetime2 NOT NULL,
    UserId int NULL,
    RoomId int NOT NULL,
    CONSTRAINT FK_Messages_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (UserId) ON DELETE NO ACTION,
    CONSTRAINT FK_Messages_Rooms_RoomId FOREIGN KEY (RoomId) REFERENCES Rooms (RoomId) ON DELETE CASCADE,
    INDEX IX_Messages_UserId (UserId),
    INDEX IX_Messages_RoomId (RoomId)
);
