using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleGrayCalculator.Cores
{
    internal class DegreeConverter
    {
        private const string hexnum = "0123456789ABCDEF";
        private const int carry = 10; // Assuming carry is 10 for decimal system

        // 数学常数
        public static readonly BigNumber Pi = new BigNumber(Math.PI.ToString());
        public static readonly BigNumber HalfPi = Pi / new BigNumber("2");
        public static readonly BigNumber E = new BigNumber(Math.E.ToString());
        public static readonly BigNumber DegreeToRadian = Pi / new BigNumber("180");
        public static readonly BigNumber RadianToDegree = new BigNumber("180") / Pi;

        // 度分秒转换小数点
        public static double DEG(string exp)
        {
            string _expression = exp;
            double[] _opnd = new double[4] { 0, 0, 0, 0 };
            double _weight = 0;
            int _opnd_i = 0;
            int _i = 0;
            double _deg = 0;
            double _operand = 0.0;

            while (_i < _expression.Length)
            {
                string currentChar = _expression.Substring(_i, 1);
                _weight = carry;
                _operand = 0.0;

                if (IsNumber(currentChar))
                {
                    while (_i < _expression.Length && IsNumber(_expression.Substring(_i, 1)))
                    {
                        _operand = _operand * _weight + StrStr(hexnum, _expression.Substring(_i, 1), 0);
                        _i += 1;
                    }

                    if (_i < _expression.Length && _expression.Substring(_i, 1) == ".")
                    {
                        _i += 1;
                        while (_i < _expression.Length && IsNumber(_expression.Substring(_i, 1)))
                        {
                            _operand = _operand + (StrStr(hexnum, _expression.Substring(_i, 1), 0) + 0.0) / _weight;
                            _weight *= carry;
                            _i += 1;
                        }
                    }

                    if (_opnd_i < _opnd.Length)
                    {
                        _opnd[_opnd_i] = _operand;
                        _opnd_i += 1;
                    }
                }
                else
                {
                    _i += 1;
                }
            }

            _deg = _opnd[0] + _opnd[1] / 60 + _opnd[2] / 3600 + _opnd[3];
            return _deg;
        }

        // 小数点转换度分秒
        public static string DMS(string exp)
        {
            if (Compare(exp, Math.Pow(2, 64).ToString()) > 0)
            {
                return "INF";
            }

            double _exp = DEG(exp);
            string _dms = "";
            int _d = (int)_exp;
            double _t = (_exp - _d) * 60;
            int _m = (int)_t;
            double _s = (_t - _m) * 60;
            double _S = _s;

            _dms = $"{_d}°{_m}'{_S}";
            return _dms;
        }

        // Helper methods
        private static bool IsNumber(string str)
        {
            return double.TryParse(str, out _);
        }

        private static int StrStr(string haystack, string needle, int startIndex)
        {
            return haystack.IndexOf(needle, startIndex);
        }

        public static int Compare(string x, string y)
        {
            string _x = x;
            string _y = y;

            // 修正前面多加的0
            if (_x.Length > 1 && _x[0] == '0' && (_x.Length <= 1 || _x[1] != '.'))
            {
                _x = _x.Substring(1);
            }

            if (_y.Length > 1 && _y[0] == '0' && (_y.Length <= 1 || _y[1] != '.'))
            {
                _y = _y.Substring(1);
            }

            // 处理包含小数点的情况
            if (_x.Contains(".") || _y.Contains("."))
            {
                if (double.TryParse(_x, out double a) && double.TryParse(_y, out double b))
                {
                    if (Math.Abs(a - b) < double.Epsilon)
                    {
                        return 0;
                    }
                    return a > b ? 1 : -1;
                }
            }

            // 比较整数部分
            int len1 = _x.Length;
            int len2 = _y.Length;

            if (len1 > len2)
            {
                return 1;
            }
            else if (len1 < len2)
            {
                return -1;
            }
            else // len1 == len2
            {
                if (_x == _y)
                {
                    return 0;
                }

                for (int i = 0; i < len1; i++)
                {
                    if (_x[i] > _y[i])
                    {
                        return 1;
                    }
                    else if (_x[i] < _y[i])
                    {
                        return -1;
                    }
                }

                return 0;
            }
        }

        public static BigNumber ToDegrees(BigNumber radians, int precision = 25)
        {
            return (radians * RadianToDegree);
        }

        public static BigNumber ToRadians(BigNumber degrees, int precision = 25)
        {
            return (degrees * DegreeToRadian);
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
    }
}
