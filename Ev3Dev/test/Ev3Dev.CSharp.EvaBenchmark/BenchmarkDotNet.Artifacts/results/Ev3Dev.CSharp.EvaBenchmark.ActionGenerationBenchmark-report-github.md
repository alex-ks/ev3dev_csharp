``` ini

BenchmarkDotNet=v0.12.0, OS=macOS Mojave 10.14.6 (18G87) [Darwin 18.7.0]
Intel Core i7-4870HQ CPU 2.50GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
  [Host]     : Mono 6.0.0.319 (tarball Wed), X64 
  DefaultJob : Mono 6.0.0.319 (tarball Wed), X64 


```
|         Method |        Mean |     Error |    StdDev |      Median |
|--------------- |------------:|----------:|----------:|------------:|
|  ZeroArgAction |    82.19 ns |  1.674 ns |  4.010 ns |    81.17 ns |
|   OneArgAction |   106.63 ns |  2.170 ns |  3.913 ns |   104.67 ns |
|   TwoArgAction |   127.58 ns |  2.533 ns |  3.016 ns |   127.35 ns |
| ThreeArgAction |   146.58 ns |  1.851 ns |  1.731 ns |   146.64 ns |
|  FourArgAction |   169.73 ns |  1.080 ns |  0.957 ns |   169.42 ns |
|  FiveArgAction |   182.52 ns |  1.872 ns |  1.751 ns |   182.46 ns |
|   SixArgAction | 2,415.78 ns | 12.667 ns | 11.849 ns | 2,415.59 ns |
