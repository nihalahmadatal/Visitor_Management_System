create table TblVisitor(
		Id int primary key identity(1,1),
		Name nvarchar(100),
		Email nvarchar(50),
		Phone nvarchar(10),
		PhotoPath nvarchar(max),
		Status nvarchar(50),
		QRCodeToken uniqueidentifier,
		CreatedAt datetime,
		UpdatedAt datetime null);

create table TblLogin(
		Id int primary key identity(1,1),
		Username nvarchar(50),
		Password nvarchar(50),
		Role nvarchar(50));

INSERT INTO TblLogin (Username, Password, Role) 
VALUES ('user1', '1234', 'user');