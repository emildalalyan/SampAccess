# SampAccess Library
This .NET library may simplify your access to SA:MP servers (gathering information about them) and to SA:MP client settings
- [![NuGet](https://img.shields.io/github/v/release/emildalalyan/SampAccess?sort=semver&style=flat-square)](https://www.nuget.org/packages/SampAccess)

### Warning
This library was written long ago, and it won't be maintained anymore.

### Requirements
  - .NET 6 - https://dotnet.microsoft.com/

### Notes
- I wrote this library as an analogue under the MIT license, since those that I found in the Internet were either proprietary or under the GPL license, which is not always suitable.

- *Client* class **IS NOT CROSS-PLATFORM**. It requires ***Microsoft.Win32.Registry*** package, which is working only in Windows.

- The *Query* algorithm is based on https://github.com/zeelorenc/SA-MP-Server-Query-Class, but code is rewritten

### Query example
SampAccess API is pretty easy and straightforward. Here is an example:
```c#
using System;
using SampAccess;

namespace Test
{
    class Program
    {
        public static void Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using Query query = new(/* IP */ args[1], /* Port */ Convert.ToUInt16(args[2]));
            query.Initialize();

            // Basic information about a server
            Console.WriteLine($"Ping {query.PingInMilliseconds} ms\n");
            Console.WriteLine("Common information:\n" +
                $"Players: {query.CommonInformation.PlayersOnline}/{query.CommonInformation.LimitOfPlayers}\n" +
                $"Has password: {query.CommonInformation.HasPassword}\n" +
                $"Host name: {query.CommonInformation.HostName}\n" +
                $"Game mode: {query.CommonInformation.GameMode}\n" +
                $"Language: {query.CommonInformation.Language}\n");

            // Server rules
            Console.WriteLine("Server rules:");
            foreach(KeyValuePair<string, string> rule in query.ServerRules)
            {
                Console.WriteLine($"Key: {rule.Key}; Value: {rule.Value}");
            }
            Console.WriteLine();

            // Player information
            Console.WriteLine("Players:");
            foreach (Query.Player player in query.PlayersInformation)
            {
                Console.WriteLine($"Player nick: {player.Nick}\n" +
                    $"Ping: {player.Ping}\n" +
                    $"Score: {player.Score}");

                if(player.PlayerID != null)
                {
                    Console.WriteLine($"Player ID: {player.PlayerID}");
                }

                Console.WriteLine();
            }
        }
    }
}
```

### Client example
Client is even more simple, than Query class:
```c#
using System;
using SampAccess;

namespace Test
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Your nickname: {Client.PlayerName}");
            Console.WriteLine($"Game executable path: {Client.GameExecutable}");
            Console.WriteLine("Checkboxes:\n" +
                $"Save RCON passwords: {Client.SaveRconPasswords}\n" +
                $"Save server passwords: {Client.SaveServerPasswords}");

            Console.WriteLine();

            Console.Write("Your new nickname: ");
            Client.PlayerName = Console.ReadLine();

            Console.Write("Your new game executable: ");
            Client.GameExecutable = Console.ReadLine();

            Console.Write("Do you want to save RCON passwords: ");
            Client.SaveRconPasswords = Convert.ToInt32(Console.ReadLine());

            Console.Write("Do you want to save server passwords: ");
            Client.SaveServerPasswords = Convert.ToInt32(Console.ReadLine());
        }
    }
}
```
