# Novus

[![nuget](https://img.shields.io/nuget/v/Novus?style=flat-square)](https://www.nuget.org/packages/Novus/)
[![nuget dl](https://img.shields.io/nuget/dt/Novus?style=flat-square)](https://www.nuget.org/packages/Novus/)

![Icon](https://github.com/Decimation/Novus/raw/master/icon64.png)

Low-level utilities and tools for working with the CLR, CLR internal structures, and memory. Improved version 
of **[NeoCore](https://github.com/Decimation/NeoCore)**.

# Goals

**Novus** aims to provide functionality similar to that of **ClrMD**, **WinDbg SOS**, and **Reflection** but in a more detailed fashion while also exposing more underlying metadata and CLR functionality.

**Novus** also allows for manipulation of the CLR and low-level operations with managed objects. Additionally, **Novus** doesn't require attachment of a debugger 
to the process to acquire metadata. All metadata is acquired through memory or low-level functions.

# Features

* Calculating heap size of managed objects
* Taking the address of managed objects
* Pointer to managed types
* Pinning unblittable objects
* And much more

# Compatibility
* 64-bit (and partial 32-bit support)
* Windows
* .NET CLR 8+
* Workstation GC
