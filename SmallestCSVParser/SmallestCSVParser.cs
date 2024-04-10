using System.Diagnostics;
using System.Text;

namespace SmallestCSV;


public class SmallestCSVParser
{
    public class Error: Exception {
        public Error(string message): base(message) { }
    }

    public SmallestCSVParser(StreamReader stream) {
        _stream = stream;
        _sb = new();
    }

    /*
       Read all columns for the next row/line.
       If we are at end of file, this returns null.

       By default, columns that were quoted (") have their enclosing quotes
       removed.  Set `removeEnclosingQuotes` to false if you want to preserve
       the quotes, for example to distinguish between an empty quoted vs
       unquoted column.
    */
    public List<string>? ReadNextRow(bool removeEnclosingQuotes=true) {
        List<string> ret = new();
        while (true) {
            var (column, hasMore) = ReadNextColumn();
            if (column != null) {
                if (removeEnclosingQuotes && column.StartsWith('"')) {
                    Trace.Assert(column.EndsWith('"'));
                    column = column.Substring(1, column.Length - 2);
                }
                ret.Add(column);
            }
            if (!hasMore) {
                return ret.Any() ? ret : null;
            }
        }
    }

    private (string? Column, bool RowHasMoreColumns) ReadNextColumn() {
        _sb.Clear();
        switch (_stream.Peek()) {
            case -1:
                return (null, false);
            case '"':
                _sb.Append((char)_stream.Read());
                foreach (var ch in ReadQuotedColumn()) { _sb.Append(ch); }
                return (_sb.ToString(), !TryFinishLine());
            default:
                foreach (var ch in ReadNonQuotedColumn()) { _sb.Append(ch); }
                return (_sb.ToString(), !TryFinishLine());
        }
    }

    private IEnumerable<char> ReadQuotedColumn() {
        while (true) {
            var ch = _stream.Read();
            var lookAheadChar = _stream.Peek();
            switch (ch, lookAheadChar) {
                case (-1, _):
                    throw new Error("EOF reached inside quoted column");
                case ('"', '"'):
                    // A "" is an escaped "
                    _stream.Read();
                    yield return '"';
                    break;
                case ('"', _):
                    // A " followed by any other char means we're at the end
                    yield return '"';
                    yield break;
                default:
                    yield return (char)ch;
                    break;
            }
        }
    }

    private IEnumerable<char> ReadNonQuotedColumn() {
        while (true) {
            var ch = _stream.Peek();
            // We aren't consuming the '\r' here. A later call to ReadWithNormalizedNewlines will consume it.
            if (ch == -1 || ch == '\r' || ch == '\n' || ch == ',') {
                yield break;
            }
            _stream.Read();
            yield return (char)ch;
        }
    }

    // Maps ('\r', '\n', '\r\n') -> '\n'
    private int ReadWithNormalizedNewline() {
        int ret = _stream.Read();
        if (ret == '\r') {
            if (_stream.Peek() == '\n') {
                _stream.Read();
            }
            ret = '\n';
        }
        return ret;
    }

    private bool TryFinishLine() {
        var ch = ReadWithNormalizedNewline();
        switch (ch) {
            case -1:
            case '\n':
                return true;  // All columns parsed for this row/line
            case ',':
                return false; // More columns remain for this row/line
            default:
                throw new Error($"Unrecognized character '{(char)ch}' after a parsed column");
        }
    }

    private readonly StreamReader _stream;
    private readonly StringBuilder _sb;
}
