using System;
using System.Text;

/// <summary>
/// A BsonTokener takes a byte array and extracts elements from it. It is used by
/// the BSONDocument and BSONArray constructors to parse BSON data.
/// </summary>
///
public class BonJsonTokener {
    private int myIndex;
    private readonly char[] mySource;

    /// <summary>
    /// Construct a JSONTokener from a string.
    /// </summary>
    ///
    /// <param name="s">A source string.</param>
    public BonJsonTokener(string json) {
        this.mySource = json.ToCharArray();
    }

    /// <summary>
    /// Determine if the source string still contains characters that next() can
    /// consume.
    /// </summary>
    ///
    /// <returns>true if not yet at the End of the source.</returns>
    private bool More() {
        return IsIndexInBounds(this.myIndex);
    }

    private bool IsIndexInBounds(int idx) {
        return idx < mySource.Length;
    }

    /// <summary>
    /// Get the next character in the source string.
    /// </summary>
    ///
    /// <returns>The next character, or 0 if past the End of the source string.</returns>
    private char Next() {
        if(More()) {
            char b = this.mySource[this.myIndex++];
            return b;
        }

        throw SyntaxError("bounds error");
    }

    /// <summary>
    /// Make a JSONException to signal a syntax error.
    /// </summary>
    ///
    /// <param name="message">The error message.</param>
    /// <returns>A JSONException object, suitable for throwing</returns>
    private BonException SyntaxError(String message) {
        return new BonException(message + ToString());
    }

    /// <summary>
    /// Make a printable string of this JSONTokener.
    /// </summary>
    ///
    /// <returns>" at character [this.myIndex] of [this.mySource]"</returns>
    public override String ToString() {
        return " at owner " + this.myIndex;
    }

    private readonly char[] doubleTip = new char[] { '.', 'e', 'E' };

    private BonValue NextNumber() {
        myIndex--;
        int startIndex = myIndex;
        while(true) {
            char c = Next();
            if((c >= '0' && c <= '9') || c == '.' || c == '-' || c == '+' || c == 'e' || c == 'E') {
                continue;
            }
            break;
        }
        myIndex--;
        string s = new string(mySource, startIndex, myIndex - startIndex);
        if(s.IndexOfAny(doubleTip) >= 0) {
            double d;
            if(double.TryParse(s, out d)) {
                BonDouble v = new BonDouble(d);
                return v;
            }
            throw SyntaxError("Cast Double error");
        }

        int i;
        if(int.TryParse(s, out i)) {
            BonInt v = new BonInt(i);
            return v;
        }

        long l;
        if(long.TryParse(s, out l)) {
            BonLong v = new BonLong(l);
            return v;
        }
        throw SyntaxError("Cast Int32 error");
    }

    private string GetNextString() {
        StringBuilder sb = new StringBuilder();
        while(true) {
            char c = Next();
            if(c == '\\') {
                c = Next();
                switch(c) {
                    case '"':
                    sb.Append('"');
                    break;
                    case '\\':
                    sb.Append('\\');
                    break;
                    case '/':
                    sb.Append('/');
                    break;
                    case 'b':
                    sb.Append('\b');
                    break;
                    case 'f':
                    sb.Append('\f');
                    break;
                    case 'n':
                    sb.Append('\n');
                    break;
                    case 'r':
                    sb.Append('\r');
                    break;
                    case 't':
                    sb.Append('\t');
                    break;
                    case 'u': {
                        char c1 = Next();
                        char c2 = Next();
                        char c3 = Next();
                        char c4 = Next();
                        c = (char)ParseUnicode(c1, c2, c3, c4);
                        sb.Append(c);
                        break;
                    }
                }
            } else {
                if(c == '\"') {// || c == '\'') {
                    break;
                }
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private uint ParseSingleChar(char c1, uint multipliyer) {
        uint p1 = 0;
        if(c1 >= '0' && c1 <= '9')
            p1 = (uint)(c1 - '0') * multipliyer;
        else if(c1 >= 'A' && c1 <= 'F')
            p1 = (uint)((c1 - 'A') + 10) * multipliyer;
        else if(c1 >= 'a' && c1 <= 'f')
            p1 = (uint)((c1 - 'a') + 10) * multipliyer;
        return p1;
    }

    private uint ParseUnicode(char c1, char c2, char c3, char c4) {
        uint p1 = ParseSingleChar(c1, 0x1000);
        uint p2 = ParseSingleChar(c2, 0x100);
        uint p3 = ParseSingleChar(c3, 0x10);
        uint p4 = ParseSingleChar(c4, 1);
        return p1 + p2 + p3 + p4;
    }

    private BonValue NextString() {
        BonString v = new BonString(GetNextString());
        return v;
    }

    private BonValue NextBoolean() {
        myIndex--;
        int startIndex = myIndex;
        char c = Next();
        if(c == 'f') {
            if(!IsIndexInBounds(startIndex + 5 - 1))
                throw SyntaxError("Cast false error");
            if(new string(mySource, startIndex, 5) != "false")
                throw SyntaxError("Cast false error");
            myIndex += 4;
            BonBoolean v = new BonBoolean(false);
            return v;
        } else {
            if(!IsIndexInBounds(startIndex + 4 - 1))
                throw SyntaxError("Cast true error");
            if(new string(mySource, startIndex, 4) != "true")
                throw SyntaxError("Cast true error");
            myIndex += 3;
            BonBoolean v = new BonBoolean(true);
            return v;
        }
    }

    //private BsonDateTime NextDateTime() {
    //    myIndex--;
    //    if(!IsIndexInBounds(myIndex + 9 - 1)) throw SyntaxError("Cast DateTime error");
    //    if(new string(mySource, myIndex, 9) != "ISODate(\"") throw SyntaxError("Cast DateTime error");
    //    myIndex += 9;
    //    int startIndex = myIndex;
    //    while(true) {
    //        char c = Next();
    //        if(c == '\"') {
    //            break;
    //        }
    //    }
    //    string s = new string(mySource, startIndex, myIndex - startIndex - 1);
    //    Next();
    //    string[] formats = new string[] { "yyyy-MM-ddK", "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK" };
    //    DateTime value = DateTime.ParseExact(s, formats, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    //    return new BsonDateTime(value);
    //}

    private BonValue NextNull() {
        myIndex--;
        int startIndex = myIndex;
        if(!IsIndexInBounds(startIndex + 4 - 1))
            throw SyntaxError("Cast null error");
        if(new string(mySource, startIndex, 4) != "null")
            throw SyntaxError("Cast null error");
        myIndex += 4;
        return BonNull.value;
    }

    private BonDocument NextDocument() {
        BonDocument d = new BonDocument();
        while(true) {
            char c = NextToken();
            if(c == '}')
                break;
            if(c != '\"')// && c != '\'')
                throw SyntaxError("Document error");
            string k = GetNextString();
            if(NextToken() != ':')
                throw SyntaxError("Document error");
            BonValue v = NextValue();
            d[k] = v;
            c = NextToken();
            if(c == '}')
                break;
            if(c != ',') {
                string txt = "";
                int ends = myIndex + 20;
                ends = ends < (mySource.Length - 1) ? ends : mySource.Length - 1;
                for(int i = myIndex - 1; i < myIndex + 10; i++) {
                    txt += mySource[i];
                }
                throw SyntaxError("Document error token [" + txt + "]");
            }
        }
        return d;
    }

    private BonArray NextArray() {
        BonArray b = new BonArray();
        while(true) {
            char c = NextToken();
            if(c == ']')
                break;
            myIndex--;
            BonValue v = NextValue();
            b.Add(v);
            c = NextToken();
            if(c == ']')
                break;
            if(c != ',')
                throw SyntaxError("Array error");
        }
        return b;
    }

    public BonValue NextValue() {
        char c = NextToken();
        switch(c) {
            case '{':
            return NextDocument();
            case '[':
            return NextArray();
            //case '\'':
            case '\"':
            return NextString();
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case '-':
            case '+':
            case '.':
            return NextNumber();
            //case 'N': return NextInt64();
            case 'f':
            case 't':
            return NextBoolean();
            //case 'I': return NextDateTime();
            case 'n':
            return NextNull();
            default:
            throw SyntaxError("Unkown Token");
        }

    }

    private char NextToken() {
        char c;
        while(true) {
            c = Next();
            if(c == 0)
                throw SyntaxError("bounds error");
            if(c != ' ' && c != '\t' && c != '\n' && c != '\r') {
                return c;
            }
        }
    }
}
