 
namespace aya
{
    public class Token
    {
        private readonly int _type;
        private readonly string _token;
        private readonly bool _spaceWasBeforeToken;
        private readonly Position _position;

        public Token(int type, string token, bool spaceWasBeforeToken)
        {
            _type = type;
            _token = token;
            _spaceWasBeforeToken = spaceWasBeforeToken;
            _position = new Position();
        }

        public Token(int type, string token, bool spaceWasBeforeToken, int row)
            : this(type, token, spaceWasBeforeToken)
        {
            _position = new Position(row);
        }

        public int Type => _type;

        public bool IsIdentifier => _type == LexicalAnalyzer.TYPE_IDENTIFIER;

        public bool IsSymbol => _type == LexicalAnalyzer.TYPE_SYMBOL;

        public bool IsNumber => _type == LexicalAnalyzer.TYPE_NUMBER;

        public bool IsString => _type == LexicalAnalyzer.TYPE_STRING;

        public string GetToken => _token;

        public bool WasSpaceBefore => _spaceWasBeforeToken;

        public Position GetPosition => _position;

        public override string ToString()
        {
            return "{" + _type + ":" + _token + "}";
        }

        public class Position
        {
            private readonly int _row;

            public Position()
            {
                _row = 0;
            }

            public int Row => _row;

            public Position(int row)
            {
                _row = row;
            }

            public override string ToString()
            {
                return "line " + _row;
            }
        }
    }
}
