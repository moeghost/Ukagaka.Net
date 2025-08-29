using System;

namespace aya.Eval
{
    public class Value : ICloneable
    {
        protected const int TypeInteger = 0;
        protected const int TypeReal = 1;
        protected const int TypeString = 2;

        private int type;
        private long integer;
        private double real;
        private string str;

        public Value(long integer)
        {
            SetInteger(integer);
        }

        public Value(double real)
        {
            SetReal(real);
        }

        public Value(string str)
        {
            SetString(str);
        }

        public object Clone()
        {
            return IsInteger() ? new Value(integer) : IsReal() ? new Value(real) : new Value(str);
        }

        public Value SetInteger()
        {
            return SetInteger(GetInteger());
        }

        public Value SetReal()
        {
            return SetReal(GetReal());
        }

        public Value SetString()
        {
            return SetString(GetString());
        }

        public Value SetInteger(long integer)
        {
            type = TypeInteger;
            this.integer = integer;
            real = 0;
            str = null;
            return this;
        }

        public Value SetReal(double real)
        {
            if (real == Math.Floor(real))
            {
                SetInteger((long)real);
            }
            else
            {
                type = TypeReal;
                integer = 0;
                this.real = real;
                str = null;
            }
            return this;
        }

        public Value SetString(string str)
        {
            type = TypeString;
            integer = 0;
            real = 0;
            this.str = str;
            return this;
        }

        public bool IsNumeric()
        {
            return type == TypeInteger || type == TypeReal;
        }

        public bool IsInteger()
        {
            return type == TypeInteger;
        }

        public bool IsReal()
        {
            return type == TypeReal;
        }

        public bool IsString()
        {
            return type == TypeString;
        }

        public long GetInteger()
        {
            return type switch
            {
                TypeInteger => integer,
                TypeReal => (long)real,
                TypeString => long.TryParse(str, out long result) ? result : 0,
                _ => 0,
            };
        }

        public double GetReal()
        {
            return type switch
            {
                TypeInteger => (double)integer,
                TypeReal => real,
                TypeString => double.TryParse(str, out double result) ? result : 0,
                _ => 0,
            };
        }

        public string GetString()
        {
            return type switch
            {
                TypeString => str ?? "",
                TypeInteger => integer.ToString(),
                _ => real.ToString(),
            };
        }

        public override string ToString()
        {
            return type switch
            {
                TypeString => '"' + (str ?? "") + '"',
                TypeInteger => integer.ToString(),
                _ => real.ToString(),
            };
        }
    }
}
