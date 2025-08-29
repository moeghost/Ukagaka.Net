using LittleGrayCalculator.Cores;
using System;

namespace aya.Eval
{
    public class Value : ICloneable
    {
        protected const int TypeInteger = 0;
        protected const int TypeReal = 1;
        protected const int TypeString = 2;
        protected const int TypeBigNumber = 3;
        protected const int TypeBool = 4;
        private int type;
        private long integer;
        private double real;
        private string str;
        private BigNumber bigNumber;
        private bool boolValue;
        public Value(BigNumber bigNumber)
        {
            SetBigNumber(bigNumber);
        }

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

        public Value(bool boolValue)
        {
            SetBool(boolValue);

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
            bigNumber = new BigNumber("0");
            str = null;
            return this;
        }

        public Value SetBigNumber(BigNumber bigNumber)
        {
            type = TypeBigNumber;
            this.bigNumber = bigNumber;
            integer = bigNumber.ToInt();
            str = null;
            real = 0;
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
                bigNumber = new BigNumber("0");
                this.real = real;
                this.boolValue = false;
                str = null;
            }
            return this;
        }


        public Value SetBool(bool value)
        {

            type = TypeBool;
            integer = 0;
            bigNumber = new BigNumber("0");
            this.real = 0;
            this.boolValue = value;
            str = null;

            return this;
        }



        public Value SetString(string str)
        {
            type = TypeString;
            integer = 0;
            real = 0;
            bigNumber = new BigNumber("0");
            this.str = str == "" ? "0" : str;
            return this;
        }

        public bool IsNumeric()
        {
            return type == TypeInteger || type == TypeReal || type == TypeBigNumber || type == TypeBool;
        }

        public bool IsInteger()
        {
            return type == TypeInteger;
        }

        public bool IsReal()
        {
            return type == TypeReal;
        }
        public bool IsBigNumber()
        {
            return type == TypeBigNumber;
        }

        public bool IsBool()
        {
            return type == TypeBool;
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
                TypeBigNumber => bigNumber.ToInt(),
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
                TypeBigNumber => bigNumber.ToDouble(),
                _ => 0,
            };
        }

        public bool GetBool()
        {
            return type switch
            {
                TypeInteger => (bool)(integer > 0),
                TypeReal => (bool)(real > 0),
                TypeString => bool.TryParse(str, out bool result) ? result : false,
                TypeBigNumber => bigNumber.ToBool(),
                _ => false,
            };
        }


        public BigNumber GetBigNumber()
        {
            return type switch
            {
                TypeInteger => new BigNumber(integer.ToString()),
                TypeReal => new BigNumber(real.ToString()),
               // TypeString => double.TryParse(str, out double result) ? result : 0,
               TypeBigNumber => bigNumber,
                TypeString => new BigNumber(str),
                 _ => new BigNumber("0"),
            };
        }

        public string GetString()
        {
            return type switch
            {
                TypeString => str ?? "",
                TypeInteger => integer.ToString(),
                TypeReal => real.ToString(),
                TypeBigNumber => bigNumber.ToString(),
                _ => real.ToString(),
            };
        }

        public override string ToString()
        {
            return type switch
            {
                TypeString => '"' + (str ?? "") + '"',
                TypeInteger => integer.ToString(),
                TypeReal => real.ToString(),
                TypeBigNumber => bigNumber.ToString(),
                _ => real.ToString(),
            };
        }
    }
}
