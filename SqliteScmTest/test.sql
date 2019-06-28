CREATE TABLE PartType
(
    Id   INTEGER PRIMARY KEY,
    Name VARCHAR(255) NOT NULL
);

INSERT INTO PartType
    (NAME)
VALUES ('8289 L-shaped plate');

SELECT Id, Name
FROM PartType;

CREATE TABLE PartType
(
    Id   INTEGER PRIMARY KEY,
    Name VARCHAR(255) NOT NULL
);

CREATE TABLE InventoryItem
(
    PartTypeId     INTEGER PRIMARY KEY,
    Count          INTEGER NOT NULL,
    OrderThreshold INTEGER,
    FOREIGN KEY (PartTypeId) REFERENCES PartType (Id)
);

INSERT INTO PartType
    (Id, Name)
VALUES (0, '8289 L-shaped plate');

INSERT INTO InventoryItem
    (PartTypeId, Count, OrderThreshold)
VALUES (0, 100, 10);

SELECT PartTypeId, Count, OrderThreshold
FROM InventoryItem;

CREATE TABLE PartCommand
(
    Id         INTEGER PRIMARY KEY,
    PartTypeId INTEGER KEY,
    Count      INTEGER      NOT NULL,
    Command    VARCHAR(255) NOT NULL,
    FOREIGN KEY (PartTypeId) REFERENCES PartType (Id)
);

INSERT INTO PartCommand (PartTypeId, Count, Command)
VALUES (@partTypeId, @partCount, @command);
SELECT last_insert_rowid();

SELECT Id, PartTypeId, Count, Command
FROM PartCommand
ORDER BY Id;

UPDATE InventoryItem
SET Count=@count
WHERE PartTypeId = @partTypeId;

DELETE
FROM PartCommand
WHERE Id = @id;

CREATE TABLE Supplier
(
    Id         INTEGER PRIMARY KEY,
    Name       VARCHAR(255) NOT NULL,
    Email      VARCHAR(255) NOT NULL,
    PartTypeId INTEGER      NOT NULL,
    FOREIGN KEY (PartTypeId) REFERENCES PartType (Id)
);

CREATE TABLE [Order]
(
    Id            INTEGER PRIMARY KEY,
    SupplierId    INTEGER  NOT NULL,
    PartTypeId    INTEGER  NOT NULL,
    PartCount     INTEGER  NOT NULL,
    PlacedDate    DATETIME NOT NULL,
    FulfilledDate DATETIME,
    FOREIGN KEY (PartTypeId) REFERENCES PartType (Id),
    FOREIGN KEY (SupplierId) REFERENCES Supplier (Id)
);

CREATE TABLE SendEmailCommand
(
    Id      INTEGER PRIMARY KEY,
    [To]    VARCHAR(255) NOT NULL,
    Subject VARCHAR(255) NOT NULL,
    Body    BLOB
);

INSERT INTO [Order] (SupplierId, PartTypeId, PartCount, PlacedDate)
VALUES (@supplierId, @partTypeId, @partCount, @placedDate);
SELECT last_insert_rowid();

INSERT INTO SendEmailCommand ([To], Subject, Body)
VALUES (@To, @Subject, @Body);

INSERT INTO Supplier
    (Name, Email, PartTypeId)
VALUES ('Joe Supplier', 'joe@joesupplier.com', 0);

SELECT Id, Name, Email, PartTypeId
FROM Supplier;

SELECT Count(*)
FROM [Order]
WHERE SupplierId = @supplierId
  AND PartTypeId = @partTypeId
  AND PlacedDate = @placedDate
  AND PartCount = 10
  AND FulfilledDate IS NULL;

SELECT (Id, SupplierId, PartTypeId, PartCount, PlacedDate, FulfilledDate)
FROM [Order]