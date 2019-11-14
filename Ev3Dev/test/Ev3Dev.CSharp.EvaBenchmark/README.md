# EvA action call benchmark

To get actual results, build the project with Release configuration:

```bash
$ dotnet build -c Release
```

And run with administrator/root privileges.

Unfortunately, BenchmarkDotNet cannot use appropriate libraries when running `net471` binary on macOS/Linux with `dotnet run`, so use mono directly:

```bash
$ sudo mono bin/Release/net471/Ev3Dev.CSharp.EvaBenchmark.exe
```