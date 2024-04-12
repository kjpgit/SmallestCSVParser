using System.Text;
using System.Text.Json;
using SmallestCSV;

[TestClass]
public class SmallestCSVParserTest
{
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

        var rows = readAllRows(() => parser.ReadNextRow()).ToList();
        compareRows(expected: expected, actual: rows);
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

        var rows = readAllRows(() => parser.ReadNextRow(removeEnclosingQuotes:false)).ToList();
        compareRows(expected: expected, actual: rows);
    }

    [TestMethod]
    public void TestErrorQuoteEOF() {
        string data = "\"this_field_is_not_terminated_by_quote";
        using var sr = streamReaderFromString(data);
        var parser = new SmallestCSVParser(sr);
        var e = Assert.ThrowsException<SmallestCSVParser.Error>(() => parser.ReadNextRow());
        Assert.AreEqual("EOF reached inside quoted column", e.Message);
    }

    [TestMethod]
    public void TestErrorUnknownChar() {
        string data = "\"this_field_has_garbage_after\" ";
        using var sr = streamReaderFromString(data);
        var parser = new SmallestCSVParser(sr);
        var e = Assert.ThrowsException<SmallestCSVParser.Error>(() => parser.ReadNextRow());
        Assert.AreEqual("Unrecognized character ' ' after a parsed column", e.Message);
    }

    [TestMethod]
    public void TestEOFAfterField() {
        string data = "abc,this_is_terminated_by_eof";
        using var sr = streamReaderFromString(data);
        var parser = new SmallestCSVParser(sr);

        string[][] expected = [ [ "abc", "this_is_terminated_by_eof" ] ];

        var rows = readAllRows(() => parser.ReadNextRow()).ToList();
        compareRows(expected: expected, actual: rows);
    }

    [TestMethod]
    public void TestConformingEmbeddedWhitespace() {
        string data = "\"all whitespace\r is\n legal\r\n in a quoted\t\v field\"";
        using var sr = streamReaderFromString(data);

        var parser = new SmallestCSVParser(sr);
        var columns = parser.ReadNextRow();
        Assert.IsNotNull(columns);
        Assert.AreEqual(1, columns.Count());
        Assert.AreEqual(data[1..^1], columns[0]);
        Assert.IsNull(parser.ReadNextRow());

        using var sr2 = streamReaderFromString(data);
        parser = new SmallestCSVParser(sr2);
        columns = parser.ReadNextRow(removeEnclosingQuotes:false);
        Assert.IsNotNull(columns);
        Assert.AreEqual(1, columns.Count());
        Assert.AreEqual(data, columns[0]);
        Assert.IsNull(parser.ReadNextRow());
    }

    [TestMethod]
    public void TestNonConformingEmbeddedWhitespace() {
        // '\r' and '\n' are (supposedly) not allowed in a non-quoted field.
        // If we see them, we treat them as equivalent to '\r\n'.  Most likely,
        // the csv was just written with unix ('\n') line endings, not windows ('\r\n').
        string data = "Bad1\rBad2\nOK1\r\n";
        using var sr = streamReaderFromString(data);
        var parser = new SmallestCSVParser(sr);

        // This is 3 rows
        string[][] expected = [ [ "Bad1" ], [ "Bad2" ], [ "OK1" ] ];

        var rows = readAllRows(() => parser.ReadNextRow()).ToList();
        compareRows(expected: expected, actual: rows);
    }

    private IEnumerable<List<string>> readAllRows(Func<List<string>?> action) {
        while (true) {
            var row = action();
            if (row == null) {
                break;
            }
            printRow(row);
            yield return row;
        }
        // Make sure more calls still return null...
        Assert.IsNull(action());
        Assert.IsNull(action());
    }

    private StreamReader streamReaderFromString(string data) {
        // The StreamReader when disposed, will also dispose the memory stream
        var mem = new MemoryStream(Encoding.UTF8.GetBytes(data));
        return new StreamReader(mem);
    }

    private void compareRows(string[][] expected, List<List<string>> actual) {
        Assert.AreEqual(expected.Length, actual.Count(), "row count mismatch");
        for (int i = 0; i < expected.Length; i++) {
            Assert.AreEqual(expected[i].Length, actual[i].Count(), "column count mismatch");
            for (var j = 0; j < expected[i].Length; j++) {
                Assert.AreEqual(expected[i][j], actual[i][j], "column data mismatch");
            }
        }
    }

    private void printRow(List<string> columns) {
        Console.WriteLine($"-- Read row");
        for (var i = 0; i < columns.Count(); i++) {
            Console.WriteLine($"column {escapeText(columns[i])}");
        }
    }

    private string escapeText(string s) {
        return JsonSerializer.Serialize(s);
    }

}
