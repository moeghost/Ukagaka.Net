/*============================================
 * 类名 :TaylorFunction
 * 描述 :计算泰勒公式的静态类
 *   
 * 创建时间: 2011-2-5 22:10:52
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using aya.Eval;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>计算泰勒公式的静态类</summary>
    class TaylorFunction
    {
        static int precision = 18;
        static BigNumber symbol = new BigNumber("-1");
        static BigNumber one = new BigNumber("1");
        static BigNumber two = new BigNumber("2");
        static BigNumber Pi = new PiNode().Value;

        static int limitTime = BigNumber.precision * BigNumber.OneCount * 10;
        /// <summary>正弦的泰勒公式</summary>
        /// sin(x)=x-1/(3!)*x^3+1/(5!)*x^5+···+(-1)^n*1/(2*n+1)!*x^(2*n+1)+o(x^(2*n+1))


        public static BigNumber Sine(BigNumber value, int precision)
        {
            if (precision > TaylorFunction.precision)
            {
                //precision = TaylorFunction.precision;
            }
            BigNumber radians = value * Pi / new BigNumber("180");
            radians.KeepPrecision(precision);
            BigNumber sum = new BigNumber("0");
            BigNumber term = radians.Clone();
            BigNumber xSquared = radians * radians;
            BigNumber power = radians.Clone();
            BigNumber factorial = one.Clone();

            int n = 0;
            int maxIterations = precision; // 增加迭代次数
            BigNumber tolerance = new BigNumber($"0.{new string('0', precision)}1");

            while (n < maxIterations)
            {
                // 添加当前项
                sum = sum + term;

                // 计算下一项
                power = power * xSquared;
                factorial = factorial * new BigNumber(((2 * n + 2) * (2 * n + 3)).ToString());
                term = (n % 2 == 0 ? symbol : one) * power / factorial;

                // 检查收敛
                if (term.AbsoluteNumber < tolerance)
                {
                    break;
                }
                n++;
            }
            sum = DeciamlCalculator.Round(sum, precision + 3);
            Remove(sum, precision);


            return sum;
        }
        public static BigNumber Sine1(BigNumber value, int precision) 
        {
            BigNumber sum = new BigNumber("0");
            value = value * Pi / new BigNumber("180");
             
            int i = 0;

            for (BigNumber n = new BigNumber("0"); ; n++) 
            {
                //  BigNumber r = BigNumber.Division(symbol.Power(n) * value.Power(1 + 2 * n), (1 + 2 * n).Factorial(), precision + 1);

                BigNumber flag = (i % 2 == 0 ?   one: symbol);

                BigNumber r = (flag) * (value ^ (two * n + one)) /(two * n + one).Factorial();
                if (r.GetPrecision(0) >= precision)
                {
                    break;
                }
                sum = sum + r;
                i++;
            }
            Remove(sum, precision);
            return sum;
        }


        public static BigNumber Cosine(BigNumber value, int precision)
        {
            // 角度转弧度
            BigNumber radians = value * Pi / new BigNumber("180");
            radians.KeepPrecision(precision + 2);

            BigNumber sum = new BigNumber("1");  // 余弦级数第一项是1
            BigNumber term = new BigNumber("1"); // 当前项初始为1
            BigNumber xSquared = radians * radians;
            BigNumber power = new BigNumber("1"); // x^0 = 1
            BigNumber factorial = new BigNumber("1"); // 0! = 1

            int n = 1;  // 从第1项开始（第0项已设为1）
            int maxIterations = precision * 2;
            BigNumber tolerance = new BigNumber($"0.{new string('0', precision)}1");

            while (n < maxIterations)
            {
                // 计算下一项: (-1)^n * x^(2n) / (2n)!
                power = power * xSquared;
                factorial = factorial * new BigNumber(((2 * n - 1) * (2 * n)).ToString());
                term = (n % 2 == 0 ? one : symbol) * power / factorial;

                sum = sum + term;

                // 检查收敛
                if (term.AbsoluteNumber < tolerance)
                {
                    break;
                }
                n++;
            }
            sum = DeciamlCalculator.Round(sum, precision + 3);
            Remove(sum, precision);
            return sum;
        }



        /// <summary>余弦的泰勒公式</summary>
        /// cos(x)=1-1/(2!)*x^2+1/(4!)*x^4+···+(-1)^n*1/(2*n)!*x^(2*n)+o(x^(2*n))
        public static BigNumber Cosine1(BigNumber value, int precision) 
        {
            BigNumber sum = new BigNumber("0");

            for (BigNumber n = new BigNumber("0"); ; n++) 
            {
                //sin x=x-1/3!x³+1/5!x⁵+o(x⁵)
                //  BigNumber r = BigNumber.Division(symbol.Power(n) * value.Power(2 * n), (2 * n).Factorial(), precision + 1);
                BigNumber r = ((symbol) ^ n) * one * (value ^ (two * n)) / (two * n).Factorial();
                if (r.GetPrecision(0) >= precision)
                {
                    break;
                }
                sum = sum + r;
            }
            Remove(sum, precision);
            return sum;
        }


        /// <summary>正切的泰勒公式</summary>
        /// tan(x)=x+x^3/3+2*x^5/15+17*x^7/315+62*x^9/2835+···+(2^(2*n)*(2^(2*n)-1)*B(2*n-1)*x^(2*n-1))/(2*n)!+···
        public static BigNumber Tangent(BigNumber value, int precision)
        {
            BigNumber result = Sine(value, precision + 4) / Cosine(value, precision + 4);
             result.KeepPrecision(precision);
            result = DeciamlCalculator.Round(result, precision + 3);
            return result;

            BigNumber sum = new BigNumber("0");
 
            for (BigNumber n = new BigNumber("0"); ; n++)
            {
                //
                BigNumber r = (two ^ (two * n) * (two ^ (two * n) - one) * Bernoulli(two * n - one) * value ^ (two * n - one)) / (two * n).Factorial();


                if (r.GetPrecision(0) >= precision)
                {
                    break;
                }
                sum = sum + r;
            }
            Remove(sum, precision);
            return sum;




        }
        /// <summary>余切的</summary>

        public static BigNumber Cotangent(BigNumber value, int precision)
        {
            BigNumber result = Cosine(value, precision + 4) / Sine(value, precision + 4);
            result.KeepPrecision(precision);
            result = DeciamlCalculator.Round(result, precision + 3);
            return result;



            BigNumber sum = new BigNumber("0");

            for (BigNumber n = new BigNumber("0"); ; n++)
            {
                //
                BigNumber r = ((symbol) ^ n) * one * (value ^ (two * n)) / (two * n).Factorial();
                r = r / (((symbol) ^ n) * (value ^ (two * n + one)) / (two * n + one).Factorial());

                if (r.GetPrecision(0) >= precision)
                {
                    break;
                }
                sum = sum + r;
            }
            Remove(sum, precision);
            return sum;
             
        }

        /// <summary>反正弦的</summary>

        /// arcsinx = x+ 1/2*x^3/3+   1/2*3/4*x^5/5+1/2*3/4*5/6* x^7/7+···+(2*n)!/(2^(2*n)*(n!)^2)*x^(2*n+1)/(2*n+1)

        public static BigNumber Arcsine(BigNumber value, int precision)
        {
             
            if (value == new BigNumber("1"))
            {
                return new BigNumber((Math.PI / 2).ToString());
            }

            if (value > new BigNumber("1"))
            {
                throw new ExpressionException("反正弦的参数的绝对值必须小于等于1");
            }
            if (value < new BigNumber("-1"))
            {
                throw new ExpressionException("反正弦的参数的绝对值必须小于等于1");
            }


            BigNumber sum = new BigNumber("0");

            for (BigNumber n = new BigNumber("0"); ; n++)
            {
                //
                BigNumber r = (((two * n).Factorial()) / ((two ^ (two * n)) * ((n.Factorial()) ^ two))) * ((value ^ (two * n + one)) / (two * n + one));


                if (r.GetPrecision(0) >= precision)
                {
                    break;
                }
                sum = sum + r;
            }
            Remove(sum, precision);
            return sum;

        }


        /// <summary>反余弦的</summary>

        /// arcosinx = pi / 2 - arcsinx
         public static BigNumber Arccosine(BigNumber value, int precision)
        {
            return new BigNumber((Math.PI / 2).ToString()) - Arcsine(value, precision);
             
        }

        /// <summary>反正切的</summary>
        ///arctanx=x-1/3x³+1/5x⁵-1/7x⁷+···+((-1)^n)*(x^(2*n+1))/(2*n+1)
        public static BigNumber Arctangent(BigNumber value, int precision)
        {
            if (value == new BigNumber("1"))
            {
                return new BigNumber((Math.PI / 4).ToString());
            }


            if (value > new BigNumber("1"))
            {
                return new BigNumber(Math.Atan(double.Parse(value.ToString())).ToString());
                // throw new ExpressionException("反正切的参数的绝对值必须小于等于1");
            }

            if (value < new BigNumber("-1"))
            {
                return new BigNumber(Math.Atan(double.Parse(value.ToString())).ToString());
                //   throw new ExpressionException("反正切的参数的绝对值必须小于等于1");
            }

            BigNumber sum = new BigNumber("0");

            for (BigNumber n = new BigNumber("0"); ; n++)
            {
                //
                BigNumber r = ((symbol) ^ n) * (value ^ (two * n + one)) / (two * n + one);


                if (r.GetPrecision(0) >= precision)
                {
                    break;
                }
                sum = sum + r;
            }
            Remove(sum, precision);
            return sum;

        }


        /// <summary>反余切的</summary>
        /// arccotangent(x) = pi / 2 - arctangent(x)
        public static BigNumber Arccotangent(BigNumber value, int precision)
        {
            return new BigNumber((Math.PI / 2).ToString()) - Arctangent(value, precision);

        }





        /// <summary>
        /// 双曲正弦的展开
        /// sinh(x)=x+(x^3)/(3!)+(x^5)/(5!)+(x^7)/(7!)+···+(x^(2*n+1))/(2*n+1)!+···
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BigNumber SineHyper(BigNumber value, int precision)
        {
           // value = value * Pi / new BigNumber("180");


            BigNumber sum = new BigNumber("0");

            for (BigNumber n = new BigNumber("0"); ; n++)
            {
                //
                BigNumber r = (value ^ (two * n + one)) / (two * n + one).Factorial();


                if (r.GetPrecision(0) >= precision)
                {
                    break;
                }
                sum = sum + r;
            }
            Remove(sum, precision);
            return sum;
 
        }
        /// <summary>
        /// 双曲余弦的展开
        ///双曲余弦：cosh(x) = 1+(x^2)/(2!)+(x^4)/(4!)+(x^6)/(6!)+···+(x^(2*n))/(2*n)!+···
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BigNumber CosineHyper(BigNumber value, int precision)
        {

            BigNumber sum = new BigNumber("0");

            for (BigNumber n = new BigNumber("0"); ; n++)
            {
                //
                BigNumber r = (value ^ (two * n)) / (two * n).Factorial();


                if (r.GetPrecision(0) >= precision)
                {
                    break;
                }
                sum = sum + r;
            }
            Remove(sum, precision);
            return sum;

        }

        /// <summary>
        /// 双曲正切
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BigNumber TangentHyper(BigNumber value, int precision)
        {
             if (value == new BigNumber("0"))
            {
                return value;
            }

          

            return SineHyper(value,precision) / CosineHyper(value, precision);
        }


        /// <summary>
        /// 反双曲正弦
        /// arcsinh(x)=x-(1/2)*(x^3)/3+(1*3/(2*4))*x^5/5-(1*3*5/(2*4*6))*x^7/7+···+(((-1)^n)*(2*n)!)/((2^(2*n))*(n!)^2)*(x^(2*n+1))/(2*n+1)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BigNumber ArcsineHyper(BigNumber value, int precision)
        {

            if (value > new BigNumber("1"))
            {
                throw new ExpressionException("双曲正弦的参数绝对值必须小于1");
                // return new BigNumber("0");
            }

            if (value < new BigNumber("-1"))
            {
                throw new ExpressionException("双曲正弦的参数绝对值必须小于1");
                // return new BigNumber("0");
            }

             

            BigNumber sum = new BigNumber("0");
            BigNumber check = new BigNumber("0");
            long time = Utils.SystemTimer.GetTimeTickCount();
            for (BigNumber n = new BigNumber("0");; n++)
            {
                //
                BigNumber r = ((((symbol) ^ n) * ((two * n).Factorial())) / ((two ^ (two * n)) * (n.Factorial() ^ two))) * (value ^ (two * n + one)) / (two * n + one);


                if (r.GetPrecision(0) >= precision )
                {
                    break;
                }
                    
                if (Utils.SystemTimer.GetTimeTickCount() - time > limitTime)
                {
                     return new BigNumber(Math.Asinh(double.Parse(value.ToString())).ToString());
                }
                sum = sum + r;

                

            }
            Remove(sum, precision);
            return sum;

        }



        /// <summary>
        /// 反双曲正切
        ///  arctanh(x)=x+x^3/3+x^5/5+x^7/7+···+(x^(2*n+1))/(2*n+1)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BigNumber ArctangentHyper(BigNumber value, int precision)
        {

            if (value > new BigNumber("1"))
            {
                throw new ExpressionException("反双曲正弦的参数绝对值必须小于1");
                //return new BigNumber("0");
            }

            if (value < new BigNumber("-1"))
            {
                throw new ExpressionException("反双曲正弦的参数绝对值必须小于1");
                // return new BigNumber("0");
            }


            if (value == new BigNumber("1"))
            {
                return new BigNumber("0");
            }


            BigNumber sum = new BigNumber("0");

            for (BigNumber n = new BigNumber("0"); ; n++)
            {
                //
                BigNumber r = (value ^ (two * n + one)) / (two * n + one);

                if (r.GetPrecision(0) >= precision)
                {
                    break;
                }
                sum = sum + r;
            }
            Remove(sum, precision);
            return sum;

        }

        /// <summary>
        /// 反双曲余弦
        /// arcsinh(x)=x-(1/2)*(x^3)/3+(1*3/(2*4))*x^5/5-(1*3*5/(2*4*6))*x^7/7+···+(((-1)^n)*(2*n)!)/((2^(2*2n))*(n!)^2)*(x^(2*n+1))/(2*n+1)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BigNumber ArccosineHyper(BigNumber value, int precision)
        {

            if (value < new BigNumber("1"))
            {
                // return new BigNumber("0");
                throw new ExpressionException("反双曲余弦的参数必须大于等于1");
            }

             
            // return ArctangentHyper(value, precision) / ArcsineHyper(value,precision);
           
            return new BigNumber(Math.Acosh(double.Parse(value.ToString())).ToString());
        }




        /// <summary>伯努利数的展开公式</summary>
        public static BigNumber Bernoulli(BigNumber value)
        {
            
            BigNumber sum = new BigNumber("0");
            if (value == sum)
            {
                return new BigNumber("1");
            }


            for (BigNumber n = new BigNumber("0"); n < value; n++)
            {
                BigNumber r = Bernoulli(n) / (value - n + new BigNumber("1"));

                sum = sum + r;
            }




            return sum;
        }




        /// <summary>移除精度后多余的数字</summary>
        private static void Remove(BigNumber sum, int precision) 
        {
            if (precision < sum.DecimalPart.Count)
            {
                sum.DecimalPart.RemoveRange(precision, sum.DecimalPart.Count - precision);
            }
        }

          
        /// <summary>e次幂</summary>
        public static BigNumber Exp(BigNumber value, int precision) 
        {
            BigNumber sum = new BigNumber("0");

            if (!value.IsPlus)
            {
                return new BigNumber("1") / Exp(value.AbsoluteNumber, precision);
            }


            for (BigNumber n = new BigNumber("0"); ; n++) 
            {
                BigNumber r = BigNumber.Division(value.Power(n), n.Factorial(), precision + 1);

                if (r.GetPrecision(0) > precision)
                {
                    break;
                }
                sum = sum + r;
            }
            Remove(sum, precision);
            return sum;
        }

    } // end class
}
