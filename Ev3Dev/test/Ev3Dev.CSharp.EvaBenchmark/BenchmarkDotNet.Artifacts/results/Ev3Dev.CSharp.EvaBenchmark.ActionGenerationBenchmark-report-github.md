``` ini

BenchmarkDotNet=v0.12.0, OS=macOS Mojave 10.14.6 (18G87) [Darwin 18.7.0]
Intel Core i7-4870HQ CPU 2.50GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
  [Host]     : Mono 6.0.0.319 (tarball Wed), X64 
  DefaultJob : Mono 6.0.0.319 (tarball Wed), X64 


```
|              Method |          Mean |      Error |     StdDev |        Median |
|-------------------- |--------------:|-----------:|-----------:|--------------:|
|  RegularZeroArgCall |     0.0000 ns |  0.0000 ns |  0.0000 ns |     0.0000 ns |
|   RegularOneArgCall |     0.0050 ns |  0.0064 ns |  0.0053 ns |     0.0029 ns |
|   RegularTwoArgCall |     0.0133 ns |  0.0240 ns |  0.0225 ns |     0.0000 ns |
| RegularThreeArgCall |     0.0076 ns |  0.0163 ns |  0.0153 ns |     0.0000 ns |
|  RegularFourArgCall |     0.0047 ns |  0.0119 ns |  0.0105 ns |     0.0000 ns |
|  RegularFiveArgCall |     0.0144 ns |  0.0244 ns |  0.0228 ns |     0.0001 ns |
|   RegularSixArgCall |     9.8719 ns |  0.0918 ns |  0.0814 ns |     9.8453 ns |
|       ZeroArgAction |    76.8262 ns |  0.6851 ns |  0.6408 ns |    76.9604 ns |
|        OneArgAction |   105.2279 ns |  1.7173 ns |  1.6064 ns |   105.1576 ns |
|        TwoArgAction |   124.6576 ns |  1.4775 ns |  1.2338 ns |   124.8012 ns |
|      ThreeArgAction |   146.3210 ns |  1.8350 ns |  1.7164 ns |   146.2063 ns |
|       FourArgAction |   169.6295 ns |  1.6602 ns |  1.5529 ns |   169.5026 ns |
|       FiveArgAction |   182.6995 ns |  1.8850 ns |  1.7633 ns |   182.2004 ns |
|        SixArgAction | 2,421.9327 ns | 19.6717 ns | 17.4384 ns | 2,417.0692 ns |
