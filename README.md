# SampAccess Library
This .NET library simplify your access to SA:MP servers (gather information about them) and to SA:MP client settings

### Requirements
  - .NET 5 — https://dotnet.microsoft.com/

### Notes
- I wrote this library as an analogue under the MIT license, since those that I found on the Internet were either proprietary or under the GPL license, which is not always suitable.

- Only *Query* class is **CROSS-PLATFORM**, *Client* class **IS NOT**. *Client* class requires ***Microsoft.Win32.Registry*** package, which is working only in Windows.

- The *Query* algorithm is based on https://github.com/zeelorenc/SA-MP-Server-Query-Class, but completely rewritten 
