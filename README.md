# ![SemaphoreSlimThrottling](https://raw.githubusercontent.com/MarkCiliaVincenti/SemaphoreSlimThrottling/master/logo32.png) SemaphoreSlimThrottling
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/MarkCiliaVincenti/SemaphoreSlimThrottling/dotnet.yml?branch=master&logo=github&style=for-the-badge)](https://actions-badge.atrox.dev/MarkCiliaVincenti/SemaphoreSlimThrottling/goto?ref=master) [![Nuget](https://img.shields.io/nuget/v/SemaphoreSlimThrottling?label=SemaphoreSlimThrottling&logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/SemaphoreSlimThrottling) [![Nuget](https://img.shields.io/nuget/dt/SemaphoreSlimThrottling?logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/SemaphoreSlimThrottling)

A .NET Standard 2.0 library that provides a class that uses SemaphoreSlim but allows a negative `initialCount`. This could be used, for example, when starting to throttle requests but initially already have more concurrent requests than the maximum you want to start allowing.

Supports .NET Framework 4.6.1 or later, .NET Core 2.0 or later, and .NET 5.0 or later.

## Installation
The recommended means is to use [NuGet](https://www.nuget.org/packages/SemaphoreSlimThrottling), but you could also download the source code from [here](https://github.com/MarkCiliaVincenti/SemaphoreSlimThrottling/releases).

## Usage
```csharp
// there are 11 concurrent requests, and we want to start limiting to 10.
// SemaphoreSlim does not allow negative initialCount.
var mySemaphore = new SemaphoreSlimThrottle(-1, 10);
```

For more information, read the documentation for [SemaphoreSlim](https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim).
