## Getting Started

### 0.Introduction
This is the gamemode on which SVRP is based and contains a very general and draft structure of the current gamemode. You can use it to test your scripts in order to allow an easier embedding in the full gamemode. You can also fix some bugs on this gamemode if you need it and push these fixes for future development.

### 1.Prerequisites

* [RAGE Multiplayer](https://cdn.rage.mp/updater/prerelease/updater.exe) - The client to login into the server
* [MySQL Server](https://dev.mysql.com/downloads/mysql/) - The database to store the data
* [.NET Core SDK](https://www.microsoft.com/net/download) - The SDK to develop C# resources

**Note:** This project has only been tested under Windows environments

### 2.Installing the Server
1. Execute the **updater.exe** downloaded from RAGE Multiplayer's website
2. Edit the **%RAGEMP Installed folder%/config.xml** and change the channel to **10_mNwuchuQ4ktWbR8d2N5jUzRt**
3. Execute the **updater.exe** to get the 1.1 version
4. Make sure your router has opened 22005 UDP port and 22006 TCP/IP, if you dont know how to do that just google for router port forwarding

**Note:** For more information check the [Wiki](https://wiki.rage.mp/index.php?title=Getting_Started_with_Server) from RAGE

### 3.Installing the GameMode
1. Get all the files from this GitHub and place them into the same folder as before, replacing the files you're asked for
2. Open your MySQL client and import the **.sql** database scripts located under **%RAGEMP Installed folder%/server-files/dotnet/resources/WiredPlayers/data/scripts** folder
3. Import to Visual Studio the **WiredPlayers.csproj** file, located on the following path: **%RAGEMP Installed folder%/server-files/dotnet/resources/WiredPlayers/**

#### Database Connection:
4. Change the database connection settings under **meta.xml** located on the following path: **%RAGEMP Installed folder%/server-files/dotnet/resources/WiredPlayers/** or in your Visual Studio Project! You may get an error regarding to Database SSL Connection, check the **F.A.Q.** page bellow.
5. On Visual Studio, clean and build the solution in order to generate the required **WiredPlayers.dll** library
6. Execute the **rage_server.exe** located under the **%RAGEMP Installed folder%/server-files** folder
7. Log into your server and enjoy it

If you followed all this steps, you should be able to login with your newly registered account, if not please check the **[F.A.Q.](https://github.com/xabier1989/WiredPlayers-RP/wiki/FAQ)** where some of the common errors are solved!


## Other information
Languages files: dotnet/resources/WiredPlayers/messages
