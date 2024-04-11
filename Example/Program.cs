using var sr = new StreamReader("test1.csv");
var parser = new SmallestCSV.SmallestCSVParser(sr);

// Set this to true if you don't care about preserving the quotes around fields.
// (Sometimes that is used to distinguish a null vs empty field.)
const bool removeEnclosingQuotes=false;

while (true) {
    List<string>? columns = parser.ReadNextRow(removeEnclosingQuotes:removeEnclosingQuotes);
    if (columns == null) {
        Console.WriteLine("End of file reached");
        break;
    }
    var prettyRow = System.Text.Json.JsonSerializer.Serialize(columns);
    Console.WriteLine($"Read row: {prettyRow}");
}
