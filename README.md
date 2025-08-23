# World's Smallest CSV Parser (C#)

This is a CSV ([RFC4180](https://www.rfc-editor.org/rfc/rfc4180)) Parser in 100 LOC.

It does not give you spaghetti abstractions like other libraries.  The API is
simply this:

```cs
while (true) {
    List<string>? columns = parser.ReadNextRow();
    if (columns == null) {
        Console.WriteLine("End of file reached");
        break;
    }
    var prettyRow = System.Text.Json.JsonSerializer.Serialize(columns);
    Console.WriteLine($"Read row: {prettyRow}");
}
```

You can easily create your own high level DictReader or whatever extravagance
you want, for example supporting an Excel "column groups feature" (multiple
headers having the same name, and/or headers with empty names that are expected
to inherit the previous header's name), or mapping "NA" and "N/A" to null, etc.,
which is very far outside the scope of RFC4180.

Unicode and emojis (surrogate pairs) are fully supported.  The Unicode encoding
(UTF8, etc.) is fully under your control, and is handled by the StreamReader you pass in.

## Links

* [World's Smallest Parser](SmallestCSVParser/SmallestCSVParser.cs)

* [Example Usage Program](Example/Program.cs)

* [Unit Tests](SmallestCSVParserTests/UnitTest1.cs)

* [Changelog](CHANGELOG.md)

## Install

Install the [NuGet Package](https://www.nuget.org/packages/SmallestCSVParser) via:
`dotnet add package SmallestCSVParser`

## Performance

According to [CsharpCSVBench](https://github.com/lemire/CsharpCSVBench/tree/master), SmallestCSVParser 1.2.0 gets 75% the performance of
CsvHelper, with only 1% of the lines of code (100 versus 9700), and a simpler API .
Choose your tradeoff.

```
                   | Mean      | Error     | StdDev    | Speed (GB/s) | Allocated   |
 CsvHelper         | 33.270 ms | 0.0961 ms | 0.0802 ms |  0.354       | 18266.48 KB |
 SmallestCSVParser | 44.026 ms | 0.1987 ms | 0.1859 ms |  0.268       | 55299.09 KB |
```


## License

* If you are a human, you may use and modify this code.

* Copyright © 2024-2025 Karl Pickett

Enjoy 👍
