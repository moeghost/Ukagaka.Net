using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
namespace Ukagaka
{
    public static class SCFileUtils
    {
        /// <summary>
        /// 拷贝文件
        /// </summary>
        public static void Cp(string srcPath, string destPath)
        {
            using var src = new FileStream(srcPath, FileMode.Open, FileAccess.Read);
            using var dest = new FileStream(destPath, FileMode.Create, FileAccess.Write);

            byte[] buffer = new byte[1024];
            int read;
            while ((read = src.Read(buffer, 0, buffer.Length)) > 0)
            {
                dest.Write(buffer, 0, read);
            }
        }

        /// <summary>
        /// 比较两个文件内容是否完全相同
        /// </summary>
        public static bool SameFiles(string fileA, string fileB)
        {
            FileInfo fa = new FileInfo(fileA);
            FileInfo fb = new FileInfo(fileB);

            if (fa.Length != fb.Length) return false;

            using var fsA = new FileStream(fileA, FileMode.Open, FileAccess.Read);
            using var fsB = new FileStream(fileB, FileMode.Open, FileAccess.Read);

            byte[] bufA = new byte[1024];
            byte[] bufB = new byte[1024];

            int read;
            while ((read = fsA.Read(bufA, 0, bufA.Length)) > 0)
            {
                fsB.Read(bufB, 0, read);
                for (int i = 0; i < read; i++)
                {
                    if (bufA[i] != bufB[i]) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 递归删除文件或文件夹
        /// </summary>
        public static void DeleteRecursively(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path)) return;

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                foreach (var item in Directory.GetFileSystemEntries(path))
                {
                    DeleteRecursively(item);
                }
                Directory.Delete(path);
            }
        }

        /// <summary>
        /// 递归删除文件或文件夹（带排除列表）
        /// </summary>
        public static void DeleteRecursively(string path, List<string> undeleteMask)
        {
            if (!File.Exists(path) && !Directory.Exists(path)) return;

            if (File.Exists(path))
            {
                if (!undeleteMask.Contains(Path.GetFileName(path)))
                {
                    File.Delete(path);
                }
            }
            else if (Directory.Exists(path))
            {
                foreach (var item in Directory.GetFileSystemEntries(path))
                {
                    DeleteRecursively(item, undeleteMask);
                }
                if (!Directory.EnumerateFileSystemEntries(path).Any())
                {
                    Directory.Delete(path);
                }
            }
        }

        /// <summary>
        /// 获取文件 MD5 值（十六进制字符串）
        /// </summary>
        public static string GetMD5AsString(string filePath)
        {
            byte[] md5Bytes = CalcMD5(filePath);
            return BytesToHexString(md5Bytes);
        }

        /// <summary>
        /// 计算文件 MD5 值
        /// </summary>
        public static byte[] CalcMD5(string filePath)
        {
            using var md5 = MD5.Create();
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return md5.ComputeHash(fs);
        }

        /// <summary>
        /// 在文件中搜索字节序列（返回偏移量，找不到返回 -1）
        /// </summary>
        public static int FindData(string filePath, byte[] data)
        {
            try
            {
                byte[] world = File.ReadAllBytes(filePath);
                return FindData(world, data);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 在字节数组中搜索另一段字节序列
        /// </summary>
        private static int FindData(byte[] source, byte[] pattern)
        {
            for (int i = 0; i <= source.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }

        /// <summary>
        /// 删除文件扩展名
        /// </summary>
        public static string StringByDeletingExtension(string path)
        {
            return Path.HasExtension(path) ? Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path)) : path;
        }

        /// <summary>
        /// 工具：字节数组转十六进制字符串
        /// </summary>
        private static string BytesToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
