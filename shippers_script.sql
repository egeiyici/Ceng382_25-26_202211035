USE Northwind;
GO

IF OBJECT_ID('dbo.ShippersContactInfo', 'U') IS NOT NULL
    DROP TABLE dbo.ShippersContactInfo;
GO

CREATE TABLE dbo.ShippersContactInfo
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ShipperId INT NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Website NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(30) NOT NULL,
    Address NVARCHAR(200) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    PostCode NVARCHAR(20) NOT NULL
);
GO

INSERT INTO dbo.ShippersContactInfo
(ShipperId, Email, Website, Phone, Address, City, Country, PostCode)
VALUES
(1, 'contact@speedyexpress.com', 'https://speedyexpress.com', '(503) 555-9831', '123 Fast Ave.', 'London', 'UK', 'EC1A 1BB'),
(2, 'info@unitedpackage.com', 'https://unitedpackage.com', '(503) 555-3199', '456 Pack St.', 'Berlin', 'Germany', '10115'),
(3, 'support@federalshipping.com', 'https://federalshipping.com', '(503) 555-9931', '789 Ship Blvd.', 'New York', 'USA', '10001');
GO

SELECT * FROM dbo.ShippersContactInfo;
GO