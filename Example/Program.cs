// The parser does not Close (Dispose) the StreamReader.
// The calling code is responsible, so we add `using` here.
using var sr = new StreamReader("test1.csv");

var parser = new SmallestCSV.SmallestCSVParser(sr);

// Set this to true (the default) if you don't want to preserve double quotes around fields.
// Set this to false if you need to distinguish a null (,,) vs empty string (,"",) field.
const bool removeEnclosingQuotes = true;

while (true) {
    List<string>? columns = parser.ReadNextRow(removeEnclosingQuotes: removeEnclosingQuotes);
    if (columns == null) {
        Console.WriteLine("End of file reached");
        break;
    }
    var prettyRow = System.Text.Json.JsonSerializer.Serialize(columns);
    Console.WriteLine($"Read row: {prettyRow}");
}
