# ChatApp_BE

## Description
ChatApp_BE is a backend service for a real-time chat application and my school project. It handles user authentication, message routing, and storage, providing a robust backend solution built with C#. The project aims to learn and implement SignalR for real-time communication.
And this is the link to FE: https://github.com/Lmnhutw/ChatApp_FE (WIP)

## Features
- User authentication and authorization
- Real-time messaging with SignalR
- Message persistence

## Learning SignalR
SignalR is a library for ASP.NET that makes it easy to add real-time web functionality to applications. SignalR allows server-side code to push content to connected clients instantly.

### Resources for Learning SignalR:
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-5.0)
- [SignalR GitHub Repository](https://github.com/dotnet/aspnetcore/tree/main/src/SignalR)
- [SignalR Tutorials](https://www.youtube.com/results?search_query=signalr+tutorial)

## SignalR Implementation
To implement SignalR in your project, follow these steps:

1. Install the SignalR package:
   ```sh
   dotnet add package Microsoft.AspNetCore.SignalR
   ```
2. Create a Hub class:
   ```csharp
   public class ChatHub : Hub
   {
       public async Task SendMessage(string user, string message)
       {
           await Clients.All.SendAsync("ReceiveMessage", user, message);
       }
   }
   ```
3. Configure SignalR in `Startup.cs`:
   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       services.AddSignalR();
   }

   public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
   {
       app.UseEndpoints(endpoints =>
       {
           endpoints.MapHub<ChatHub>("/chathub");
       });
   }
   ```

## Key Features to Implement
Here are some suggested features to implement using SignalR:
- Typing indicators: Show when a user is typing a message.
- Presence indicators: Show when users are online or offline.
- Private messaging: Enable one-on-one messaging.
- Group chats: Allow users to create and join group chats.
- Message history: Persist and retrieve chat history.

## Installation
To install and run the project locally, follow these steps:
1. Clone the repository:
   ```sh
   git clone https://github.com/Lmnhutw/ChatApp_BE.git
   ```
2. Navigate to the project directory:
   ```sh
   cd ChatApp_BE
   ```
3. Build the project:
   ```sh
   dotnet build
   ```
4. Run the project:
   ```sh
   dotnet run
   ```

## Usage
- After running the project, the backend service will be available on the default port.
- Connect the frontend chat application to this backend service to start using the chat functionalities.

## Contributing
Contributions are welcome! Please fork the repository and create a pull request with your changes.

## License
This project does not have a license specified.

Would you like to add or modify any details in the drafts above?
