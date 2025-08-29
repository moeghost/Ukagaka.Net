using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    public static class SCMiscUtils
    {
        /// <summary>
        /// 文字列をUTF8バイト列に変換し、そのMD5を16進文字列として返します。
        /// </summary>
        public static string GetMD5AsString(string str)
        {
            return BytesToHexString(CalcMD5(str));
        }

        /// <summary>
        /// 文字列をデフォルト文字コード(UTF8)でバイト列に変換し、そのMD5を求める。
        /// </summary>
        public static byte[] CalcMD5(string str)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            }
        }

        /// <summary>
        /// バイト列を16進文字列に変換します。
        /// </summary>
        public static string BytesToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2")); // 小文字16進
            }
            return sb.ToString();
        }

        /// <summary>
        /// world内からdataを検索し、見つかればそのオフセットを、見つからなければ-1を返します。(Boyer Moore法)
        /// </summary>
        public static int FindData(byte[] world, byte[] data)
        {
            int dataLen = data.Length;
            int[] skip = new int[256];

            for (int i = 0; i < 256; i++)
                skip[i] = dataLen;

            for (int i = 0; i < dataLen - 1; i++)
            {
                int idx = data[i] < 0 ? 256 + data[i] : data[i];
                skip[idx] = dataLen - i - 1;
            }

            int limit = world.Length - data.Length;
            for (int i = 0; i <= limit;)
            {
                int w = world[i + dataLen - 1];
                int idx = w < 0 ? 256 + w : w;
                if (world[i + dataLen - 1] != data[dataLen - 1])
                {
                    i += skip[idx];
                    continue;
                }

                bool matched = true;
                for (int j = 0; j < dataLen; j++)
                {
                    if (world[i + j] != data[j])
                    {
                        matched = false;
                        break;
                    }
                }
                if (matched)
                    return i;

                i += skip[idx];
            }
            return -1;
        }

        /// <summary>
        /// delimに含まれている文字ならどれでも1文字単位で分割する。
        /// </summary>
        public static List<string> Split(string src, string delim)
        {
            if (src == null) return null;

            var parts = src.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return parts.ToList();
        }

        /// <summary>
        /// delimをそのまま区切り文字として結合する。
        /// </summary>
        public static string Join(string delim, List<string> list)
        {
            return string.Join(delim, list);
        }
    }

}
