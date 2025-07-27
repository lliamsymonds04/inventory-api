CREATE USER [inventory] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [inventory];
ALTER ROLE db_datawriter ADD MEMBER [inventory];