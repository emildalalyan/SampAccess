# SampAccess Library
This .NET library may simplify your access to SA:MP servers (gather information about them) and to SA:MP client settings
- [![NuGet](https://img.shields.io/github/v/release/emildalalyan/SampAccess?sort=semver&style=flat-square)](https://www.nuget.org/packages/SampAccess)

### Requirements
  - .NET 5 — https://dotnet.microsoft.com/

### Notes
- I wrote this library as an analogue under the MIT license, since those that I found in the Internet were either proprietary or under the GPL license, which is not always suitable.

- *Client* class **IS NOT CROSS-PLATFORM**. It requires ***Microsoft.Win32.Registry*** package, which is working only in Windows.

- The *Query* algorithm is based on https://github.com/zeelorenc/SA-MP-Server-Query-Class, but code is completely rewritten 
