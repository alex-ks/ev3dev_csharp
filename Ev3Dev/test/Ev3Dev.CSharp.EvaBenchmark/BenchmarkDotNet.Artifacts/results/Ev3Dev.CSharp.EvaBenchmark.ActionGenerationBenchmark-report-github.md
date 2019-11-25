``` ini

BenchmarkDotNet=v0.12.0, OS=macOS Mojave 10.14.6 (18G87) [Darwin 18.7.0]
Intel Core i7-4870HQ CPU 2.50GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
  [Host]     : Mono 6.0.0.319 (tarball Wed), X64
  DefaultJob : Mono 6.0.0.319 (tarball Wed), X64


```
|              Method |          Mean |      Error |     StdDev |        Median |
|-------------------- |--------------:|-----------:|-----------:|--------------:|
|  RegularZeroArgCall |     0.0022 ns |  0.0039 ns |  0.0035 ns |     0.0000 ns |
|   RegularOneArgCall |     0.0082 ns |  0.0093 ns |  0.0082 ns |     0.0067 ns |
|   RegularTwoArgCall |     0.0247 ns |  0.0318 ns |  0.0297 ns |     0.0106 ns |
| RegularThreeArgCall |     0.0006 ns |  0.0020 ns |  0.0016 ns |     0.0000 ns |
|  RegularFourArgCall |     0.0018 ns |  0.0024 ns |  0.0022 ns |     0.0012 ns |
|  RegularFiveArgCall |     0.0084 ns |  0.0141 ns |  0.0132 ns |     0.0019 ns |
|   RegularSixArgCall |     9.8726 ns |  0.0658 ns |  0.0583 ns |     9.8792 ns |
|       ZeroArgAction |     2.7912 ns |  0.0442 ns |  0.0392 ns |     2.7866 ns |
|        OneArgAction |     8.3715 ns |  0.0589 ns |  0.0492 ns |     8.3686 ns |
|        TwoArgAction |    11.4109 ns |  0.0400 ns |  0.0334 ns |    11.4022 ns |
|      ThreeArgAction |    14.9420 ns |  0.3345 ns |  0.3435 ns |    14.7967 ns |
|       FourArgAction |    17.8668 ns |  0.0988 ns |  0.0876 ns |    17.8542 ns |
|       FiveArgAction |    20.9789 ns |  0.0826 ns |  0.0732 ns |    20.9656 ns |
|        SixArgAction | 2,415.3454 ns | 12.8929 ns | 11.4292 ns | 2,415.7185 ns |
