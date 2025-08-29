using System;
using System.Collections.Generic;

namespace LittleGrayCalculator.Cores
{
    public static class EngineerFunction
    {
        public const int DefaultPrecision = 15;

        // 数学常数
        public static readonly BigNumber Pi = new BigNumber(Math.PI.ToString());
        public static readonly BigNumber HalfPi = Pi / new BigNumber("2");
        public static readonly BigNumber E = new BigNumber(Math.E.ToString());
        public static readonly BigNumber DegreeToRadian = new BigNumber((Math.PI / 180).ToString());
        public static readonly BigNumber RadianToDegree = new BigNumber((180.0 / Math.PI).ToString());

        #region 基本三角函数

        /// <summary>
        /// 正弦函数 (Sine)
        /// </summary>
        public static BigNumber Sine(BigNumber x, int precision = DefaultPrecision)
        {
            
            double value = ConvertToDouble(x);

            BigNumber result = new BigNumber(Math.Sin(value).ToString());

            result = DeciamlCalculator.Round(result, result.DecimalLength - 5);
            
            return result;
        }

        /// <summary>
        /// 余弦函数 (Cosine)
        /// </summary>
        public static BigNumber Cosine(BigNumber x, int precision = DefaultPrecision)
        {
            
            double value = ConvertToDouble(x);
            BigNumber result = new BigNumber(Math.Cos(value).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        /// <summary>
        /// 正切函数 (Tangent)
        /// </summary>
        public static BigNumber Tangent(BigNumber x, int precision = DefaultPrecision)
        {
            
            double value = ConvertToDouble(x);
            BigNumber result = new BigNumber(Math.Tan(value).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        /// <summary>
        /// 余切函数 (Cotangent)
        /// </summary>
        public static BigNumber Cotangent(BigNumber x, int precision = DefaultPrecision)
        {
            
            double tanValue = Math.Tan(ConvertToDouble(x));
            if (Math.Abs(tanValue) < double.Epsilon)
                throw new DivideByZeroException("余切函数在整数倍π处无定义");
            BigNumber result = new BigNumber((1.0 / tanValue).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        #endregion

        #region 反三角函数

        /// <summary>
        /// 反正弦函数 (Arcsine)
        /// </summary>
        public static BigNumber Arcsine(BigNumber x, int precision = DefaultPrecision)
        {
            double value = ConvertToDouble(x);
            if (value < -1 || value > 1)
                throw new ExpressionException("反正弦函数的参数必须在[-1,1]区间内");
            BigNumber result = new BigNumber(Math.Asin(value).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        /// <summary>
        /// 反余弦函数 (Arccosine)
        /// </summary>
        public static BigNumber Arccosine(BigNumber x, int precision = DefaultPrecision)
        {
            double value = ConvertToDouble(x);
            if (value < -1 || value > 1)
                throw new ExpressionException("反余弦函数的参数必须在[-1,1]区间内");
            BigNumber result = new BigNumber(Math.Acos(value).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        /// <summary>
        /// 反正切函数 (Arctangent)
        /// </summary>
        public static BigNumber Arctangent(BigNumber x, int precision = DefaultPrecision)
        {
            double value = ConvertToDouble(x);
            BigNumber result = new BigNumber(Math.Atan(value).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        /// <summary>
        /// 反余切函数 (Arccotangent)
        /// </summary>
        public static BigNumber Arccotangent(BigNumber x, int precision = DefaultPrecision)
        {
            double value = ConvertToDouble(x);
            if (Math.Abs(value) < double.Epsilon)
                return HalfPi; // cot⁻¹(0) = π/2
            BigNumber result = new BigNumber(Math.Atan(1.0 / value).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        #endregion

        #region 双曲函数

        /// <summary>
        /// 双曲正弦函数 (SineHyper)
        /// </summary>
        public static BigNumber SineHyper(BigNumber x, int precision = DefaultPrecision)
        {
            double value = ConvertToDouble(x);
            BigNumber result = new BigNumber(Math.Sinh(value).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        /// <summary>
        /// 双曲余弦函数 (CosineHyper)
        /// </summary>
        public static BigNumber CosineHyper(BigNumber x, int precision = DefaultPrecision)
        {
            double value = ConvertToDouble(x);
            BigNumber result = new BigNumber(Math.Cosh(value).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        /// <summary>
        /// 双曲正切函数 (TangentHyper)
        /// </summary>
        public static BigNumber TangentHyper(BigNumber x, int precision = DefaultPrecision)
        {
            double value = ConvertToDouble(x);
            BigNumber result = new BigNumber(Math.Tanh(value).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        #endregion

        #region 反双曲函数

        /// <summary>
        /// 反双曲正弦函数 (ArcsineHyper)
        /// </summary>
        public static BigNumber ArcsineHyper(BigNumber x, int precision = DefaultPrecision)
        {
            double value = ConvertToDouble(x);
            BigNumber result = new BigNumber(Math.Log(value + Math.Sqrt(value * value + 1)).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        /// <summary>
        /// 反双曲余弦函数 (ArccosineHyper)
        /// </summary>
        public static BigNumber ArccosineHyper(BigNumber x, int precision = DefaultPrecision)
        {
            double value = ConvertToDouble(x);
            if (value < 1)
                throw new ExpressionException("反双曲余弦函数的参数必须≥1");
            BigNumber result = new BigNumber(Math.Log(value + Math.Sqrt(value * value - 1)).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        /// <summary>
        /// 反双曲正切函数 (ArctangentHyper)
        /// </summary>
        public static BigNumber ArctangentHyper(BigNumber x, int precision = DefaultPrecision)
        {
            double value = ConvertToDouble(x);
            if (value <= -1 || value >= 1)
                throw new ExpressionException("反双曲正切函数的参数必须在(-1,1)区间内");
            BigNumber result = new BigNumber((0.5 * Math.Log((1 + value) / (1 - value))).ToString());

            result = DeciamlCalculator.Round(result, precision);
            return result;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 角度转弧度
        /// </summary>
        public static BigNumber ToRadians(BigNumber degrees, int precision = DefaultPrecision)
        {
            return (degrees * DegreeToRadian);
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        public static BigNumber ToDegrees(BigNumber radians, int precision = DefaultPrecision)
        {
            return (radians * RadianToDegree);
        }

        /// <summary>
        /// 将大数转换为double (有限精度)
        /// </summary>
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

        #endregion
    }
}