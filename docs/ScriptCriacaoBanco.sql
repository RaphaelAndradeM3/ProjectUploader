CREATE TABLE [LogsEventos] (
    [Id] uniqueidentifier NOT NULL,
    [DataHora] datetime2 NOT NULL,
    [Nivel] nvarchar(20) NOT NULL,
    [Operacao] nvarchar(100) NOT NULL,
    [Detalhes] nvarchar(max) NOT NULL,
    [IdUsuarioLogado] uniqueidentifier NULL,
    [IpOrigem] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_LogsEventos] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Usuarios] (
    [Id] uniqueidentifier NOT NULL,
    [NomeCompleto] nvarchar(150) NOT NULL,
    [NomeUsuario] nvarchar(50) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [EmailExtra] nvarchar(150) NOT NULL,
    [SenhaHash] nvarchar(255) NOT NULL,
    [Telefone] nvarchar(20) NOT NULL,
    [WhatsApp] nvarchar(20) NOT NULL,
    [Telegram] nvarchar(50) NOT NULL,
    [DicaRecuperacao] nvarchar(100) NOT NULL,
    [Perfil] int NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ArquivosTransferencia] (
    [Id] uniqueidentifier NOT NULL,
    [IdUsuario] uniqueidentifier NOT NULL,
    [NomeOriginal] nvarchar(255) NOT NULL,
    [CaminhoServidor] nvarchar(1000) NOT NULL,
    [TamanhoBytes] bigint NOT NULL,
    [HashVerificacao] nvarchar(255) NOT NULL,
    [Status] int NOT NULL,
    [DataRegistro] datetime2 NOT NULL,
    [DataConclusao] datetime2 NULL,
    [MensagemErro] nvarchar(4000) NOT NULL,
    CONSTRAINT [PK_ArquivosTransferencia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ArquivosTransferencia_Usuarios_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);
GO


CREATE INDEX [IX_ArquivosTransferencia_IdUsuario] ON [ArquivosTransferencia] ([IdUsuario]);
GO


CREATE INDEX [IX_LogsEventos_DataHora] ON [LogsEventos] ([DataHora]);
GO


CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
GO


CREATE UNIQUE INDEX [IX_Usuarios_NomeUsuario] ON [Usuarios] ([NomeUsuario]);
GO


