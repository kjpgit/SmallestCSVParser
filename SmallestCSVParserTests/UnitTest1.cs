using System.Text.Json;
using SmallestCSV;

[TestClass]
public class SmallestCSVParserTest
{
    public record ReadNextColumnResponse(string? column, bool hasMore);

    [TestMethod]
    public void Test1() {
        using var sr = new StreamReader("test1.csv");
        var parser = new SmallestCSVParser(sr);

        string[][] expected = [
            [ "a", "b", " c ", " 1 2 \"3\"  ", "\"4\"" ],
            [ "d", "e", "f" ],
            [ "", "", "" ],
            [ "" ],
            [ "single_field" ],
            [ "a", "b", " c ",
                " some \r\n emoji \uD83D\uDE0A and \r\nembedded , commas, and \r\n\"new\"\r\nlines ",
                " 3"],
        ];

        for (int i = 0; i < expected.Length; i++) {
            var columns = parser.ReadNextRow();
            Assert.IsNotNull(columns);
            printRow(i+1, columns);
            Assert.AreEqual(expected[i].Length, columns.Count(), "count mismatch");
            for (var j = 0; j < columns.Count(); j++) {
                Assert.AreEqual(expected[i][j], columns[j], "row mismatch");
            }
        }
        foreach (var i in Enumerable.Range(0, 3)) {
            Assert.IsNull(parser.ReadNextRow());
        }
    }

    [TestMethod]
    public void Test1Raw() {
        using var sr = new StreamReader("test1.csv");
        var parser = new SmallestCSVParser(sr);

        string[][] expected = [
            [ "a", "b", " c ", "\" 1 2 \"3\"  \"", "\"\"4\"\"" ],
            [ "d", "e", "f" ],
            [ "", "\"\"", "" ],
            [ "" ],
            [ "single_field" ],
            [ "a", "b", " c ",
                "\" some \r\n emoji \uD83D\uDE0A and \r\nembedded , commas, and \r\n\"new\"\r\nlines \"",
                " 3"],
        ];

        for (int i = 0; i < expected.Length; i++) {
            var columns = parser.ReadNextRow(removeEnclosingQuotes:false);
            Assert.IsNotNull(columns);
            printRow(i+1, columns);
            Assert.AreEqual(expected[i].Length, columns.Count(), "count mismatch");
            for (var j = 0; j < columns.Count(); j++) {
                Assert.AreEqual(expected[i][j], columns[j], "row mismatch");
            }
        }
        foreach (var i in Enumerable.Range(0, 3)) {
            Assert.IsNull(parser.ReadNextRow());
        }
    }

    [TestMethod]
    public void TestErrorQuoteEOF() {
        string data = "\"this_field_is_not_terminated_by_quote";
        using var mem = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));
        using var sr = new StreamReader(mem);
        var parser = new SmallestCSVParser(sr);
        var e = Assert.ThrowsException<SmallestCSVParser.Error>(() => parser.ReadNextRow());
        Assert.AreEqual("EOF reached inside quoted column", e.Message);
    }

    [TestMethod]
    public void TestErrorUnknownChar() {
        string data = "\"this_field_has_garbage_after\" ";
        using var mem = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));
        using var sr = new StreamReader(mem);
        var parser = new SmallestCSVParser(sr);
        var e = Assert.ThrowsException<SmallestCSVParser.Error>(() => parser.ReadNextRow());
        Assert.AreEqual("Unrecognized character ' ' after a parsed column", e.Message);
    }

    private void printRow(int rowNum, List<string> columns) {
        Console.WriteLine($"-- Read row {rowNum}");
        for (var i = 0; i < columns.Count(); i++) {
            Console.WriteLine($"column {escapeText(columns[i])}");
        }
    }

    private string escapeText(string? s) {
        return JsonSerializer.Serialize(s) ?? "(null)";
    }

}
