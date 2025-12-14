# ![SemaphoreSlimThrottling](https://raw.githubusercontent.com/MarkCiliaVincenti/SemaphoreSlimThrottling/master/logo32.png)&nbsp;SemaphoreSlimThrottling
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/MarkCiliaVincenti/SemaphoreSlimThrottling/dotnet.yml?branch=master&logo=github&style=flat)](https://actions-badge.atrox.dev/MarkCiliaVincenti/SemaphoreSlimThrottling/goto?ref=master) [![NuGet](https://img.shields.io/nuget/v/SemaphoreSlimThrottling?label=NuGet&logo=nuget&style=flat)](https://www.nuget.org/packages/SemaphoreSlimThrottling) [![NuGet](https://img.shields.io/nuget/dt/SemaphoreSlimThrottling?logo=nuget&style=flat)](https://www.nuget.org/packages/SemaphoreSlimThrottling) [![Codacy Grade](https://img.shields.io/codacy/grade/091aeebda7f14580a9c1bfc323d56f2b?style=flat)](https://app.codacy.com/gh/MarkCiliaVincenti/SemaphoreSlimThrottling/dashboard) [![Codecov](https://img.shields.io/codecov/c/github/MarkCiliaVincenti/SemaphoreSlimThrottling?label=coverage&logo=codecov&style=flat)](https://app.codecov.io/gh/MarkCiliaVincenti/SemaphoreSlimThrottling)

A .NET library that provides a class that uses SemaphoreSlim but allows a negative `initialCount`. This could be used, for example, when starting to throttle requests but initially already have more concurrent requests than the maximum you want to start allowing.

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
