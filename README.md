# ProjectUploader

Sistema de envio e recebimento de grandes arquivos (imagens, vídeos, PDFs), otimizado para máxima velocidade e utilizando todo o limite de banda disponível. O sistema é dividido em aplicativos para Upload (Cliente) e Download/Recebimento (Servidor), integrados com um backend para gestão de usuários.

## Características Principais

* **Alta Performance e Paralelismo**: Upload e download de arquivos variando de poucos KBs até vários GBs utilizando multi-threading (até 999 arquivos simultâneos).
* **Gestão de Usuários e Autenticação**: 
  * Níveis de acesso: Administrador e Comum.
  * Cadastro completo (Nome, Username, Email, Telefones WhatsApp/Telegram, etc).
  * Recuperação de senha via link por e-mail.
* **Organização e Gestão de Arquivos**: Usuários possuem pastas/subpastas, configuram diretórios de upload padrão e podem visualizar o histórico e status de arquivos enviados.
* **Auditoria e Logs**: Banco de dados (SQL Server) dedicado ao log de eventos detalhado sobre operações, falhas e sucessos em uploads e downloads.
* **Segurança e Rede**: Comunicação via HTTP/HTTPS (com e sem SSL).
* **Tecnologia**: Desenvolvido em C# (.NET Framework 4.8 ou superior) usando Windows Forms para as interfaces cliente.
* **Arquitetura Limpa (Clean Architecture)**: Projeto estruturado rigorosamente seguindo os princípios de DDD (Domain-Driven Design) e SOLID.

## Estrutura do Sistema

O sistema é dividido nas seguintes frentes:
1. **Client (Upload) App**: Windows Forms App dedicado a autenticar o usuário, organizar pastas locais e enviar os arquivos de forma paralela ao servidor web.
2. **Server (Backend & Download)**:
   * **Backend/Receptor**: Serviço/Web API (HTTP/HTTPS) que recebe os uploads, gerencia a lógica de usuários e registra as informações no SQL Server.
   * **App de Download**: Aplicação para realizar o download em lote/alta velocidade dos arquivos armazenados no servidor.

## Documentação e Regras

* Os documentos de requisitos encontram-se na pasta `docs/`.
* As regras de arquitetura (Clean Architecture, DDD, SOLID) encontram-se em `config/GEMINI_Padrao_CSharp.md`. **Importante**: O código deverá ser todo escrito em pt-BR.
