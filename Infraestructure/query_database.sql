USE [s_formularios]
GO
/****** Object:  Schema [Configuracion]    Script Date: 21/1/2026 13:46:19 ******/
CREATE SCHEMA [Configuracion]
GO
/****** Object:  Schema [General]    Script Date: 21/1/2026 13:46:19 ******/
CREATE SCHEMA [General]
GO
/****** Object:  Schema [Seguridad]    Script Date: 21/1/2026 13:46:19 ******/
CREATE SCHEMA [Seguridad]
GO
/****** Object:  Table [Configuracion].[Concepto]    Script Date: 21/1/2026 13:46:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Configuracion].[Concepto](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Tipo] [int] NOT NULL,
	[Clave] [varchar](250) NOT NULL,
	[Nombre] [varchar](250) NOT NULL,
	[Valor] [varchar](250) NOT NULL,
	[Descripcion] [varchar](250) NULL,
	[Secuencia] [int] NOT NULL,
	[Editable] [bit] NULL,
 CONSTRAINT [PK_Concepto] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Configuracion].[Entidad]    Script Date: 21/1/2026 13:46:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Configuracion].[Entidad](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](250) NOT NULL,
	[Descripcion] [varchar](250) NOT NULL,
	[Direccion] [varchar](250) NULL,
	[Latitud] [varchar](250) NULL,
	[Longitud] [varchar](250) NULL,
	[Telefono] [varchar](50) NULL,
	[Correo] [varchar](250) NULL,
	[Eliminado] [bit] NOT NULL,
 CONSTRAINT [PK_Entidad] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Seguridad].[Acceso]    Script Date: 21/1/2026 13:46:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Seguridad].[Acceso](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](200) NOT NULL,
	[Secuencia] [int] NOT NULL,
	[Controlador] [varchar](200) NOT NULL,
	[Vista] [varchar](200) NOT NULL,
	[Url] [varchar](200) NOT NULL,
	[Icono] [varchar](200) NOT NULL,
	[Descripcion] [varchar](200) NOT NULL,
	[ModuloId] [bigint] NOT NULL,
	[Eliminado] [bit] NOT NULL,
 CONSTRAINT [PK_Acceso] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Seguridad].[Modulo]    Script Date: 21/1/2026 13:46:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Seguridad].[Modulo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](150) NOT NULL,
	[Icono] [varchar](100) NOT NULL,
	[Secuencia] [int] NOT NULL,
	[Eliminado] [bit] NOT NULL,
 CONSTRAINT [PK_Modulo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Seguridad].[Perfil]    Script Date: 21/1/2026 13:46:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Seguridad].[Perfil](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](250) NOT NULL,
	[Descripcion] [varchar](250) NOT NULL,
	[Eliminado] [bit] NOT NULL,
 CONSTRAINT [PK_Perfil] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Seguridad].[PerfilAcceso]    Script Date: 21/1/2026 13:46:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Seguridad].[PerfilAcceso](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PerfilId] [bigint] NOT NULL,
	[AccesoId] [bigint] NOT NULL,
	[Eliminado] [bit] NOT NULL,
 CONSTRAINT [PK_PerfilAcceso] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [Seguridad].[Usuario]    Script Date: 21/1/2026 13:46:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Seguridad].[Usuario](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](250) NOT NULL,
	[Apellido] [varchar](250) NOT NULL,
	[Username] [varchar](100) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[Password] [varchar](150) NOT NULL,
	[FechaCreacion] [datetime] NULL,
	[PerfilId] [bigint] NULL,
	[Eliminado] [bit] NOT NULL,
 CONSTRAINT [PK_Usuario] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [Configuracion].[Concepto] ON 
GO
INSERT [Configuracion].[Concepto] ([Id], [Tipo], [Clave], [Nombre], [Valor], [Descripcion], [Secuencia], [Editable]) VALUES (1, 1, N'1', N'Modulo Seguridad', N'Seguridad', N'Modulo seguridad', 1, 1)
GO
INSERT [Configuracion].[Concepto] ([Id], [Tipo], [Clave], [Nombre], [Valor], [Descripcion], [Secuencia], [Editable]) VALUES (2, 1, N'2', N'Modulo General', N'General', N'Modulo general', 2, 1)
GO
SET IDENTITY_INSERT [Configuracion].[Concepto] OFF
GO
SET IDENTITY_INSERT [Configuracion].[Entidad] ON 
GO
INSERT [Configuracion].[Entidad] ([Id], [Nombre], [Descripcion], [Direccion], [Latitud], [Longitud], [Telefono], [Correo], [Eliminado]) VALUES (1, N'string asd ', N'string asda sdasd', N'string', N'string', N'string', N'string asd ', N'string', 0)
GO
INSERT [Configuracion].[Entidad] ([Id], [Nombre], [Descripcion], [Direccion], [Latitud], [Longitud], [Telefono], [Correo], [Eliminado]) VALUES (3, N'asdasdasd', N'asdasdasda sdas dasd', N'', N'', N'', N'a sdasd', N'', 1)
GO
SET IDENTITY_INSERT [Configuracion].[Entidad] OFF
GO
SET IDENTITY_INSERT [Seguridad].[Acceso] ON 
GO
INSERT [Seguridad].[Acceso] ([Id], [Nombre], [Secuencia], [Controlador], [Vista], [Url], [Icono], [Descripcion], [ModuloId], [Eliminado]) VALUES (2, N'Home', 1, N'Home', N'Index', N'/Home/Index', N'fa fa-genderless', N'a', 4, 1)
GO
INSERT [Seguridad].[Acceso] ([Id], [Nombre], [Secuencia], [Controlador], [Vista], [Url], [Icono], [Descripcion], [ModuloId], [Eliminado]) VALUES (3, N'Usuarios', 1, N'Usuario', N'Listado', N'/Usuario/Listado', N'fa fa-genderless', N'a', 3, 0)
GO
INSERT [Seguridad].[Acceso] ([Id], [Nombre], [Secuencia], [Controlador], [Vista], [Url], [Icono], [Descripcion], [ModuloId], [Eliminado]) VALUES (4, N'Perfiles', 2, N'Perfil', N'Listado', N'/Perfil/Listado', N'fa fa-genderless', N'a', 3, 0)
GO
INSERT [Seguridad].[Acceso] ([Id], [Nombre], [Secuencia], [Controlador], [Vista], [Url], [Icono], [Descripcion], [ModuloId], [Eliminado]) VALUES (5, N'System Design', 3, N'Home', N'SystemDesign', N'/Home/SystemDesign', N'fa fa-genderless', N'a', 5, 0)
GO
INSERT [Seguridad].[Acceso] ([Id], [Nombre], [Secuencia], [Controlador], [Vista], [Url], [Icono], [Descripcion], [ModuloId], [Eliminado]) VALUES (6, N'Gestion', 1, N'Gestion', N'Listado', N'/Gestion/Listado', N'fa fa-genderless', N'a', 6, 0)
GO
INSERT [Seguridad].[Acceso] ([Id], [Nombre], [Secuencia], [Controlador], [Vista], [Url], [Icono], [Descripcion], [ModuloId], [Eliminado]) VALUES (7, N'Sucursal', 2, N'Entidad', N'Listado', N'/Entidad/Listado', N'fa fa-genderless', N'a', 7, 0)
GO
INSERT [Seguridad].[Acceso] ([Id], [Nombre], [Secuencia], [Controlador], [Vista], [Url], [Icono], [Descripcion], [ModuloId], [Eliminado]) VALUES (8, N'Formularios', 3, N'Formulario', N'Listado', N'/Formulario/Listado', N'fa fa-genderless', N'a', 6, 0)
GO
INSERT [Seguridad].[Acceso] ([Id], [Nombre], [Secuencia], [Controlador], [Vista], [Url], [Icono], [Descripcion], [ModuloId], [Eliminado]) VALUES (9, N'Grupo', 4, N'Grupo', N'Listado', N'/Grupo/Listado', N'fa fa-genderless', N'a', 6, 0)
GO
SET IDENTITY_INSERT [Seguridad].[Acceso] OFF
GO
SET IDENTITY_INSERT [Seguridad].[Modulo] ON 
GO
INSERT [Seguridad].[Modulo] ([Id], [Nombre], [Icono], [Secuencia], [Eliminado]) VALUES (3, N'Modulo Seguridad', N'fa fa-shield', 3, 0)
GO
INSERT [Seguridad].[Modulo] ([Id], [Nombre], [Icono], [Secuencia], [Eliminado]) VALUES (4, N'Modulo General', N'fa fa-home', 2, 0)
GO
INSERT [Seguridad].[Modulo] ([Id], [Nombre], [Icono], [Secuencia], [Eliminado]) VALUES (5, N'Modulo Administrativo', N'fa fa-cog', 1, 0)
GO
INSERT [Seguridad].[Modulo] ([Id], [Nombre], [Icono], [Secuencia], [Eliminado]) VALUES (6, N'Modulo Formularios', N'fa fa-file-text', 4, 0)
GO
INSERT [Seguridad].[Modulo] ([Id], [Nombre], [Icono], [Secuencia], [Eliminado]) VALUES (7, N'Modulo Configuracion', N'fa fa-cog', 5, 0)
GO
SET IDENTITY_INSERT [Seguridad].[Modulo] OFF
GO
SET IDENTITY_INSERT [Seguridad].[Perfil] ON 
GO
INSERT [Seguridad].[Perfil] ([Id], [Nombre], [Descripcion], [Eliminado]) VALUES (1, N'Administrador', N'Perfil administrador', 0)
GO
INSERT [Seguridad].[Perfil] ([Id], [Nombre], [Descripcion], [Eliminado]) VALUES (2, N'string', N'string', 0)
GO
INSERT [Seguridad].[Perfil] ([Id], [Nombre], [Descripcion], [Eliminado]) VALUES (3, N'tes', N'test', 1)
GO
INSERT [Seguridad].[Perfil] ([Id], [Nombre], [Descripcion], [Eliminado]) VALUES (4, N'tesasd asd asd a sd ', N'testasd asd ', 0)
GO
INSERT [Seguridad].[Perfil] ([Id], [Nombre], [Descripcion], [Eliminado]) VALUES (5, N'tes', N'te', 1)
GO
INSERT [Seguridad].[Perfil] ([Id], [Nombre], [Descripcion], [Eliminado]) VALUES (7, N'te', N'te', 1)
GO
INSERT [Seguridad].[Perfil] ([Id], [Nombre], [Descripcion], [Eliminado]) VALUES (8, N'Perfil modificado 2', N'asdmu cho texto ', 0)
GO
INSERT [Seguridad].[Perfil] ([Id], [Nombre], [Descripcion], [Eliminado]) VALUES (9, N'test', N'test', 1)
GO
INSERT [Seguridad].[Perfil] ([Id], [Nombre], [Descripcion], [Eliminado]) VALUES (10, N'string', N'string', 0)
GO
SET IDENTITY_INSERT [Seguridad].[Perfil] OFF
GO
SET IDENTITY_INSERT [Seguridad].[PerfilAcceso] ON 
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (12, 8, 3, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (15, 3, 3, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (17, 2, 4, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (18, 2, 3, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (19, 9, 4, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (20, 9, 3, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (26, 4, 3, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (27, 4, 4, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (43, 1, 3, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (44, 1, 4, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (45, 1, 5, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (46, 1, 6, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (47, 1, 7, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (48, 1, 8, 0)
GO
INSERT [Seguridad].[PerfilAcceso] ([Id], [PerfilId], [AccesoId], [Eliminado]) VALUES (49, 1, 9, 0)
GO
SET IDENTITY_INSERT [Seguridad].[PerfilAcceso] OFF
GO
SET IDENTITY_INSERT [Seguridad].[Usuario] ON 
GO
INSERT [Seguridad].[Usuario] ([Id], [Nombre], [Apellido], [Username], [Email], [Password], [FechaCreacion], [PerfilId], [Eliminado]) VALUES (1, N'Nahuel', N'Zalazar', N'string', N'string@gmail.com', N'AQAAAAIAAYagAAAAEO1tb0OZlpC+y5aD+uY9Mu7SGl82qW1VRQerv12Fufqf9Z9rQCHSvMK71q8Yn/RIlA==', CAST(N'2025-04-01T21:35:19.887' AS DateTime), 1, 0)
GO
INSERT [Seguridad].[Usuario] ([Id], [Nombre], [Apellido], [Username], [Email], [Password], [FechaCreacion], [PerfilId], [Eliminado]) VALUES (2, N'string', N'string', N'string1', N'string1@gmail.com', N'AQAAAAIAAYagAAAAEOFeu4mzD09abOvcbdentHsKJ0tVYku+xJNmH+zusn83w9MlMbs1QyUim31lr8/+WA==', CAST(N'2025-04-02T21:00:43.623' AS DateTime), 1, 1)
GO
INSERT [Seguridad].[Usuario] ([Id], [Nombre], [Apellido], [Username], [Email], [Password], [FechaCreacion], [PerfilId], [Eliminado]) VALUES (3, N'test 2', N'test 2', N'test 2', N'test@live.com', N'12345678', CAST(N'2025-04-19T16:43:35.830' AS DateTime), 4, 0)
GO
INSERT [Seguridad].[Usuario] ([Id], [Nombre], [Apellido], [Username], [Email], [Password], [FechaCreacion], [PerfilId], [Eliminado]) VALUES (4, N'test 2', N'test', N'test', N'test@gmail.com', N'AQAAAAIAAYagAAAAEC/t8sVR+ClRAiffdfHsWS/QCticxxFLytWfQXnYDwNU0rJbJyVgUrjuLHUkLvBZTA==', CAST(N'2025-04-19T16:55:56.587' AS DateTime), 8, 0)
GO
INSERT [Seguridad].[Usuario] ([Id], [Nombre], [Apellido], [Username], [Email], [Password], [FechaCreacion], [PerfilId], [Eliminado]) VALUES (5, N'testefa', N'asd asd', N'asd asd as', N'asdasda@gmail.com', N'AQAAAAIAAYagAAAAEDdErs+HuPInTHEQgNkkwk6XHauqBJJqIJYG8q3YLLdLkpgvZc+gNoCiauBawR6rrg==', CAST(N'2025-04-19T16:57:25.180' AS DateTime), 2, 0)
GO
INSERT [Seguridad].[Usuario] ([Id], [Nombre], [Apellido], [Username], [Email], [Password], [FechaCreacion], [PerfilId], [Eliminado]) VALUES (6, N'stringasd', N'stringasd', N'stringasd', N'string@test.com', N'AQAAAAIAAYagAAAAELNpVZ7KoLrdk36nefQuYL6u4sMh6aOUJFq5k1/ePDoHrIlfAdqr4uYND4aBKq+keA==', CAST(N'2025-04-20T17:36:28.457' AS DateTime), 10, 0)
GO
INSERT [Seguridad].[Usuario] ([Id], [Nombre], [Apellido], [Username], [Email], [Password], [FechaCreacion], [PerfilId], [Eliminado]) VALUES (7, N'sfsdfsdf', N'sdfsdf', N'sdfsdfs', N'sdfsdf@gas.com', N'AQAAAAIAAYagAAAAEI0HG3avIxpXqrzpr38Eexqfkb/1HAfiwBbLPzGNZlutrqKuqg1KArY01Qbu66IDPQ==', CAST(N'2025-04-20T18:59:29.807' AS DateTime), 1, 0)
GO
INSERT [Seguridad].[Usuario] ([Id], [Nombre], [Apellido], [Username], [Email], [Password], [FechaCreacion], [PerfilId], [Eliminado]) VALUES (8, N'reasdasd asd as1', N'asdasd', N'asdasdas', N'asdasda@adasda.com', N'', CAST(N'2025-04-20T19:43:07.967' AS DateTime), 4, 0)
GO
SET IDENTITY_INSERT [Seguridad].[Usuario] OFF
GO
ALTER TABLE [Configuracion].[Concepto] ADD  CONSTRAINT [DF_Concepto_Editable]  DEFAULT ((0)) FOR [Editable]
GO
ALTER TABLE [Configuracion].[Entidad] ADD  CONSTRAINT [DF_Entidad_Eliminado]  DEFAULT ((0)) FOR [Eliminado]
GO
ALTER TABLE [Seguridad].[Acceso] ADD  CONSTRAINT [DF_Acceso_Eliminado]  DEFAULT ((0)) FOR [Eliminado]
GO
ALTER TABLE [Seguridad].[Perfil] ADD  CONSTRAINT [DF_Perfil_Eliminado]  DEFAULT ((0)) FOR [Eliminado]
GO
ALTER TABLE [Seguridad].[PerfilAcceso] ADD  CONSTRAINT [DF_PerfilAcceso_Eliminado]  DEFAULT ((0)) FOR [Eliminado]
GO
ALTER TABLE [Seguridad].[Usuario] ADD  CONSTRAINT [DF_Usuario_Eliminacion]  DEFAULT ((0)) FOR [Eliminado]
GO
ALTER TABLE [Seguridad].[Acceso]  WITH CHECK ADD  CONSTRAINT [FK_Acceso_Modulo] FOREIGN KEY([ModuloId])
REFERENCES [Seguridad].[Modulo] ([Id])
GO
ALTER TABLE [Seguridad].[Acceso] CHECK CONSTRAINT [FK_Acceso_Modulo]
GO
ALTER TABLE [Seguridad].[PerfilAcceso]  WITH CHECK ADD  CONSTRAINT [FK_PerfilAcceso_Acceso] FOREIGN KEY([AccesoId])
REFERENCES [Seguridad].[Acceso] ([Id])
GO
ALTER TABLE [Seguridad].[PerfilAcceso] CHECK CONSTRAINT [FK_PerfilAcceso_Acceso]
GO
ALTER TABLE [Seguridad].[PerfilAcceso]  WITH CHECK ADD  CONSTRAINT [FK_PerfilAcceso_Perfil] FOREIGN KEY([PerfilId])
REFERENCES [Seguridad].[Perfil] ([Id])
GO
ALTER TABLE [Seguridad].[PerfilAcceso] CHECK CONSTRAINT [FK_PerfilAcceso_Perfil]
GO
ALTER TABLE [Seguridad].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_Perfil] FOREIGN KEY([PerfilId])
REFERENCES [Seguridad].[Perfil] ([Id])
GO
ALTER TABLE [Seguridad].[Usuario] CHECK CONSTRAINT [FK_Usuario_Perfil]
GO
