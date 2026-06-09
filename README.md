# ChatApp_BE

ASP.NET Core backend for a real-time chat application. The backend provides JWT authentication, ASP.NET Core Identity user accounts, production conversation/message APIs, SignalR realtime events, message persistence, presence, read receipts, reactions, attachment metadata, profile management, and user blocking.

Frontend repository: https://github.com/Lmnhutw/ChatApp_FE

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- ASP.NET Core Identity
- JWT bearer authentication
- SignalR
- Entity Framework Core
- SQL Server
- SendGrid for email confirmation
- Swagger / OpenAPI

## Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server Express
- EF Core CLI tool
- SendGrid API key, if email confirmation should send real emails

Install EF Core CLI if needed:

```sh
dotnet tool install --global dotnet-ef
```

## Configuration

The app reads configuration from `appsettings.json`, user secrets, environment variables, and command-line arguments.

### Required Settings

Set the database connection string:

```json
{
  "ConnectionStrings": {
    "ChatApp": "Server=localhost;Database=ChatApp;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Set a JWT secret key. It must be at least 32 characters.

For local development, prefer user secrets:

```sh
dotnet user-secrets set "Jwt:SecretKey" "replace-with-a-local-secret-at-least-32-chars"
```

Set SendGrid if email confirmation is used:

```sh
dotnet user-secrets set "SendGrid:ApiSenderKey" "replace-with-sendgrid-api-key"
```

Optional CORS configuration:

```json
{
  "Cors": {
    "AllowedOrigins": [ "http://localhost:3000" ]
  }
}
```

## Database Setup

Create or update the SQL Server database with EF Core migrations:

```sh
dotnet ef database update
```

If you change the EF model, add a migration:

```sh
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Run Locally

Restore and build:

```sh
dotnet restore
dotnet build
```

Run with the HTTPS launch profile:

```sh
dotnet run --launch-profile https
```

Default local URLs from `Properties/launchSettings.json`:

- HTTPS: `https://localhost:5000`
- HTTP: `http://localhost:5050`
- Swagger: `https://localhost:5000/swagger`
- SignalR hub: `https://localhost:5000/hub`
- Health check: `https://localhost:5000/health`

## Authentication Flow

1. Register:

```http
POST /api/Auth/register
```

2. Confirm email from the email confirmation link.

3. Login:

```http
POST /api/Auth/login
```

4. Use the returned JWT for protected APIs:

```http
Authorization: Bearer <token>
```

Important: login requires confirmed email. Unconfirmed users receive:

```text
Email confirmation is required before login.
```

## SignalR Usage

The SignalR hub requires JWT authentication. Browser clients should pass the token with `accessTokenFactory`.

Example with `@microsoft/signalr`:

```ts
const connection = new HubConnectionBuilder()
  .withUrl("https://localhost:5000/hub", {
    accessTokenFactory: () => localStorage.getItem("token") ?? "",
  })
  .withAutomaticReconnect()
  .build();
```

Production conversation hub methods include:

- `JoinConversation(conversationId)`
- `LeaveConversation(conversationId)`
- `SendConversationMessage(conversationId, request)`
- `EditConversationMessage(conversationId, messageId, request)`
- `DeleteConversationMessage(conversationId, messageId)`
- `StartTyping(conversationId)`
- `StopTyping(conversationId)`
- `MarkMessageRead(conversationId, messageId)`
- `AddMessageReaction(conversationId, messageId, request)`
- `RemoveMessageReaction(conversationId, messageId, reaction)`

Key events include:

- `MessageReceived`
- `MessageUpdated`
- `MessageDeleted`
- `TypingChanged`
- `MessageRead`
- `MessageReactionAdded`
- `MessageReactionRemoved`
- `PresenceChanged`
- `RealtimeError`

Legacy hub methods `JoinRoom`, `SendMessage`, and `LeaveRoom` still exist, but `RoomName` must now be a production `conversationId` GUID.

## Main API Areas

### Auth

- `POST /api/Auth/register`
- `POST /api/Auth/resend-verification-email`
- `GET /api/Auth/resend-verification-email/{email}`
- `GET /api/Auth/confirmemail`
- `POST /api/Auth/login`
- `GET /api/Auth/me`
- `GET /api/Auth/check`
- `POST /api/Auth/logout`

### Conversations

- `GET /api/conversations`
- `GET /api/conversations/{conversationId}`
- `POST /api/conversations/direct`
- `POST /api/conversations/groups`
- `GET /api/conversations/{conversationId}/members`
- `POST /api/conversations/{conversationId}/members`
- `DELETE /api/conversations/{conversationId}/members/{memberUserId}`

### Messages

- `GET /api/conversations/{conversationId}/messages`
- `GET /api/conversations/{conversationId}/messages/search?query=...`
- `POST /api/conversations/{conversationId}/messages`
- `PUT /api/conversations/{conversationId}/messages/{messageId}`
- `DELETE /api/conversations/{conversationId}/messages/{messageId}`
- `POST /api/conversations/{conversationId}/messages/{messageId}/read`

### Reactions and Attachments

- `GET /api/conversations/{conversationId}/messages/{messageId}/reactions`
- `POST /api/conversations/{conversationId}/messages/{messageId}/reactions`
- `DELETE /api/conversations/{conversationId}/messages/{messageId}/reactions/{reaction}`
- `GET /api/conversations/{conversationId}/messages/{messageId}/attachments`
- `POST /api/conversations/{conversationId}/messages/{messageId}/attachments`
- `DELETE /api/conversations/{conversationId}/messages/{messageId}/attachments/{attachmentId}`

Attachment endpoints currently store metadata only. They do not upload or stream file bytes.

### Users

- `GET /api/users/me`
- `PUT /api/users/me`
- `GET /api/users/search?query=...`
- `GET /api/users/blocks`
- `POST /api/users/blocks`
- `DELETE /api/users/blocks/{blockedUserId}`

## Development Notes

- Keep JWT and SendGrid secrets out of `appsettings.json`.
- Use DTOs for API contracts; do not expose EF entities directly.
- Apply migrations before running features that use new tables.
- Current presence/connection tracking is in-memory and suitable for a single app instance. For multiple instances, replace it with Redis, Azure SignalR, or another shared store.
- Swagger supports JWT bearer tokens through the `Authorize` button.

## Troubleshooting

### `Jwt:SecretKey must be at least 32 characters long`

Set a longer local secret:

```sh
dotnet user-secrets set "Jwt:SecretKey" "replace-with-a-local-secret-at-least-32-chars"
```

### Database connection fails

Check `ConnectionStrings:ChatApp`, ensure SQL Server is running, then run:

```sh
dotnet ef database update
```

### Frontend cannot call protected APIs

Make sure requests include:

```http
Authorization: Bearer <token>
```

Also confirm the frontend origin is listed in `Cors:AllowedOrigins`.

### SignalR connection fails with 401

Pass the JWT token through `accessTokenFactory` when creating the SignalR connection.

## Build Verification

Run:

```sh
dotnet build
```

The project may show existing warnings for nullable properties or package advisories. Build should complete successfully before running.
