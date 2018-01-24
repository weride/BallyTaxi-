
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/21/2016 22:09:33
-- Generated from EDMX file: C:\Dropbox\Projects\Quickode.BallyTaxi\Quickode.BallyTaxi.Models\Model.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [BallyTaxi];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Driver_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Driver] DROP CONSTRAINT [FK_Driver_User];
GO
IF OBJECT_ID(N'[dbo].[FK_FavoriteAddresses_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FavoriteAddresses] DROP CONSTRAINT [FK_FavoriteAddresses_User];
GO
IF OBJECT_ID(N'[dbo].[FK_FavoriteDrivers_Driver]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FavoriteDrivers] DROP CONSTRAINT [FK_FavoriteDrivers_Driver];
GO
IF OBJECT_ID(N'[dbo].[FK_FavoriteDrivers_Passenger]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FavoriteDrivers] DROP CONSTRAINT [FK_FavoriteDrivers_Passenger];
GO
IF OBJECT_ID(N'[dbo].[FK_Orders_Driver]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_Orders_Driver];
GO
IF OBJECT_ID(N'[dbo].[FK_Orders_Drivers_Driver]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Orders_Drivers] DROP CONSTRAINT [FK_Orders_Drivers_Driver];
GO
IF OBJECT_ID(N'[dbo].[FK_Orders_Drivers_Orders]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Orders_Drivers] DROP CONSTRAINT [FK_Orders_Drivers_Orders];
GO
IF OBJECT_ID(N'[dbo].[FK_Orders_Passenger]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_Orders_Passenger];
GO
IF OBJECT_ID(N'[dbo].[FK_Passenger_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Passenger] DROP CONSTRAINT [FK_Passenger_User];
GO
IF OBJECT_ID(N'[dbo].[FK_User_Images]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[User] DROP CONSTRAINT [FK_User_Images];
GO
IF OBJECT_ID(N'[dbo].[FK_User_Languages]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[User] DROP CONSTRAINT [FK_User_Languages];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[CountryCodes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CountryCodes];
GO
IF OBJECT_ID(N'[dbo].[Driver]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Driver];
GO
IF OBJECT_ID(N'[dbo].[FavoriteAddresses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FavoriteAddresses];
GO
IF OBJECT_ID(N'[dbo].[FavoriteDrivers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FavoriteDrivers];
GO
IF OBJECT_ID(N'[dbo].[Images]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Images];
GO
IF OBJECT_ID(N'[dbo].[Languages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Languages];
GO
IF OBJECT_ID(N'[dbo].[Orders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Orders];
GO
IF OBJECT_ID(N'[dbo].[Orders_Drivers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Orders_Drivers];
GO
IF OBJECT_ID(N'[dbo].[Passenger]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Passenger];
GO
IF OBJECT_ID(N'[dbo].[PendingUser]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PendingUser];
GO
IF OBJECT_ID(N'[dbo].[SupportedCountries]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SupportedCountries];
GO
IF OBJECT_ID(N'[dbo].[User]', 'U') IS NOT NULL
    DROP TABLE [dbo].[User];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CountryCodes'
CREATE TABLE [dbo].[CountryCodes] (
    [name] varchar(44)  NOT NULL,
    [name_fr] varchar(44)  NULL,
    [ISO31661Alpha2] varchar(2)  NOT NULL,
    [ISO31661Alpha3] varchar(3)  NULL,
    [ISO31661numeric] varchar(3)  NULL,
    [ITU] varchar(3)  NULL,
    [MARC] varchar(14)  NULL,
    [WMO] varchar(2)  NULL,
    [DS] varchar(3)  NULL,
    [Dial] varchar(17)  NULL,
    [FIFA] varchar(15)  NULL,
    [FIPS] varchar(26)  NULL,
    [GAUL] varchar(6)  NULL,
    [IOC] varchar(3)  NULL,
    [currency_alphabetic_code] varchar(3)  NULL,
    [currency_country_name] varchar(44)  NULL,
    [currency_minor_unit] varchar(1)  NULL,
    [currency_name] varchar(29)  NULL,
    [currency_numeric_code] varchar(3)  NULL,
    [is_independent] varchar(22)  NULL
);
GO

-- Creating table 'PendingUsers'
CREATE TABLE [dbo].[PendingUsers] (
    [PendingUserId] int IDENTITY(1,1) NOT NULL,
    [CodeValidation] nvarchar(4)  NOT NULL,
    [Phone] nvarchar(50)  NOT NULL,
    [RegistrationDate] datetime  NOT NULL,
    [CodeSent] bit  NOT NULL,
    [CodeExpiration] datetime  NOT NULL
);
GO

-- Creating table 'Images'
CREATE TABLE [dbo].[Images] (
    [ImageId] uniqueidentifier  NOT NULL,
    [Extension] varchar(10)  NOT NULL
);
GO

-- Creating table 'Orders'
CREATE TABLE [dbo].[Orders] (
    [OrderId] bigint IDENTITY(1,1) NOT NULL,
    [PickUpLocation] geography  NOT NULL,
    [DestinationLocation] geography  NULL,
    [PickUpAddress] nvarchar(250)  NOT NULL,
    [DestinationAddress] nvarchar(250)  NULL,
    [Notes] nvarchar(max)  NULL,
    [OrderTime] datetime  NULL,
    [PassengerId] bigint  NOT NULL,
    [DriverId] bigint  NULL,
    [StatusId] int  NOT NULL,
    [CreationDate] datetime  NOT NULL,
    [FlowStep] int  NULL,
    [LastUpdateFlowStep] datetime  NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [UserId] bigint IDENTITY(1,1) NOT NULL,
    [RegistrationDate] datetime  NOT NULL,
    [Phone] nvarchar(50)  NOT NULL,
    [Email] nvarchar(50)  NULL,
    [DeviceId] nvarchar(100)  NULL,
    [NotificationToken] nvarchar(200)  NULL,
    [PlatformId] int  NULL,
    [VersionOS] nvarchar(50)  NULL,
    [AuthenticationToken] nvarchar(200)  NOT NULL,
    [Active] bit  NOT NULL,
    [Name] nvarchar(50)  NULL,
    [AppVersion] nvarchar(50)  NULL,
    [ImageId] uniqueidentifier  NULL,
    [LanguageId] int  NOT NULL
);
GO

-- Creating table 'SupportedCountries'
CREATE TABLE [dbo].[SupportedCountries] (
    [ISOcode] varchar(2)  NOT NULL,
    [IsSupported] bit  NOT NULL
);
GO

-- Creating table 'Languages'
CREATE TABLE [dbo].[Languages] (
    [LanguageId] int IDENTITY(1,1) NOT NULL,
    [LanguageName] nvarchar(50)  NOT NULL,
    [LanguageCulture] nvarchar(10)  NULL
);
GO

-- Creating table 'Orders_Drivers'
CREATE TABLE [dbo].[Orders_Drivers] (
    [OrderId] bigint  NOT NULL,
    [DriverId] bigint  NOT NULL,
    [StatusId] int  NULL
);
GO

-- Creating table 'Drivers'
CREATE TABLE [dbo].[Drivers] (
    [UserId] bigint  NOT NULL,
    [LicensePlate] nvarchar(50)  NULL,
    [Location] geography  NULL,
    [LastUpdateLocation] datetime  NULL,
    [CarType] nvarchar(100)  NULL,
    [TaxiLicense] nvarchar(50)  NULL,
    [PaymentStatus] int  NULL,
    [WantFutureRide] bit  NOT NULL,
    [Status] int  NULL
);
GO

-- Creating table 'Passengers'
CREATE TABLE [dbo].[Passengers] (
    [UserId] bigint  NOT NULL
);
GO

-- Creating table 'FavoriteDrivers'
CREATE TABLE [dbo].[FavoriteDrivers] (
    [PassengerId] bigint  NOT NULL,
    [DriverId] bigint  NOT NULL,
    [Notes] nvarchar(500)  NULL,
    [CreationDate] datetime  NULL
);
GO

-- Creating table 'FavoriteAddresses'
CREATE TABLE [dbo].[FavoriteAddresses] (
    [PassengerId] bigint  NOT NULL,
    [PickUpLocation] geography  NOT NULL,
    [DestinationLocation] geography  NULL,
    [PickUpAddress] nvarchar(250)  NOT NULL,
    [DestinationAddress] nvarchar(250)  NULL,
    [FavoriteIndex] bigint IDENTITY(1,1) NOT NULL,
    [CreationDate] datetime  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [name], [ISO31661Alpha2] in table 'CountryCodes'
ALTER TABLE [dbo].[CountryCodes]
ADD CONSTRAINT [PK_CountryCodes]
    PRIMARY KEY CLUSTERED ([name], [ISO31661Alpha2] ASC);
GO

-- Creating primary key on [PendingUserId] in table 'PendingUsers'
ALTER TABLE [dbo].[PendingUsers]
ADD CONSTRAINT [PK_PendingUsers]
    PRIMARY KEY CLUSTERED ([PendingUserId] ASC);
GO

-- Creating primary key on [ImageId] in table 'Images'
ALTER TABLE [dbo].[Images]
ADD CONSTRAINT [PK_Images]
    PRIMARY KEY CLUSTERED ([ImageId] ASC);
GO

-- Creating primary key on [OrderId] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [PK_Orders]
    PRIMARY KEY CLUSTERED ([OrderId] ASC);
GO

-- Creating primary key on [UserId] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [ISOcode] in table 'SupportedCountries'
ALTER TABLE [dbo].[SupportedCountries]
ADD CONSTRAINT [PK_SupportedCountries]
    PRIMARY KEY CLUSTERED ([ISOcode] ASC);
GO

-- Creating primary key on [LanguageId] in table 'Languages'
ALTER TABLE [dbo].[Languages]
ADD CONSTRAINT [PK_Languages]
    PRIMARY KEY CLUSTERED ([LanguageId] ASC);
GO

-- Creating primary key on [OrderId], [DriverId] in table 'Orders_Drivers'
ALTER TABLE [dbo].[Orders_Drivers]
ADD CONSTRAINT [PK_Orders_Drivers]
    PRIMARY KEY CLUSTERED ([OrderId], [DriverId] ASC);
GO

-- Creating primary key on [UserId] in table 'Drivers'
ALTER TABLE [dbo].[Drivers]
ADD CONSTRAINT [PK_Drivers]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [UserId] in table 'Passengers'
ALTER TABLE [dbo].[Passengers]
ADD CONSTRAINT [PK_Passengers]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [PassengerId], [DriverId] in table 'FavoriteDrivers'
ALTER TABLE [dbo].[FavoriteDrivers]
ADD CONSTRAINT [PK_FavoriteDrivers]
    PRIMARY KEY CLUSTERED ([PassengerId], [DriverId] ASC);
GO

-- Creating primary key on [PassengerId], [FavoriteIndex] in table 'FavoriteAddresses'
ALTER TABLE [dbo].[FavoriteAddresses]
ADD CONSTRAINT [PK_FavoriteAddresses]
    PRIMARY KEY CLUSTERED ([PassengerId], [FavoriteIndex] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [ImageId] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_User_Images]
    FOREIGN KEY ([ImageId])
    REFERENCES [dbo].[Images]
        ([ImageId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_User_Images'
CREATE INDEX [IX_FK_User_Images]
ON [dbo].[Users]
    ([ImageId]);
GO

-- Creating foreign key on [DriverId] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Driver]
    FOREIGN KEY ([DriverId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_Driver'
CREATE INDEX [IX_FK_Orders_Driver]
ON [dbo].[Orders]
    ([DriverId]);
GO

-- Creating foreign key on [PassengerId] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Passenger]
    FOREIGN KEY ([PassengerId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_Passenger'
CREATE INDEX [IX_FK_Orders_Passenger]
ON [dbo].[Orders]
    ([PassengerId]);
GO

-- Creating foreign key on [LanguageId] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_User_Languages]
    FOREIGN KEY ([LanguageId])
    REFERENCES [dbo].[Languages]
        ([LanguageId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_User_Languages'
CREATE INDEX [IX_FK_User_Languages]
ON [dbo].[Users]
    ([LanguageId]);
GO

-- Creating foreign key on [OrderId] in table 'Orders_Drivers'
ALTER TABLE [dbo].[Orders_Drivers]
ADD CONSTRAINT [FK_Orders_Drivers_Orders]
    FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[Orders]
        ([OrderId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [UserId] in table 'Drivers'
ALTER TABLE [dbo].[Drivers]
ADD CONSTRAINT [FK_Driver_User]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [DriverId] in table 'Orders_Drivers'
ALTER TABLE [dbo].[Orders_Drivers]
ADD CONSTRAINT [FK_Orders_Drivers_Driver]
    FOREIGN KEY ([DriverId])
    REFERENCES [dbo].[Drivers]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_Drivers_Driver'
CREATE INDEX [IX_FK_Orders_Drivers_Driver]
ON [dbo].[Orders_Drivers]
    ([DriverId]);
GO

-- Creating foreign key on [UserId] in table 'Passengers'
ALTER TABLE [dbo].[Passengers]
ADD CONSTRAINT [FK_Passenger_User]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [DriverId] in table 'FavoriteDrivers'
ALTER TABLE [dbo].[FavoriteDrivers]
ADD CONSTRAINT [FK_FavoriteDrivers_Driver]
    FOREIGN KEY ([DriverId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FavoriteDrivers_Driver'
CREATE INDEX [IX_FK_FavoriteDrivers_Driver]
ON [dbo].[FavoriteDrivers]
    ([DriverId]);
GO

-- Creating foreign key on [PassengerId] in table 'FavoriteDrivers'
ALTER TABLE [dbo].[FavoriteDrivers]
ADD CONSTRAINT [FK_FavoriteDrivers_Passenger]
    FOREIGN KEY ([PassengerId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [PassengerId] in table 'FavoriteAddresses'
ALTER TABLE [dbo].[FavoriteAddresses]
ADD CONSTRAINT [FK_FavoriteAddresses_User]
    FOREIGN KEY ([PassengerId])
    REFERENCES [dbo].[Users]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------