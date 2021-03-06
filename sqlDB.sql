USE [CreditDataBase]
GO
/****** Object:  Table [dbo].[CreditApps]    Script Date: 27.09.2021 22:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CreditApps](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[Date] [date] NOT NULL,
	[Summ] [decimal](18, 2) NOT NULL,
	[Target] [nvarchar](10) NOT NULL,
	[Period] [int] NOT NULL,
	[Status] [bit] NOT NULL,
 CONSTRAINT [PK_CreditApps] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Credits]    Script Date: 27.09.2021 22:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Credits](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[AppID] [int] NOT NULL,
	[UpdatedDate] [date] NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[Delays] [int] NOT NULL,
	[Status] [bit] NOT NULL,
 CONSTRAINT [PK_Credits] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Forms]    Script Date: 27.09.2021 22:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Forms](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[Gender] [nvarchar](10) NOT NULL,
	[FamilyStatus] [nvarchar](10) NOT NULL,
	[Age] [int] NOT NULL,
	[Citizenship] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_Forms] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 27.09.2021 22:45:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](10) NOT NULL,
	[Phone] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](50) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[SurName] [nvarchar](50) NOT NULL,
	[MiddleName] [nvarchar](50) NOT NULL,
	[BirthDate] [date] NOT NULL,
	[Addres] [nvarchar](50) NOT NULL,
	[Creditibility] [int] NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[CreditApps] ON 

INSERT [dbo].[CreditApps] ([ID], [UserID], [Date], [Summ], [Target], [Period], [Status]) VALUES (5, 3, CAST(N'2021-09-27' AS Date), CAST(6000.00 AS Decimal(18, 2)), N'Appliances', 12, 1)
SET IDENTITY_INSERT [dbo].[CreditApps] OFF
GO
SET IDENTITY_INSERT [dbo].[Credits] ON 

INSERT [dbo].[Credits] ([ID], [UserID], [AppID], [UpdatedDate], [Balance], [Delays], [Status]) VALUES (1, 3, 5, CAST(N'2021-09-27' AS Date), CAST(6000.00 AS Decimal(18, 2)), 0, 1)
SET IDENTITY_INSERT [dbo].[Credits] OFF
GO
SET IDENTITY_INSERT [dbo].[Forms] ON 

INSERT [dbo].[Forms] ([ID], [UserID], [Gender], [FamilyStatus], [Age], [Citizenship]) VALUES (1, 2, N'Man', N'Single', 21, N'Tajikistan')
INSERT [dbo].[Forms] ([ID], [UserID], [Gender], [FamilyStatus], [Age], [Citizenship]) VALUES (2, 3, N'Man', N'FamilyMan', 37, N'Tajikistan')
SET IDENTITY_INSERT [dbo].[Forms] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([ID], [Type], [Phone], [Password], [FirstName], [SurName], [MiddleName], [BirthDate], [Addres], [Creditibility]) VALUES (1, N'Admin', N'900006644', N'admin2001', N'Karim', N'Karimov', N'Karimovich', CAST(N'2001-02-02' AS Date), N'Dushanbe', NULL)
INSERT [dbo].[Users] ([ID], [Type], [Phone], [Password], [FirstName], [SurName], [MiddleName], [BirthDate], [Addres], [Creditibility]) VALUES (2, N'Client', N'904047714', N'client1989', N'Osim', N'Osimov', N'Osimovich', CAST(N'1989-01-01' AS Date), N'Vahdat', 3)
INSERT [dbo].[Users] ([ID], [Type], [Phone], [Password], [FirstName], [SurName], [MiddleName], [BirthDate], [Addres], [Creditibility]) VALUES (3, N'Client', N'902970987', N'client2000', N'Hilol', N'Hilolov', N'Hilolovich', CAST(N'2000-12-07' AS Date), N'Dushnabe', 6)
INSERT [dbo].[Users] ([ID], [Type], [Phone], [Password], [FirstName], [SurName], [MiddleName], [BirthDate], [Addres], [Creditibility]) VALUES (8, N'Client', N'900667788', N'murod2002', N'Murod', N'Murodov', N'Murodovich', CAST(N'2002-04-04' AS Date), N'Kulob', NULL)
INSERT [dbo].[Users] ([ID], [Type], [Phone], [Password], [FirstName], [SurName], [MiddleName], [BirthDate], [Addres], [Creditibility]) VALUES (9, N'Client', N'908778877', N'komil19999', N'Komil', N'Komilov', N'Komilovich', CAST(N'1999-09-09' AS Date), N'Dushanbe', NULL)
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Creditworthiness]  DEFAULT (NULL) FOR [Creditibility]
GO
ALTER TABLE [dbo].[CreditApps]  WITH CHECK ADD  CONSTRAINT [FK_CreditApps_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[CreditApps] CHECK CONSTRAINT [FK_CreditApps_Users]
GO
ALTER TABLE [dbo].[Credits]  WITH CHECK ADD  CONSTRAINT [FK_Credits_CreditApps] FOREIGN KEY([AppID])
REFERENCES [dbo].[CreditApps] ([ID])
GO
ALTER TABLE [dbo].[Credits] CHECK CONSTRAINT [FK_Credits_CreditApps]
GO
ALTER TABLE [dbo].[Credits]  WITH CHECK ADD  CONSTRAINT [FK_Credits_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Credits] CHECK CONSTRAINT [FK_Credits_Users]
GO
ALTER TABLE [dbo].[Forms]  WITH CHECK ADD  CONSTRAINT [FK_Forms_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Forms] CHECK CONSTRAINT [FK_Forms_Users]
GO
