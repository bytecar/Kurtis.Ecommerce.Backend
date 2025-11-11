-- ============================================================
-- KURTIS & MORE - DATABASE INITIAL SCHEMA (DDL)
-- Target: Microsoft SQL Server 2022
-- ============================================================

CREATE DATABASE KurtisDB;
GO
USE KurtisDB;
GO

-- Drop existing tables (for clean init)
IF OBJECT_ID('dbo.ProductCollections', 'U') IS NOT NULL DROP TABLE dbo.ProductCollections;
IF OBJECT_ID('dbo.Inventories', 'U') IS NOT NULL DROP TABLE dbo.Inventories;
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Brands', 'U') IS NOT NULL DROP TABLE dbo.Brands;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Collections', 'U') IS NOT NULL DROP TABLE dbo.Collections;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Brands (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Label NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE dbo.Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Label NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE dbo.Collections (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL
);
GO

CREATE TABLE dbo.Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(18,2) NOT NULL,
    DiscountedPrice DECIMAL(18,2) NULL,
    BrandId INT NOT NULL,
    CategoryId INT NOT NULL,
    Gender NVARCHAR(50) NULL,
    SizesJson NVARCHAR(MAX) NULL,
    AverageRating FLOAT NULL,
    RatingCount INT NULL DEFAULT 0,
    Featured BIT NOT NULL DEFAULT 0,
    IsNew BIT NOT NULL DEFAULT 0,
    ImageUrlsJson NVARCHAR(MAX) NULL,    
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (BrandId) REFERENCES dbo.Brands(Id),
    FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
);
GO

CREATE TABLE dbo.Inventories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    Size NVARCHAR(10) NOT NULL,
    Quantity INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.ProductCollections (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    CollectionId INT NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (CollectionId) REFERENCES dbo.Collections(Id) ON DELETE CASCADE
);
GO
