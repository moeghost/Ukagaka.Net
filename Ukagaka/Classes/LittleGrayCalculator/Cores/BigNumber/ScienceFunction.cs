using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LittleGrayCalculator.Cores
{
    /// <summary>
    /// 提供科学计算函数的类，包括对数等运算
    /// </summary>
    public static class ScienceFunction
    {
        // 预计算常数
        private static readonly BigNumber Ln10 = new BigNumber("2.3025850929940456840179914547");
        private static readonly BigNumber LnR = new BigNumber("0.2002433314278771112016301167");
        private static readonly BigNumber One = new BigNumber("1");
        private static readonly BigNumber PointOne = new BigNumber("0.1");
        private static readonly BigNumber Point9047 = new BigNumber("0.9047");
        private static readonly BigNumber OnePoint2217 = new BigNumber("1.2217");
        private static readonly BigNumber Ten = new BigNumber("10");
        static int BaseNumber = BigNumber.Max;
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
        /// 计算以10为底的对数
        /// </summary>
        public static BigNumber Log10(this BigNumber x, int precision = 25)
        {

            double value = ConvertToDouble(x);

            BigNumber result = new BigNumber(Math.Log10(value).ToString());

            //result = DeciamlCalculator.Round(result, result.DecimalLength - 5);

            return result;



            BigNumber lnX = Ln(x, precision);
            BigNumber ln10 = Ln(Ten, precision);
            //lnX.KeepPrecision(precision);
            //ln10.KeepPrecision(precision);
            result = lnX / ln10;

            result.KeepPrecision(precision);
           // result = DeciamlCalculator.Round(result,precision / 2 );
            return result;
        }

        /// <summary>
        /// 计算自然对数
        /// </summary>
        public static BigNumber Ln(this BigNumber x, int precision = 25)
        {
             

            if (!x.IsPlus || x.IsZero())
            {
                throw new ArgumentException("对数函数的参数必须是正数");
            }

            // 处理x=1的特殊情况
            if (x == One)
            {
                return BigNumber.Zero;
            }

            double value = ConvertToDouble(x);

            BigNumber result = new BigNumber(Math.Log(value).ToString());

            //result = DeciamlCalculator.Round(result, result.DecimalLength - 5);

            return result;






            int k = 0;
            int l = 0;
            BigNumber xTemp = x.Clone();

            // 调整x到(0.1, 1]区间
            while (xTemp > One)
            {
                xTemp = xTemp / Ten;
                xTemp.KeepPrecision(precision * 10);
                k++;
            }
            while (xTemp <= PointOne)
            {
                xTemp = xTemp * Ten;
                xTemp.KeepPrecision(precision * 10);
                k--;
            }

            // 调整x到[0.9047, 1.10527199)区间
            while (xTemp < Point9047)
            {
                xTemp = xTemp * OnePoint2217;
                xTemp.KeepPrecision(precision * 10);
                l--;
            }

            // 计算对数
            BigNumber y = (xTemp - One) / (xTemp + One);
            y.KeepPrecision(precision * 10);
            result = new BigNumber(k.ToString()) * Ln10 + l * LnR + Logarithm(y, precision);
            result.KeepPrecision(precision * 10);
            return result ;
        }

        /// <summary>
        /// 计算ln((1+y)/(1-y))的辅助函数
        /// </summary>
        private static BigNumber Logarithm(BigNumber y, int precision)
        {
            y.KeepPrecision(precision * 10);
            BigNumber v = One;
            BigNumber y2 = y * y;
            y2.KeepPrecision(precision * 10);
            BigNumber t = y2;
            BigNumber z = t / new BigNumber("3");

            for (int i = 3; !z.IsZero(); i += 2)
            {
                v = v + z;
                t = t * y2;
                 
                z = t / new BigNumber(i.ToString());

                // 检查z是否足够小以停止迭代
                if (z.DecimalLength > precision * 10)
                {
                    break;
                }
            }

            return v * y * new BigNumber("2");
        }
        /// <summary>
        /// 计算任意底数的对数
        /// </summary>
        /// <param name="x">输入值</param>
        /// <param name="baseNumber">对数底数</param>
        /// <param name="precision">精度</param>
        /// <returns>以baseNumber为底x的对数</returns>
        public static BigNumber Log(BigNumber x, BigNumber baseNumber, int precision = 25)
        {
            if (!baseNumber.IsPlus || baseNumber == new BigNumber("1"))
                throw new ArgumentException("对数底数必须为正数且不等于1");

            // log_b(x) = ln(x)/ln(b)
            BigNumber lnX = Ln(x, precision + 2);
            BigNumber lnB = Ln(baseNumber, precision + 2);

            lnX.KeepPrecision(precision);
            lnB.KeepPrecision(precision);
            BigNumber result = lnX / lnB;
            result = DeciamlCalculator.Round(result, 15);
            return result;
        }

        public static BigNumber Factorial(BigNumber x)
        {
            if (!x.IsPlus || x.DecimalPart.Count != 0)
                throw new NumberException("只有正整数才有阶乘运算", 1);
            
            
            
            
            
            long AddResult = 0;



            List<long> res = new List<long>();
            List<int> res_int = new List<int>();
            res.Add(1);
            int n = 0;
            int oneResult = 0;


            for (BigNumber i = new BigNumber("2"); i.CompareTo(x) != 1; i++)
            {
                //  result = result * value;
                //continue;
                oneResult = i.ToInt();
                AddResult = 0;
                for (int j = 0; j <= n; j++)
                {

                    long t = res[j] * oneResult + AddResult;
                    AddResult = t / BaseNumber;

                    res[j] = (t % BaseNumber);
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

            BigNumber result = new BigNumber(res_int, new List<int>(), true);
            return result;
        }





    }
}