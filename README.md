# KRT Bank - Account API

API REST para gerenciamento de contas do banco KRT, desenvolvida como teste técnico em **.NET 8**, seguindo os princípios de **Clean Architecture**, **CQRS** (com MediatR) e **Domain-Driven Design**.

## Sumário

- [Sobre o projeto](#sobre-o-projeto)
- [Arquitetura](#arquitetura)
- [Tecnologias e pacotes](#tecnologias-e-pacotes)
- [Pré-requisitos](#pré-requisitos)
- [Configuração](#configuração)
- [Como rodar](#como-rodar)
- [Endpoints](#endpoints)
- [Regras de negócio](#regras-de-negócio)
- [Testes](#testes)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Pontos de atenção / limitações conhecidas](#pontos-de-atenção--limitações-conhecidas)

## Sobre o projeto

A API permite cadastrar, consultar, atualizar e desativar contas bancárias, cada uma identificada por um **CPF** único e validado. Não há operações financeiras (depósito, saque, transferência) — o escopo é o ciclo de vida cadastral da conta (CRUD com exclusão lógica).

Casos de uso implementados:

- Criar conta (`CreateAccount`)
- Consultar conta por Id (`GetAccountById`)
- Listar contas, com filtro opcional por status (`GetAllAccounts`)
- Atualizar nome do titular (`UpdateAccount`)
- Desativar conta — soft delete (`DeleteAccount`)

## Arquitetura

O projeto segue Clean Architecture, dividido em camadas com dependências apontando sempre para dentro (Domain no centro):

```
KRT.API            -> Controllers, middlewares, Swagger, ponto de entrada (Program.cs)
KRT.Application    -> Casos de uso (CQRS com MediatR: Commands/Queries), DTOs, validações (FluentValidation)
KRT.Domain         -> Entidades, Value Objects, Enums, Exceptions, interfaces de repositório (sem dependências externas)
KRT.Infrastructure -> EF Core (SQL Server), cache (Redis), repositórios, Unit of Work
KRT.IoC            -> Composição/registro de dependências (Dependency Injection)
```

- **CQRS / Mediator**: cada caso de uso é um `Command`/`Query` + `Handler`, orquestrado via MediatR.
- **Value Object `Cpf`**: encapsula normalização (remove máscara) e validação do CPF (dígitos verificadores, módulo 11).
- **Cache-aside**: consultas (`GetAccountById`, `GetAllAccounts`) usam cache com TTL de 5 minutos; comandos de escrita invalidam as chaves relacionadas. Motivação: diferentes áreas do banco (prevenção a fraude, cartões, etc.) consultam repetidamente os dados da mesma conta ao longo do dia, e cada consulta ao banco gera custo na AWS — o cache evita re-consultar uma conta já buscada recentemente.
- **Soft delete**: `DeleteAccount` não remove o registro, apenas altera o `Status` para `Inactive`.

## Tecnologias e pacotes

- **.NET 8** / ASP.NET Core Web API
- **Entity Framework Core 8** (`Microsoft.EntityFrameworkCore.SqlServer`) — SQL Server / LocalDB
- **MediatR 12** — CQRS
- **FluentValidation 11** — validação de comandos
- **StackExchange.Redis** — cache distribuído (com fallback automático para um cache nulo caso o Redis esteja indisponível)
- **Swashbuckle (Swagger/OpenAPI)** — documentação e teste interativo da API
- **xUnit + Moq + FluentAssertions** — testes automatizados

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express ou instância completa)
- Redis (opcional — a aplicação funciona sem ele, apenas sem cache)

## Configuração

As configurações ficam em `src/KRT.API/appsettings.json` (e `appsettings.Development.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\Local;Database=KrtBankDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

- `DefaultConnection`: obrigatória — string de conexão do SQL Server. Ajuste conforme seu ambiente (LocalDB, Docker, instância remota, etc).
- `Redis`: opcional — se a conexão falhar na inicialização, a aplicação usa automaticamente um cache "nulo" (sem cache) e continua funcionando normalmente.

## Como rodar

1. Clone o repositório e restaure os pacotes:

   ```bash
   dotnet restore
   ```

2. Aplique as migrations para criar o banco de dados (`KrtBankDb`):

   ```bash
   dotnet ef database update --project src/KRT.Infrastructure --startup-project src/KRT.API
   ```

   > Caso não tenha a ferramenta instalada: `dotnet tool install --global dotnet-ef`

3. (Opcional) Suba um Redis local, por exemplo via Docker:

   ```bash
   docker run -d --name krt-redis -p 6379:6379 redis:alpine
   ```

4. Execute a API:

   ```bash
   dotnet run --project src/KRT.API
   ```

5. Acesse o Swagger:

   - HTTPS: `https://localhost:7189/swagger`
   - HTTP: `http://localhost:5252/swagger`

## Endpoints

Base route: `/api/accounts`

| Verbo  | Rota                  | Descrição                                   | Body / Query                          | Respostas                       |
|--------|-----------------------|----------------------------------------------|----------------------------------------|----------------------------------|
| GET    | `/api/accounts`       | Lista contas, paginada, com filtro opcional por status | `?status=Active\|Inactive&pageNumber=1&pageSize=20` | `200 OK` — `PagedResult<AccountDto>` |
| GET    | `/api/accounts/{id}`  | Busca conta por Id                           | -                                       | `200 OK` / `404 Not Found`       |
| POST   | `/api/accounts`       | Cria uma nova conta                          | `{ "holderName": string, "cpf": string }` | `201 Created` / `400` / `422`  |
| PUT    | `/api/accounts/{id}`  | Atualiza o nome do titular                   | `{ "holderName": string }`             | `204 No Content` / `400` / `404` |
| DELETE | `/api/accounts/{id}`  | Desativa a conta (soft delete)               | -                                       | `204 No Content` / `404` / `422` |

### Paginação (`GET /api/accounts`)

A listagem de contas é paginada via query string:

- `pageNumber` (opcional, padrão `1`)
- `pageSize` (opcional, padrão `20`)
- `status` (opcional, filtra por `Active` ou `Inactive`)

Exemplo: `GET /api/accounts?status=Active&pageNumber=2&pageSize=50`

### Formato de resposta (`PagedResult<AccountDto>`)

```json
{
  "items": [
    {
      "id": "guid",
      "holderName": "string",
      "cpf": "string",
      "status": "Active | Inactive"
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3
}
```

### Tratamento de erros

Um middleware global (`ExceptionHandlerMiddleware`) padroniza as respostas de erro:

| Origem                              | Status | Corpo                                  |
|--------------------------------------|--------|-----------------------------------------|
| `DomainException` (regra de negócio) | `422`  | `{ "errors": "mensagem" }`              |
| `ValidationException` (FluentValidation) | `400` | `{ "errors": ["mensagem1", ...] }`  |
| Qualquer outra exceção               | `500`  | `{ "errors": "Ocorreu um erro interno." }` |

## Regras de negócio

- **CPF único**: não é possível cadastrar duas contas com o mesmo CPF.
- **CPF válido**: o value object `Cpf` valida os 11 dígitos e os dígitos verificadores (algoritmo módulo 11), rejeitando sequências repetidas (ex.: `111.111.111-11`).
- **Nome do titular obrigatório**, com no máximo 150 caracteres.
- **Exclusão lógica**: contas nunca são removidas do banco; `DELETE` apenas altera o status para `Inactive`.

## Testes

O projeto conta com testes unitários para `KRT.Domain` (entidades e value objects) e `KRT.Application` (handlers de commands/queries), usando xUnit, Moq e FluentAssertions.

```bash
dotnet test
```

## Estrutura de pastas

```
src/
  KRT.API/              # Controllers, middlewares, Program.cs, Swagger
  KRT.Application/       # Commands, Queries, Handlers, DTOs, Validators (CQRS)
  KRT.Domain/            # Entidades, Value Objects, Enums, Exceptions, interfaces
  KRT.Infrastructure/    # EF Core, migrations, repositórios, cache, Unit of Work
  KRT.IoC/               # Registro de dependências (DI)
tests/
  KRT.Domain.Tests/
  KRT.Application.Tests/
```
