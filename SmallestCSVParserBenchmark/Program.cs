using System.Diagnostics;
using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SmallestCSV;


public class SmallestCSVParserBenchmark
{
    [Benchmark]
    public void ReadCSV() {
        using var sr = new StreamReader("performance1.csv");
        var parser = new SmallestCSVParser(sr);
        var rows = 0;
        while (parser.ReadNextRow() != null) {
            rows++;
        }
        Trace.Assert(rows == 550);
    }

    [Benchmark]
    public void ReadCSV_1_0() {
        using var sr = new StreamReader("performance1.csv");
        var parser = new SmallestCSVParser_1_0(sr);
        var rows = 0;
        while (parser.ReadNextRow() != null) {
            rows++;
        }
        Trace.Assert(rows == 550);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<SmallestCSVParserBenchmark>();
    }
}
