/*============================================
 * 类名 :DeciamlPowerCalculator
 * 描述 :
 *   
 * 创建时间: 2011-3-5 15:15:54
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace LittleGrayCalculator.Cores
{
    /// <summary>小数次幂的运算</summary>
    class DeciamlCalculator
    {
        static int BaseNumber =  BigNumber.Max;
        /// <summary>开方</summary>
        static internal BigNumber Sqrt(BigNumber value, int precision) {
            if (!value.IsPlus)
                throw new ExpressionException("只有正数才有开平方运算");

            List<int> div = new List<int>();
            int index = 0;
            int resultIntCount = (value.IntPart.Count + 1) / 2;

            List<int> result = GetFirstDiv(value, ref div, ref index);

            div = FirstTry(result, div);
            div.AddRange(GetNewTwoDiv(value, ref index));
            BigCalculate.RemoveStartZero(div);
            //考虑精度的计算
            while (true) {
                div = TryDiv(div, result);
                if (result.Count - resultIntCount >= precision / BigNumber.OneCount + 2)
                    break;

                div.AddRange(GetNewTwoDiv(value, ref index));
                BigCalculate.RemoveStartZero(div);
            }

            return new BigNumber(result.GetRange(0, resultIntCount), result.GetRange(resultIntCount, result.Count - resultIntCount), true);
        }

        /// <summary>获取第一个除数</summary>
        static List<int> GetFirstDiv(BigNumber value, ref List<int> div, ref int index) {
            //这里有待加入对0.02和0.002的考虑
            if (value.IntPart.Count == 1 && value.IntPart[0] == 0) {
                List<int> result = new List<int>();
                result.Add(0);
                while (true) {
                    if (value.DecimalPart[index] != 0) {
                        for (int i = 0; i < index / 2; i++) {
                            result.Add(0);
                        }
                        //0.005
                        if (index % 2 == 0) {
                            div.Add(value.DecimalPart[index]);
                            if (index + 1 < value.DecimalPart.Count) {
                                div.Add(value.DecimalPart[index + 1]);
                            } else {
                                div.Add(0);
                            }
                        }
                            //0.05
                        else {
                            div.Add(value.DecimalPart[index]);
                        }
                        index++;
                        break;
                    }
                    index++;
                    if (index == value.DecimalPart.Count)
                        break;
                }
                index++;
                return result;
            } else {
                //如果是偶数则第一次取两位
                if (value.IntPart.Count % 2 == 0) {
                    div = value.IntPart.GetRange(0, 2);
                    index = 2;
                }
                    //是奇数则只取一位
                else {
                    div = value.IntPart.GetRange(0, 1);
                    index = 1;
                }
                return new List<int>();
            }
        }

        /// <summary>第一次用开方尝试</summary>
        private static List<int> FirstTry(List<int> result, List<int> div) {
            int tryDiv = BigNumber.Max / 2;
            int low = 1;
            int top = BigNumber.Max - 1;
            //第一位数是1

            //第一用平方试商
            while (true) {
                if (BigCalculate.CompareList(new List<int>() { 1 }, div) == 0) {
                    div = BigCalculate.IntMinus(div, new List<int>() { 1 });
                    result.Add(1);
                    break;
                }
                //连9都小了，那么就是9
                if (BigCalculate.CompareList(BigCalculate.Multiply(BigNumber.Max - 1, BigNumber.Max - 1), div) == -1) {
                    div = BigCalculate.IntMinus(div, BigCalculate.Multiply(BigNumber.Max - 1, BigNumber.Max - 1));
                    result.Add(BigNumber.Max - 1);
                    break;
                }
                int c = BigCalculate.CompareList(BigCalculate.Multiply(tryDiv, tryDiv), div);
                //商大了
                if (c == -1) {
                    low = tryDiv;
                    tryDiv = (low + top) / 2;

                } else if (c == 1) { //商小了
                    top = tryDiv;
                    tryDiv = (low + top) / 2;

                } else { //刚好相等
                    div.Clear();
                    result.Add(tryDiv);
                    break;
                }

                if (low + 1 == top) {
                    div = BigCalculate.IntMinus(div, BigCalculate.Multiply(low, low));
                    result.Add(low);
                    break;
                }
            }
            return div;
        }

        /// <summary>获取新的2数字组</summary>
        static List<int> GetNewTwoDiv(BigNumber value, ref int index) {
            List<int> two = new List<int>();
            //正数部分
            if (index <= value.IntPart.Count - 2) {
                two.AddRange(value.IntPart.GetRange(index, 2));
            }
                //已到小数部分
            else {
                if (index - value.IntPart.Count <= value.DecimalPart.Count - 2) {
                    two.AddRange(value.DecimalPart.GetRange(index - value.IntPart.Count, 2));
                } else if (index - value.IntPart.Count == value.DecimalPart.Count - 1) {
                    two.Add(value.DecimalPart[value.DecimalPart.Count - 1]);
                    two.Add(0);
                } else {
                    two.Add(0);
                    two.Add(0);
                }
            }
            index += 2;
            return two;
        }

        /// <summary>执行一次试商的运算</summary>
        /// <param name="div">已经加组的被除数</param>
        /// <param name="result">已生成的结果</param>
        static List<int> TryDiv(List<int> div, List<int> result) {
            int i = BigNumber.Max / 2;
            int low = 0;
            int top = BigNumber.Max - 1;
            //连1都大了,那么商为0，除数不变
            if (CompartDiv(1, result, div) == 1) {
                result.Add(0);
                return div;
            }
            //连9999都小了
            if (CompartDiv(BigNumber.Max - 1, result, div) == -1) {
                List<int> r = BigCalculate.IntMinus(div, CalDiv(BigNumber.Max - 1, result));
                result.Add(BigNumber.Max - 1);
                return r;
            }

            while (true) {

                int c = CompartDiv(i, result, div);
                //商大了
                if (c == 1) {
                    top = i;
                    i = (low + top) / 2;
                }
                    //商小了
                else if (c == -1) {
                    low = i;
                    i = (low + top) / 2;
                }
                    //刚好相等
                else {
                    //div.Clear();
                    result.Add(i);
                    return new List<int>();
                }
                //已找到合适的商
                if (low + 1 == top) {
                    List<int> r = BigCalculate.IntMinus(div, CalDiv(low, result));
                    result.Add(low);
                    return r;
                }
            }
        }

        /// <summary>对除数进行比较</summary>
        static int CompartDiv(int x, List<int> result, List<int> div) {
            return BigCalculate.CompareList(CalDiv(x, result), div);
        }

        /// <summary>计算x(x+20*result)的值</summary>
        static List<int> CalDiv(int x, List<int> result) {
            List<int> result20 = BigCalculate.Multiply(new List<int>() { BigNumber.Max * 2 }, result);
            List<int> add = BigCalculate.IntAdd(new List<int>() { x }, result20, 0);
            List<int> r = BigCalculate.Multiply(new List<int>() { x }, add);

            return r;
        }

        /// <summary>开2的N次方</summary>
        static internal BigNumber Root(BigNumber value, int n, int precison) {
            BigNumber result = value;

            for (int i = 0; i < n; i++) {
                result = Sqrt(result, precison + 1);
            }
            return result;
        }

        private static BigNumber Sqrt(BigNumber value) {
            throw new NotImplementedException();
        }

        private static double ConvertToDouble(BigNumber x)
        {
            try
            {
                return double.Parse(x.ToString());
            }
            catch
            {
                throw new ArgumentException("数值超出double范围或格式不正确");
            }
        }

        /// <summary>
        /// 计算大数的任意次方根
        /// </summary>
        /// <param name="value">被开方数</param>
        /// <param name="root">开方次数</param>
        /// <param name="precision">精度（小数位数）</param>
        /// <returns>开方结果</returns>
        public static BigNumber NthRoot(BigNumber value, BigNumber root, int precision = 25)
        {

              

            if (root <= BigNumber.Zero)
                throw new ArgumentException("开方次数必须为正数", nameof(root));

            if (!value.IsPlus && root.DecimalPart.Count == 0 && root.IntPart[0] % 2 == 0)
                throw new ArgumentException("负数不能开偶数次方");

            // 特殊情况处理
            if (root == BigNumber.One)
                return value.Clone();

            if (value.IsZero())
                return BigNumber.Zero;

            // 初始猜测值（可以使用更智能的初始值）
            BigNumber guess = value.AbsoluteNumber / root;
            guess.KeepPrecision(precision + 2);

            BigNumber tolerance = new BigNumber($"0.{new string('0', precision)}1");
            BigNumber prevGuess;
            int maxIterations = 1000;
            int iterations = 0;

            do
            {
                prevGuess = guess.Clone();

                // 牛顿迭代公式: x_{n+1} = [(k-1)*x_n + A/(x_n^(k-1))]/k
                BigNumber term1 = (root - BigNumber.One) * prevGuess;
                BigNumber term2 = value / Power(prevGuess, root - BigNumber.One, precision + 2);

                guess = (term1 + term2) / root;

                guess.KeepPrecision(precision + 2);

              
                    iterations++;

                if (iterations > maxIterations)
                {
                    break;
                }
            } while ((guess - prevGuess).AbsoluteNumber > tolerance);

            guess.KeepPrecision(precision);

            // 处理负数开奇数次方
            if (!value.IsPlus && root.IntPart[0] % 2 == 1)
                return guess.ReverseNumber;

            return guess;
        }



        /// <summary>计算value^pow</summary>
        static internal BigNumber Power(BigNumber value, BigNumber pow, int precision) {
            if (pow.DecimalPart.Count != 0 && !value.IsPlus)  //第一个条件是小数次幂，第二个条件是负数
                throw new ExpressionException("只有正数才有小数次幂");


            bool flag = true;
            if (value < BigNumber.Zero)
            {
                flag = false;
                value = value.AbsoluteNumber;
            }




            BigNumber result = new BigNumber("1");
            bool blag = false;
            if (pow.IsPlus == false)
            {
                pow.IsPlus = true;
                blag = true;
            }

            if (pow < BigNumber.One)
            {
                double x = ConvertToDouble(value);
                double y = ConvertToDouble(pow);

                try
                {
                    result = new BigNumber(Math.Pow(x, y).ToString());
                }
                catch
                {
                    BigNumber root = BigNumber.One / pow;
                     root = DeciamlCalculator.Round(root, 0);
                     return NthRoot(value, root, precision);
                }
                 

                
                return result;

            }



            int n = 0;
            bool isFloat = false;
            int oneResult = 0;
            if (value.DecimalLength > 0 || (value.IntLength > 8 && pow.IntLength < 5))
            {
                isFloat = true;
            }
            else
            {
                try
                {

                    oneResult = int.Parse(value.ToString());
                }
                catch
                {
                    isFloat = true;
                }
            }
            if (isFloat)
            {
                for (BigNumber i = new BigNumber("1"); i.CompareTo(pow) != 1; i++)
                {
                    result = result * value;

                }
            }
            else
            {
               
                long AddResult = 0;

                List<long> res = new List<long>();
                List<int> res_int = new List<int>();
                res.Add(1);
                for (BigNumber i = new BigNumber("1"); i.CompareTo(pow) != 1; i++)
                {
                    //  result = result * value;
                    //continue;
                     
                    AddResult = 0;
                    for (int j = 0; j <= n; j++)
                    {

                        long t = res[j] * oneResult + AddResult;
                        AddResult = t / BaseNumber;

                        res[j] =  (t % BaseNumber);
                    }
                    while (AddResult > 0)
                    {
                        n += 1;
                        if (n < res.Count)
                        {
                            res[n] = (AddResult % BaseNumber);
                        }
                        else
                        {
                            res.Add((AddResult % BaseNumber));
                        }
                        AddResult = AddResult / BaseNumber;
                    }


                }
                res.Reverse();
                foreach (long num in res)
                {
                    res_int.Add((int)num);
                }

                result = new BigNumber(res_int, new List<int>(), flag);
            }

            if (blag)
            {
                result = new BigNumber("1") / result;
            }

            return result;

            BigNumber two = new BigNumber("2");
            n = 1;
            BigNumber tt = new BigNumber(new List<int>(), pow.DecimalPart, true);





            if (tt < BigNumber.One)
            {
              


                while (true)
                {
                    // 如果整数部分为1，那么就得进行一次2^n开方运算
                    if (tt.IntPart.Count == 1 && tt.IntPart[0] == 1)
                    {
                        tt.IntPart.Clear();
                        BigNumber r = Root(value, n, precision + 1);

                        if (r.GetPrecision(1) >= precision)
                            break;
                        result = result * r;

                    }
                    else if (tt.IsZero())
                        break;

                    tt = tt * two;
                    n++;


                }

                result.KeepPrecision(precision);
            }

            if (blag)
            {
                result = new BigNumber("1") / result;
            }




            return result;
        }


        /// <summary>
        /// 对大数进行四舍五入（基于4位存储结构）
        /// </summary>
        /// <param name="value">要四舍五入的大数</param>
        /// <param name="decimalDigits">保留的小数位数（总位数，不是数组元素个数）</param>
        /// <returns>四舍五入后的结果</returns>
        public static BigNumber Round(BigNumber value, int decimalDigits)
        {
            if (decimalDigits < 0)
                throw new ArgumentException("小数位数不能为负数", nameof(decimalDigits));

            // 计算需要保留的4位数组元素个数
            int decimalBlocks = (decimalDigits) / BigNumber.OneCount; // 向上取整
            int remainingDigits = decimalDigits % BigNumber.OneCount;

            // 如果不需要舍入
            if (value.DecimalPart.Count <= decimalBlocks)
                return value.Clone();

            // 分离整数和小数部分
            List<int> integerPart = new List<int>(value.IntPart);
            List<int> decimalPart = new List<int>(value.DecimalPart);

            // 检查是否需要进位
            bool needRoundUp = NeedRoundUpOneCount(decimalPart, decimalBlocks, remainingDigits);

            // 截取需要保留的小数部分
            List<int> newDecimalPart = decimalPart.GetRange(0, Math.Min(decimalBlocks, decimalPart.Count));

            // 处理进位
            if (needRoundUp)
            {
                AddOneToDecimalPart(newDecimalPart, integerPart);
            }

            // 如果保留的小数位数不是4的倍数，需要截断多余位数
            if (remainingDigits != 0 && newDecimalPart.Count > 0)
            {
                int lastBlock = newDecimalPart[newDecimalPart.Count - 1];
                int divisor = (int)Math.Pow(10, BigNumber.OneCount - remainingDigits);
                newDecimalPart[newDecimalPart.Count - 1] = lastBlock / divisor * divisor;
            }

            return new BigNumber(integerPart, newDecimalPart, value.IsPlus);
        }

        /// <summary>
        /// 判断是否需要四舍五入（多位存储专用）
        /// </summary>
        private static bool NeedRoundUpOneCount(List<int> decimalPart, int decimalBlocks, int remainingDigits)
        {
            if (decimalBlocks >= decimalPart.Count)
                return false;

            // 获取舍去部分的第一个4位块
            int discardedBlock = decimalPart[decimalBlocks];

            // 计算需要检查的位数
            int checkDigits = remainingDigits == 0 ? BigNumber.OneCount : BigNumber.OneCount - remainingDigits;
            int threshold = (int)Math.Pow(10, checkDigits) / 2; // 5, 50, 500 或 5000

            // 获取需要比较的部分
            int comparisonValue = discardedBlock / (int)Math.Pow(10, BigNumber.OneCount - checkDigits);

            return comparisonValue >= threshold;
        }

        /// <summary>
        /// 给小数部分加1并处理进位
        /// </summary>
        private static void AddOneToDecimalPart(List<int> decimalPart, List<int> integerPart)
        {
            int carry = 1; // 初始加1
            for (int i = decimalPart.Count - 1; i >= 0 && carry > 0; i--)
            {
                int newValue = decimalPart[i] + carry;
                if (newValue >= BigNumber.Max)
                {
                    decimalPart[i] = newValue - BigNumber.Max;
                    carry = 1;
                }
                else
                {
                    decimalPart[i] = newValue;
                    carry = 0;
                }
            }

            // 处理向整数部分的进位
            if (carry > 0)
            {
                for (int i = integerPart.Count - 1; i >= 0 && carry > 0; i--)
                {
                    int newValue = integerPart[i] + carry;
                    if (newValue >= BigNumber.Max)
                    {
                        integerPart[i] = newValue - BigNumber.Max;
                        carry = 1;
                    }
                    else
                    {
                        integerPart[i] = newValue;
                        carry = 0;
                    }
                }

                if (carry > 0)
                {
                    integerPart.Insert(0, carry);
                }
            }
        }

        internal static BigNumber Power(BigNumber bigNumber, BigNumber value) {
            return Power(bigNumber, value, bigNumber.DecimalPart.Count + 1);
        }

        internal static BigNumber Squrt(BigNumber bigNumber, BigNumber value, int precision)
        {
            return Power(bigNumber, new BigNumber("1") / value, precision);
        }
    } // end class
}
