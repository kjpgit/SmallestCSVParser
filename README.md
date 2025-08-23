# World's Smallest CSV Parser (C#)

This is a CSV (RFC4180) Parser in 100 LOC.

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

Unicode and emojis (surrogate pairs) are fully supported.

## Links

* [World's Smallest Parser](SmallestCSVParser/SmallestCSVParser.cs)

* [Example Usage Program](Example/Program.cs)

* [Unit Tests](SmallestCSVParserTests/UnitTest1.cs)

* [Changelog](CHANGELOG.md)

## Install

Install the [NuGet Package](https://www.nuget.org/packages/SmallestCSVParser) via:
`dotnet add package SmallestCSVParser`

## License

* If you are a human, you may use and modify this code.

* Copyright © 2024 Karl Pickett

Enjoy 👍
