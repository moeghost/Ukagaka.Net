using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace LittleGrayCalculator.Cores
{
    internal class BigBinaryCalculate
    {
        #region 位运算实现

        /// <summary>大数按位与</summary>
        public static BigNumber BitwiseAnd(BigNumber one, BigNumber two)
        {
            // 确保两个数都是整数
            if (one.DecimalPart.Count > 0 || two.DecimalPart.Count > 0)
            {
                throw new ArgumentException("位运算只能用于整数");
            }
            // 转换为二进制表示
            var bitsOne = ConvertToBits(one.IntPart);
            var bitsTwo = ConvertToBits(two.IntPart);

            // 对齐位数
            AlignBits(ref bitsOne, ref bitsTwo);

            // 执行按位与操作
            var resultBits = new List<bool>();
            for (int i = 0; i < bitsOne.Count; i++)
            {
                resultBits.Add(bitsOne[i] & bitsTwo[i]);
            }

            // 转换回大数
            return ConvertFromBits(resultBits);
        }

        /// <summary>大数按位或</summary>
        public static BigNumber BitwiseOr(BigNumber one, BigNumber two)
        {
            // 确保两个数都是整数
            if (one.DecimalPart.Count > 0 || two.DecimalPart.Count > 0)
                throw new ArgumentException("位运算只能用于整数");

            // 转换为二进制表示
            var bitsOne = ConvertToBits(one.IntPart);
            var bitsTwo = ConvertToBits(two.IntPart);

            // 对齐位数
            AlignBits(ref bitsOne, ref bitsTwo);

            // 执行按位或操作
            var resultBits = new List<bool>();
            for (int i = 0; i < bitsOne.Count; i++)
            {
                resultBits.Add(bitsOne[i] | bitsTwo[i]);
            }

            // 转换回大数
            return ConvertFromBits(resultBits);
        }

        /// <summary>大数按位取反</summary>
        public static BigNumber BitwiseNot(BigNumber num)
        {
            // 确保是整数
            if (num.DecimalPart.Count > 0)
                throw new ArgumentException("位运算只能用于整数");

            // 转换为二进制表示
            var bits = ConvertToBits(num.IntPart);

            // 执行按位取反操作
            var resultBits = new List<bool>();
            for (int i = 0; i < bits.Count; i++)
            {
                resultBits.Add(!bits[i]);
            }

            // 转换回大数
            return ConvertFromBits(resultBits);
        }

        /// <summary>大数按位异或</summary>
        public static BigNumber BitwiseXor(BigNumber one, BigNumber two)
        {
            // 确保两个数都是整数
            if (one.DecimalPart.Count > 0 || two.DecimalPart.Count > 0)
                throw new ArgumentException("位运算只能用于整数");

            // 转换为二进制表示
            var bitsOne = ConvertToBits(one.IntPart);
            var bitsTwo = ConvertToBits(two.IntPart);

            // 对齐位数
            AlignBits(ref bitsOne, ref bitsTwo);

            // 执行按位异或操作
            var resultBits = new List<bool>();
            for (int i = 0; i < bitsOne.Count; i++)
            {
                resultBits.Add(bitsOne[i] ^ bitsTwo[i]);
            }

            // 转换回大数
            return ConvertFromBits(resultBits);
        }

        public static string BigBinaryLeftMoveOper(string a, string b)
        {
            string y = b;
            string result = "";

            string src = BigProgramer.BigTrans(a, 10, 2);

            if (string.IsNullOrEmpty(src))
            {
                return "0";
            }

            if (!y.Contains("."))
            {
                y = y + ".0";
            }

            string n = y.Split('.')[0];
            int n_num = 0;

            if (int.TryParse(n, out int parsedNum))
            {
                n_num = parsedNum;
            }

            if (n_num < 0)
            {
                return "";
            }

            result = src;
            for (int i = 0; i < n_num; i++)
            {
                result += "0";
            }

            result = BigProgramer.BigTrans(result, 2, 10);
            Console.WriteLine($"leftmove is {a} {b}");

            return result;
        }

        public static string BigBinaryRightMoveOper(string a, string b)
        {
            string result = "";
            string src = BigProgramer.BigTrans(a, 10, 2);

            if (string.IsNullOrEmpty(src))
            {
                return "0";
            }

            string n = b.Split('.')[0];
            Console.WriteLine($"rightmove is {a} {b}");

            int n_right = 0;
            if (int.TryParse(n, out int parsedNum))
            {
                n_right = parsedNum;
            }

            if (n_right < 0)
            {
                return "";
            }

            // Handle the right shift by removing characters
            for (int i = 0; i < n_right; i++)
            {
                if (src.Length > 0)
                {
                    src = src.Remove(src.Length - 1);
                }
            }

            if (src.Length == 0)
            {
                return "0";
            }

            result = BigProgramer.BigTrans(src, 2, 10);
            return result;
        }



        /// <summary>
        /// 大数左移运算（相当于乘以2^shiftBits）
        /// </summary>
        /// <param name="value">要移位的大数</param>
        /// <param name="shiftBits">左移的位数</param>
        /// <returns>移位后的结果</returns>
        public static BigNumber LeftShift(BigNumber value, int shiftBits)
        {
            if (shiftBits < 0)
                throw new ArgumentException("移位位数不能为负数", nameof(shiftBits));

            if (shiftBits == 0 || value.IsZero())
                return value.Clone();

            // 分离整数和小数部分
            List<int> intPart = new List<int>(value.IntPart);
            List<int> decimalPart = new List<int>(value.DecimalPart);

            // 处理小数部分左移
            LeftShiftDecimalPart(ref decimalPart, ref intPart, shiftBits);

            // 处理整数部分左移
            LeftShiftIntegerPart(ref intPart, shiftBits);

            return new BigNumber(intPart, decimalPart, value.IsPlus);
        }

        /// <summary>
        /// 小数部分左移处理
        /// </summary>
        private static void LeftShiftDecimalPart(ref List<int> decimalPart, ref List<int> intPart, int shiftBits)
        {
            if (decimalPart.Count == 0)
                return;

            int bitsRemaining = shiftBits;
            while (bitsRemaining > 0 && decimalPart.Count > 0)
            {
                int currentShift = Math.Min(bitsRemaining, BigNumber.OneCount);
                int multiplier = (int)Math.Pow(2, currentShift);
                int carry = 0;

                // 从最低位开始处理
                for (int i = decimalPart.Count - 1; i >= 0; i--)
                {
                    long temp = (long)decimalPart[i] * multiplier + carry;
                    decimalPart[i] = (int)(temp % BigNumber.Max);
                    carry = (int)(temp / BigNumber.Max);
                }

                // 处理向整数部分的进位
                if (carry > 0)
                {
                    if (intPart.Count == 0)
                        intPart.Add(0);

                    intPart[intPart.Count - 1] += carry;
                    CarryOver(intPart, intPart.Count - 1);
                }

                // 检查是否需要将小数部分最高位移到整数部分
                if (decimalPart.Count > 0 && decimalPart[0] >= BigNumber.Max / 2)
                {
                    if (intPart.Count == 0)
                        intPart.Add(0);

                    intPart[intPart.Count - 1] += decimalPart[0] / (BigNumber.Max / 2);
                    decimalPart[0] %= BigNumber.Max / 2;
                    CarryOver(intPart, intPart.Count - 1);
                }

                bitsRemaining -= currentShift;
            }

            // 移除前导零
            BigCalculate.RemoveStartZero(decimalPart);
        }

        /// <summary>
        /// 整数部分左移处理（修正版）
        /// </summary>
        private static void LeftShiftIntegerPart(ref List<int> intPart, int shiftBits)
        {
            if (shiftBits <= 0)
                return;



            int multiplier = 2;
            int carry = 0;
            int num = multiplier;
            int n = 0;

            List<int> res = new List<int>();
            res.AddRange(intPart);
            for (int i = 0; i < shiftBits; i++)
            {
                carry = 0;
                for (int j = 0; j <= n; j++)
                {
                    int temp = res[j] * multiplier + carry;
                    carry = temp / BigNumber.Max;
                    res[j] = (temp % BigNumber.Max);

                }
                while (carry > 0)
                {
                    n += 1;
                    if (n < res.Count)
                    {

                        res[n] = (carry % BigNumber.Max);
                    }
                    else
                    {
                        res.Add(carry % BigNumber.Max);
                    }
                    carry = carry / BigNumber.Max;
                }

            }
            res.Reverse();
            intPart = res;
        }
        /// <summary>
        /// 处理进位
        /// </summary>
        private static void CarryOver(List<int> parts, int position)
        {
            while (position >= 0 && parts[position] >= BigNumber.Max)
            {
                int carry = parts[position] / BigNumber.Max;
                parts[position] %= BigNumber.Max;

                if (position == 0)
                {
                    parts.Insert(0, carry);
                    position++;
                }
                else
                {
                    parts[position - 1] += carry;
                }

                position--;
            }
        }


        /// <summary>
        /// 大数右移运算（相当于除以2^shiftBits）
        /// </summary>
        /// <param name="value">要移位的大数</param>
        /// <param name="shiftBits">右移的位数</param>
        /// <returns>移位后的结果</returns>
        public static BigNumber RightShift(BigNumber value, int shiftBits)
        {
            if (shiftBits < 0)
                throw new ArgumentException("移位位数不能为负数", nameof(shiftBits));

            if (shiftBits == 0 || value.IsZero())
                return value.Clone();

            // 分离整数和小数部分
            List<int> intPart = new List<int>(value.IntPart);
            List<int> decimalPart = new List<int>(value.DecimalPart);

            // 处理整数部分右移
            RightShiftIntegerPart(ref intPart, ref decimalPart, shiftBits);

            // 处理小数部分右移
            RightShiftDecimalPart(ref decimalPart, shiftBits);

            // 移除前导零和尾随零
            BigCalculate.RemoveStartZero(intPart);
            while (decimalPart.Count > 0 && decimalPart[decimalPart.Count - 1] == 0)
            {
                decimalPart.RemoveAt(decimalPart.Count - 1);
            }

            return new BigNumber(intPart, decimalPart, value.IsPlus);
        }

        /// <summary>
        /// 整数部分右移处理
        /// </summary>
        private static void RightShiftIntegerPart(ref List<int> intPart, ref List<int> decimalPart, int shiftBits)
        {
            if (intPart.Count == 0)
                return;

            int bitsRemaining = shiftBits;
            while (bitsRemaining > 0 && intPart.Count > 0)
            {
                int currentShift = Math.Min(bitsRemaining, BigNumber.OneCount);
                int divisor = (int)Math.Pow(2, currentShift);
                int remainder = 0;

                // 从最高位开始处理
                for (int i = 0; i < intPart.Count; i++)
                {
                    long temp = (long)remainder * BigNumber.Max + intPart[i];
                    intPart[i] = (int)(temp / divisor);
                    remainder = (int)(temp % divisor);
                }

                // 将余数转移到小数部分
                if (remainder > 0)
                {
                    if (decimalPart.Count == 0)
                        decimalPart.Add(0);

                    // 将余数作为最高位加入小数部分
                    decimalPart.Insert(0, remainder * (BigNumber.Max / divisor));
                    CarryOverDecimal(decimalPart, 0);
                }

                // 移除整数部分的高位零
                if (intPart.Count > 0 && intPart[0] == 0)
                {
                    intPart.RemoveAt(0);
                }

                bitsRemaining -= currentShift;
            }
        }

        /// <summary>
        /// 小数部分右移处理
        /// </summary>
        private static void RightShiftDecimalPart(ref List<int> decimalPart, int shiftBits)
        {
            if (decimalPart.Count == 0 || shiftBits <= 0)
                return;

            int bitsRemaining = shiftBits;
            while (bitsRemaining > 0 && decimalPart.Count > 0)
            {
                int currentShift = Math.Min(bitsRemaining, BigNumber.OneCount);
                int divisor = (int)Math.Pow(2, currentShift);
                int remainder = 0;

                // 从最低位开始处理
                for (int i = decimalPart.Count - 1; i >= 0; i--)
                {
                    long temp = (long)decimalPart[i] + (long)remainder * BigNumber.Max;
                    decimalPart[i] = (int)(temp / divisor);
                    remainder = (int)(temp % divisor);
                }

                // 如果还有余数，可以舍去或考虑四舍五入
                // 这里选择直接舍去余数，如需四舍五入可额外处理

                bitsRemaining -= currentShift;
            }
        }

        /// <summary>
        /// 小数部分进位处理
        /// </summary>
        private static void CarryOverDecimal(List<int> decimalPart, int position)
        {
            while (position < decimalPart.Count && decimalPart[position] >= BigNumber.Max)
            {
                int carry = decimalPart[position] / BigNumber.Max;
                decimalPart[position] %= BigNumber.Max;

                if (position == decimalPart.Count - 1)
                {
                    decimalPart.Add(carry);
                }
                else
                {
                    decimalPart[position + 1] += carry;
                }

                position++;
            }
        }



        #endregion

        #region 辅助方法

        /// <summary>将大数的整数部分转换为二进制位列表</summary>
        private static List<bool> ConvertToBits(List<int> intPart)
        {
            List<bool> bits = new List<bool>();
            if (intPart.Count == 1 && intPart[0] == 0)
            {
                bits.Add(false);
                return bits;
            }

            List<int> number = new List<int>(intPart);
            while (true)
            {
                // 检查是否已经为0
                bool allZero = true;
                foreach (var n in number)
                {
                    if (n != 0)
                    {
                        allZero = false;
                        break;
                    }
                }
                if (allZero) break;

                // 除以2取余数
                int remainder = 0;
                for (int i = 0; i < number.Count; i++)
                {
                    int value = number[i] + remainder * BigNumber.Max;
                    number[i] = value / 2;
                    remainder = value % 2;
                }

                bits.Add(remainder != 0);
            }

            // 移除前导零
            while (bits.Count > 1 && bits[bits.Count - 1] == false)
            {
                bits.RemoveAt(bits.Count - 1);
            }

            bits.Reverse(); // 反转使低位在前
            return bits;
        }

        /// <summary>将二进制位列表转换回大数</summary>
        private static BigNumber ConvertFromBits(List<bool> bits)
        {
            if (bits.Count == 0)
                return BigNumber.Zero;

            List<int> result = new List<int> { 0 };
            for (int i = 0; i < bits.Count; i++)
            {
                // 乘以2
                int carry = 0;
                for (int j = 0; j < result.Count; j++)
                {
                    int value = result[j] * 2 + carry;
                    result[j] = value % BigNumber.Max;
                    carry = value / BigNumber.Max;
                }
                if (carry > 0)
                {
                    result.Add(carry);
                }

                // 加上当前位
                if (bits[i])
                {
                    int index = 0;
                    int add = 1;
                    while (add > 0 && index < result.Count)
                    {
                        int sum = result[index] + add;
                        result[index] = sum % BigNumber.Max;
                        add = sum / BigNumber.Max;
                        index++;
                    }
                    if (add > 0)
                    {
                        result.Add(add);
                    }
                }
            }

            result.Reverse(); // 恢复高位在前
            return new BigNumber(result, new List<int>(), true);
        }

        /// <summary>对齐两个二进制数的位数</summary>
        private static void AlignBits(ref List<bool> bitsOne, ref List<bool> bitsTwo)
        {
            int maxLength = Math.Max(bitsOne.Count, bitsTwo.Count);
            while (bitsOne.Count < maxLength)
            {
                bitsOne.Add(false); // 补零
            }
            while (bitsTwo.Count < maxLength)
            {
                bitsTwo.Add(false); // 补零
            }
        }

        #endregion











    }
}
