SELECT Id, Username, Name, Role, IsActive, IsApproved
FROM [User]
WHERE Role = 'Admin';

SELECT Id, Username, Name, Role, IsActive, IsApproved
FROM [User]
WHERE Role = 'Child';

SELECT Id, Username, Name, Role, IsActive, IsApproved
FROM [User]
WHERE Role = 'Parent';

SELECT Id, Username, Name, Role, IsActive, IsApproved
FROM [User]
WHERE Role = 'Teacher';