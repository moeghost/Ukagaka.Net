using aya.Eval;
using System;
using System.Collections.Generic;
using System.Text;
 
namespace LittleGrayCalculator.Cores
{
    /// <summary>表示大数的类</summary>
    public class BigNumber : IComparable<BigNumber>
    {

        public static int precision = 25;
         

        /// <summary>一位上存储的数位</summary>
        public static readonly int OneCount = 4;
        
        /// <summary>进位的数</summary>
        public static readonly int Max = (int)Math.Pow(10, OneCount);

        public static readonly BigNumber One = new BigNumber("1");
        public static readonly BigNumber Zero = new BigNumber("0");



        public BigNumber(string text) {
            intPart = new List<int>();
            decimalPart = new List<int>();
            IsPlus = IdentifyNumber.GetBigNumber(text, intPart, decimalPart);
        }


        public BigNumber(List<int> i, List<int> d, bool plus) {
            this.IntPart = i;
            this.DecimalPart = d;
            this.IsPlus = plus;
        }

        public int ToInt()
        {
            try
            {
                int value = Convert.ToInt32(this.ToString());
                return value;
            }
            catch
            {
                return 0;
            }
            
        }

        public double ToDouble()
        {
            try
            {
                double value = Convert.ToDouble(this.ToString());
                return value;
            }
            catch
            {
                return 0;
            }
           
        }

        public bool ToBool()
        {
            try
            {
                bool value = Convert.ToBoolean(this.ToString());
                return value;
            }
            catch
            {
                return false;
            }

        }


        #region "属性"
        //整数部分与小数部分都是从左到右存储
        List<int> intPart;
        List<int> decimalPart;

        /// <summary>整数部分 </summary>
        public List<int> IntPart {
            get { return intPart; }
            private set {
                intPart = new List<int>(value);
                //intPart = value;
            }
        }

        /// <summary>整数部分的长度</summary>
        public int IntLength {
            get { return IntPart.Count * OneCount; }
        }

        /// <summary>
        /// 小数部分
        /// </summary>
        public List<int> DecimalPart {
            get { return decimalPart; }
            private set {
                decimalPart = new List<int>(value);
            }
        }
        /// <summary>小数部分的实际长度</summary>
        public int DecimalLength {
            get { return DecimalPart.Count * OneCount; }
        }

        /// <summary>返回0.00001这样的数小数点后、有效数前有几个零</summary>
        internal int GetPrecision(int value) 
        {
            if (value == 0) {
                BigCalculate.RemoveStartZero(IntPart);
                if (IntPart.Count == 1 && IntPart[0] == 0) 
                {
                    if (DecimalPart.Count == 0)
                    {
                        return 0;
                    }
                    for (int i = 0; i < DecimalPart.Count; i++) 
                    {
                        if (DecimalPart[i] != 0)
                        {
                            return i;
                        }
                    }
                    return DecimalPart.Count;
                }
                return 0;
            } 
            else 
            {
                if (IntPart.Count == 1 && IntPart[0] == 1) 
                {
                    if (DecimalPart.Count == 0)
                    {
                        return 0;
                    }
                    for (int i = 0; i < DecimalPart.Count; i++) 
                    {
                        if (DecimalPart[i] != 0)
                        {
                            return i;
                        }
                    }
                    return DecimalPart.Count;
                }
                return 0;
            }
        }

        /// <summary>
        /// 是否是正数
        /// </summary>
        public bool IsPlus { get; set; }

        /// <summary>
        /// 返回此实例的绝对值
        /// </summary>
        public BigNumber AbsoluteNumber {
            get { return new BigNumber(IntPart, DecimalPart, true); }
        }

        /// <summary>
        /// 返回此实例的相反数
        /// </summary>
        public BigNumber ReverseNumber {
            get { return new BigNumber(IntPart, DecimalPart, !IsPlus); }
        }

        /// <summary>大数中的零对象</summary>
        /*
        public static BigNumber Zero 
        {
            get {
                List<int> intlist = new List<int>();
                List<int> decimallist = new List<int>();
                intlist.Add(0);
                return new BigNumber(intlist, decimallist, true);
            }
        }
        */
        public BigNumber Clone() {
            return new BigNumber(IntPart, DecimalPart, IsPlus);
        }

        /// <summary>保留小数点后n位</summary>
        public BigNumber KeepPrecision(int n) 
        {
            if (this.DecimalLength > n) 
            {
                this.DecimalPart.RemoveRange(n / BigNumber.OneCount, DecimalPart.Count - n / BigNumber.OneCount);
            }
            return this;
        }

        #endregion

        #region "重载运算符"
        public static BigNumber operator +(BigNumber a, BigNumber b) {
            return BigCalculate.Add(a, b);
        }
        public static BigNumber operator +(int a, BigNumber b) {
            return (new BigNumber(a.ToString())) + b;
        }
        public static BigNumber operator -(BigNumber a, BigNumber b) {
            return BigCalculate.Minus(a, b);
        }
        public static BigNumber operator *(BigNumber a, BigNumber b) {
            return BigCalculate.Multiply(a, b);
        }
        public static BigNumber operator *(int a, BigNumber b) {
            return (new BigNumber(a.ToString())) * b;
        }
        public static BigNumber operator /(BigNumber a, BigNumber b) {

            int pre = Math.Max(a.DecimalLength, b.DecimalLength);
            pre = pre > precision ? pre : precision;


            return BigCalculate.Division(a, b , pre);
        }

        public static BigNumber operator %(BigNumber a, BigNumber b)
        {
            return BigCalculate.Modular(a, b);
        }

        public static BigNumber operator &(BigNumber a, BigNumber b)
        {
             return BigBinaryCalculate.BitwiseAnd(a, b);
        }

        public static BigNumber operator |(BigNumber a, BigNumber b)
        {
            return BigBinaryCalculate.BitwiseOr(a, b);
        }

        public static BigNumber operator ~(BigNumber a)
        {
            return BigBinaryCalculate.BitwiseNot(a);
        }

        public static BigNumber operator <<(BigNumber a,BigNumber b)
        {
            string result = BigBinaryCalculate.BigBinaryLeftMoveOper(a.ToString(), b.ToString());

            return new BigNumber(result);
        }
        public static BigNumber operator >>(BigNumber a, BigNumber b)
        {
            string result = BigBinaryCalculate.BigBinaryRightMoveOper(a.ToString(), b.ToString());

            return new BigNumber(result);
        }



        public static BigNumber BitwiseXor(BigNumber a, BigNumber b)
        {
            return BigBinaryCalculate.BitwiseXor(a,b);
        }

      
        /// <summary>指定精确的除法</summary>
        public static BigNumber Division(BigNumber dividend, BigNumber divisor, int precision) {

            int pre = Math.Max(dividend.DecimalLength, divisor.DecimalLength);
            pre = pre > precision ? pre : precision;


            return BigCalculate.Division(dividend, divisor, pre);
        }
        public static BigNumber operator ++(BigNumber a) {
            return a + new BigNumber("1");
        }

        public static BigNumber Power(BigNumber a, BigNumber b)
        {
            return a.Power(b);
        }
        public static BigNumber operator ^(BigNumber a, BigNumber b)
        {
            return a.Power(b);
        }

        /*
        public static BigNumber operator !(BigNumber a)
        {
            return a.Factorial();
        }
        */
        public static bool operator >(BigNumber a, BigNumber b)
        {
             return a.CompareTo(b) > 0;
        }

        public static bool operator >=(BigNumber a, BigNumber b)
        {
            return a.CompareTo(b) >= 0;
        }
        public static bool operator <=(BigNumber a, BigNumber b)
        {
            return a.CompareTo(b) <= 0;
        }




        public static bool operator <(BigNumber a, BigNumber b)
        {
            return a.CompareTo(b) < 0;

        }


        public static bool operator ==(BigNumber a, BigNumber b)
        {
            return a.CompareTo(b) == 0;
        }
        public static bool operator !=(BigNumber a, BigNumber b)
        {
            return a.CompareTo(b) != 0;
        }

        public static BigNumber LogicAnd(BigNumber a, BigNumber b)
        {
       
            BigNumber result = Zero;

            if (a >= One && b >= One)
            {
                result = One;
            }
            return result;
        }

        public static BigNumber LogicOr(BigNumber a, BigNumber b)
        {

            BigNumber result = Zero;

            if (a >= One || b >= One)
            {
                result = One;
            }
            return result;
        }


        public static BigNumber LogicNot(BigNumber a)
        {

            BigNumber result = Zero;

            if (a >= One)
            {
                result = Zero;
            }
            else
            {
                result = One;
            }
            return result;
        }

        public static BigNumber LogicXor(BigNumber a,BigNumber b)
        {

            BigNumber result = Zero;

            if (a >= One && b >= One)
            {
                result = Zero;
            }
            else
            {
                result = One;
            }
            return result;
        }



        #endregion

        #region "提供阶乘和幂运算"

        /// <summary>返回正整数的阶乘运算</summary>
        public BigNumber Factorial() {
            BigNumber result = ScienceFunction.Factorial(this);
            return result;
        }

        /// <summary>计算幂，现在可支持任意合法的幂运算</summary>
        public BigNumber Power(BigNumber value) {

            int pre = this.DecimalLength > precision ? this.DecimalLength : precision;
            BigNumber result = DeciamlCalculator.Power(this, value, precision);
            return result.KeepPrecision(pre);
        }

        public BigNumber Power(BigNumber value, int n) {

            int pre = this.DecimalLength > n ? this.DecimalLength : n;

            return DeciamlCalculator.Power(this, value, pre);
        }

        #endregion

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            foreach (int i in IntPart) {
                sb.Append(i.ToString().PadLeft(BigNumber.OneCount, '0'));
            }
            if (intPart.Count == 0) {
                sb.Append(0);
            }
            if (DecimalPart.Count != 0) {
                sb.Append(".");

                foreach (int i in DecimalPart) {
                    sb.Append(i.ToString().PadLeft(BigNumber.OneCount, '0'));
                }
            }
            string r = sb.ToString();
            r = r.TrimStart(new char[] { '0' });

            if (r == string.Empty)
                return "0";
            if (r[0] == '.')
                r = "0" + r;
            if (!IsPlus)
                r = "-" + r;
            return r;
        }

        #region IComparable<BigNumber> 成员

        public int CompareTo(BigNumber other) {
            return CompareNumber.Compare(this, other);
        }


        #endregion

        internal bool IsZero() {
            foreach (int i in IntPart)
                if (i != 0)
                    return false;
            foreach (int i in DecimalPart)
                if (i != 0)
                    return false;
            return true;
        }
 
        /// <summary>计算幂，现在可支持任意合法的幂运算</summary>
        public BigNumber Squrt(BigNumber value)
        {
            int pre = this.DecimalLength > precision ? this.DecimalLength : precision;


            return DeciamlCalculator.Squrt(this, value, pre);
        }

        public BigNumber Squrt(BigNumber value, int n)
        {
            int pre = this.DecimalLength > n ? this.DecimalLength : n;

            return DeciamlCalculator.Squrt(this, value, pre);
        }
    }
}
