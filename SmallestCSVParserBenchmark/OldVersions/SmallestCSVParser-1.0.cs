// SmallestCSVParser version 1.0 - Copyright (C) 2024 Karl Pickett
using System.Diagnostics;
using System.Text;

namespace SmallestCSV;


public class SmallestCSVParser_1_0
{
    public class Error: Exception {
        public Error(string message): base(message) { }
    }

    public SmallestCSVParser_1_0(StreamReader stream) {
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
            var (column, hasMore) = ReadNextColumn(removeEnclosingQuotes);
            if (column != null) {
                ret.Add(column);
            }
            if (!hasMore) {
                return ret.Any() ? ret : null;
            }
        }
    }

    private (string? Column, bool RowHasMoreColumns) ReadNextColumn(bool removeEnclosingQuotes) {
        _sb.Clear();
        switch (_stream.Peek()) {
            case -1:
                return (null, false);
            case '"':
                ReadQuotedColumn(removeEnclosingQuotes);
                return (_sb.ToString(), !TryFinishLine());
            default:
                ReadNonQuotedColumn();
                return (_sb.ToString(), !TryFinishLine());
        }
    }

    private void ReadQuotedColumn(bool removeEnclosingQuotes) {
        _stream.Read();  // Remove the quote from the stream
        if (!removeEnclosingQuotes) {
            _sb.Append('"');  // Optionally keep the quote in the result
        }
        while (true) {
            var ch = _stream.Read();
            var lookAheadChar = _stream.Peek();
            switch (ch, lookAheadChar) {
                case (-1, _):
                    throw new Error("EOF reached inside quoted column");
                case ('"', '"'):
                    // A "" is an escaped "
                    _stream.Read();
                    _sb.Append('"');
                    break;
                case ('"', _):
                    // A " followed by any other char means we're at the end
                    if (!removeEnclosingQuotes) {
                        _sb.Append('"');
                    }
                    return;
                default:
                    _sb.Append((char)ch);
                    break;
            }
        }
    }

    private void ReadNonQuotedColumn() {
        while (true) {
            var ch = _stream.Peek();
            // We aren't consuming the '\r' here. A later call to ReadWithNormalizedNewlines will consume it.
            if (ch == -1 || ch == '\r' || ch == '\n' || ch == ',') {
                return;
            }
            _stream.Read();
            _sb.Append((char)ch);
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
