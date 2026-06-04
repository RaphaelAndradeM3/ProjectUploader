# Plano de Implementação: ProjectUploader

O sistema ProjectUploader consiste em aplicativos Windows Forms (Cliente/Upload e Servidor/Download) conectados a uma infraestrutura centralizada via HTTP/HTTPS. O sistema suporta múltiplas threads, gestão completa de usuários, controle de permissões (Administrador e Comum) e auditoria no SQL Server. O código será escrito em C# (.NET 8/10) obedecendo às diretrizes de Clean Architecture, DDD, SOLID e nomenclatura em pt-BR.

## Cronograma de Passos de Execução (Roadmap)

### Passo 1: Configuração da Solução e Arquitetura Base
- Criar a Solução em branco (`ProjectUploader.sln`).
- Criar os projetos de Class Library para `Dominio`, `Aplicacao` e `Infraestrutura`.
- Criar o projeto ASP.NET Core Web API para o `Backend`.
- Criar os projetos Windows Forms para `UploadApp` (Cliente) e `DownloadApp` (Servidor GUI).
- Configurar referências entre as camadas seguindo a regra de dependência do Clean Architecture.
- Adicionar configurações base (`.editorconfig`, versionamento do pacote C#).

### Passo 2: Implementação da Camada de Domínio
- Criar Entidade `Usuario` (agregado com regras de email, senha, perfil).
- Criar Entidade `ArquivoTransferencia` (rastreio de nome, status, tamanho).
- Criar Entidade `LogEvento` (auditoria do sistema).
- Criar as interfaces `IRepositorioUsuario`, `IRepositorioArquivo`, `IRepositorioLogEvento`.
- Desenvolver as regras de negócio intrínsecas às entidades e Exceptions de Domínio personalizadas (`NomeAppException`).

### Passo 3: Implementação da Camada de Aplicação
- Implementar Casos de Uso de Autenticação (`AutenticarUsuarioUseCase`, `RecuperarSenhaUseCase`).
- Implementar Casos de Uso de Gestão de Usuários (`CadastrarUsuarioUseCase`, `EditarUsuarioUseCase`).
- Implementar as regras de orquestração para transferência (`IniciarUploadLoteUseCase`, `IniciarDownloadLoteUseCase`).

### Passo 4: Infraestrutura, Persistência e Integrações
- Configurar Entity Framework Core com SQL Server no projeto de `Infraestrutura`.
- Criar o arquivo `ContextoBancoDados` (DbContext) mapeando as tabelas e garantindo concorrência.
- Implementar as classes concretas para `RepositorioUsuario`, `RepositorioArquivo` e `RepositorioLogEvento`.
- Implementar o provedor de disparo de e-mail (SMTP) para recuperação de senha.
- Configurar logs estruturados usando `Serilog`.

### Passo 5: Desenvolvimento do Backend (Web API)
- Construir a infraestrutura HTTP/HTTPS base com suporte a SSL.
- Desenvolver o Controlador de Autenticação (`/api/auth/login`, `/api/auth/recuperar-senha`).
- Desenvolver o Controlador de Usuários (`/api/usuarios`) restrito a perfis correspondentes.
- Desenvolver o Controlador de Transferência de Arquivos (`/api/arquivos/upload`, `/api/arquivos/download`) com capacidades de chunking e validação de hash pós-recebimento.

### Passo 6: Desenvolvimento do ClientApp (UploadApp)
- Criar telas Windows Forms: `FrmLogin`, `FrmEsqueciSenha` e `FrmPrincipalUpload`.
- Integrar chamadas HTTP seguras aos controllers do backend.
- Desenvolver a lógica multi-thread local (com `Task.WhenAll` e `SemaphoreSlim`) para o upload massivo (até 999 conexões simultâneas).
- Implementar grids em tempo real de contadores, velocidade atual e tempo restante.

### Passo 7: Desenvolvimento do ServerApp (DownloadApp / Admin)
- Criar telas Windows Forms focadas no Perfil Administrador e Download.
- Desenvolver o `FrmGerenciarUsuarios` para painel de controle administrativo.
- Desenvolver o `FrmPrincipalDownload` que listará do Backend os arquivos submetidos por outros usuários, efetuando o download rápido (multi-thread) pro disco e registrando sucesso/falha no BD.

### Passo 8: Testes, Validação e Deploy
- **Testes Unitários**: Validar as regras de domínio e cálculos de banda/ETA.
- **Teste de Integração**: Testar o ciclo completo do banco de dados (EF Core InMemory ou Testcontainers).
- **Validação Manual**: Rodar a API, Client e Server App simultaneamente, forçar interrupções e garantir que logs chegam no banco e auditorias funcionam sem perdas.
