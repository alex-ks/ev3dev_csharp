using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Running;

namespace Ev3Dev.CSharp.EvaBenchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ActionGenerationBenchmark>();
        }
    }
}