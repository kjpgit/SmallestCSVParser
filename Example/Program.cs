using var sr = new StreamReader("test1.csv");
var parser = new SmallestCSV.SmallestCSVParser(sr);

while (true) {
    List<string>? columns = parser.ReadNextRow();
    if (columns == null) {
        Console.WriteLine("End of file reached");
        break;
    }
    var prettyRow = System.Text.Json.JsonSerializer.Serialize(columns);
    Console.WriteLine($"Read row: {prettyRow}");
}
