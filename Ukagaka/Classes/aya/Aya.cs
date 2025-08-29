using System;
using System.Collections;

using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using aya.Eval;
using aya.Node;
namespace aya
{

    public class Aya
    {
        public static readonly string CHARSET = "utf-8";

        private readonly Utils.File ghostDir;
        private readonly Dictionary dic;
        private readonly GlobalNamespace globalNamespace;
        private readonly SystemCall syscall;

        private string lastReqMethod;
        private string lastReqProtocol;
        private readonly ArrayList lastReqHeadersV; // String[]; [0]:key [1]:value
        private readonly Hashtable lastReqHeadersH; // {key => value}

        public Aya(Utils.File ghostDir, Utils.File shioriDll)
        {
            this.ghostDir = ghostDir;
            this.dic = new Dictionary(this);
            this.globalNamespace = new GlobalNamespace();
            this.syscall = new SystemCall(this);

            this.lastReqMethod = null;
            this.lastReqProtocol = null;
            this.lastReqHeadersV = new ArrayList();
            this.lastReqHeadersH = new Hashtable();

            // .ayajava-savedata.txtがあれば、aya.txtより先に読んで @@@SAVEDATA@@@ を評価する。
            FileInfo savedata = new FileInfo(Path.Combine(ghostDir.GetFullName(), ".ayajava-savedata.txt"));
            if (savedata.Exists)
            {
                try
                {
                    dic.LoadFile(savedata);
                    Function fSavedata = dic.GetFunction("@@@SAVEDATA@@@");
                    if (fSavedata != null)
                    {
                        fSavedata.Eval(null);

                        // 評価したら、この関数はもう要らないので消す。
                        dic.UndefFunction("@@@SAVEDATA@@@");
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Couldn't load savedata.\n");
                    Console.Error.WriteLine(e.StackTrace);
                }
            }

            // aya.dllならaya.txtを、shamrock.dllならshamrock.txtを読む。
            Match m = Regex.Match(shioriDll.GetName(), "^(.+?)\\.[^\\.]+$");
            string bootstrapfile;
            if (m.Success)
            {
                bootstrapfile = m.Groups[1].Value + ".txt";
            }
            else
            {
                bootstrapfile = "aya.txt";
            }

            // aya.txtがあれば読む。
            FileInfo ayaTxt = new FileInfo(Path.Combine(ghostDir.GetFullName(), bootstrapfile));
            if (ayaTxt.Exists)
            {
                LoadAyaConfigFile(ayaTxt);
            }

            // OnLoadを呼ぶ。
            // 唯一のリクエストヘッダである Path ヘッダからは、aya.dll のフルパスを得ることができる。
            try
            {
                Function onLoad = dic.GetFunction("OnLoad");
                if (onLoad != null)
                {
                    lastReqMethod = "";
                    lastReqProtocol = "";

                    FileInfo fullPath = new FileInfo(Path.Combine(ghostDir.GetFullName(), "aya.dll"));
                    lastReqHeadersV.Add(new string[] { "Path", fullPath.FullName });
                    lastReqHeadersH["Path"] = fullPath.FullName;

                    onLoad.Eval(null);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        public GlobalNamespace GetGlobalNamespace()
        {
            return globalNamespace;
        }

        public Dictionary GetDictionary()
        {
            return dic;
        }

        public SystemCall GetSystemCall()
        {
            return syscall;
        }

        public string GetMethodOfLastRequest()
        {
            return lastReqMethod;
        }

        public string GetProtocolOfLastRequest()
        {
            return lastReqProtocol;
        }

        public string GetKeyOfLastRequest(int index)
        {
            if (index >= lastReqHeadersV.Count)
            {
                return null;
            }
            else
            {
                string[] entry = (string[])lastReqHeadersV[index];
                return entry[0];
            }
        }

        public string GetValueOfLastRequest(int index)
        {
            if (index >= lastReqHeadersV.Count)
            {
                return null;
            }
            else
            {
                string[] entry = (string[])lastReqHeadersV[index];
                return entry[1];
            }
        }

        public string GetValueOfLastRequest(string key)
        {
            return (string)lastReqHeadersH[key];
        }

        protected void LoadAyaConfigFile(FileInfo confFile)
        {
            try
            {
                using (StreamReader sr = new StreamReader(confFile.FullName, Encoding.GetEncoding(CHARSET)))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        int doubleSlashPos = line.IndexOf("//", StringComparison.Ordinal);
                        if (doubleSlashPos != -1)
                        {
                            line = line.Substring(0, doubleSlashPos);
                        }

                        if (line.Length == 0) continue;

                        int commaPos = line.IndexOf(',', StringComparison.Ordinal);
                        if (commaPos == -1) continue;
                        string key = line.Substring(0, commaPos).Trim();
                        string value = line.Substring(commaPos + 1).Trim();

                        EvaluateAyaConfDefinition(key, value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("SCAya: exception occurred in loading Aya Config File (aya.txt).");
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        protected void EvaluateAyaConfDefinition(string key, string value)
        {
            if (key.Equals("dic"))
            {
                // 辞書を読み込む
                FileInfo dicFile = new FileInfo(Path.Combine(ghostDir.GetFullName(), ConvertWinPathToUnixOne(value)));
                dic.LoadFile(dicFile);
            }
        }

        public static string ConvertWinPathToUnixOne(string path)
        {
            // Windowsのパス区切り￥を、Unixのパス区切り/に置き換えます。
            StringBuilder buf = new StringBuilder(path);
            while (true)
            {
                int enPos = buf.ToString().IndexOf('\\');
                if (enPos == -1) enPos = buf.ToString().IndexOf('\u00a5');
                if (enPos == -1) break; // 終了

                buf[enPos] = '/';
            }
            return buf.ToString();
        }

        public static bool CheckIfUsable(Utils.File ghostDir, Utils.File shioriDll)
        {
            // 手抜き
            FileInfo ayaTxt = new FileInfo(Path.Combine(ghostDir.GetFullName(), "aya.txt"));
            if (ayaTxt.Exists) return true;
            FileInfo ayaDll = new FileInfo(Path.Combine(ghostDir.GetFullName(), "aya.dll"));
            if (ayaDll.Exists) return true;

            // shiori.dllにaya.dllの文字列が含まれていたらtrue。
            return (FindData(shioriDll, Encoding.ASCII.GetBytes("aya.dll")) != -1);
        }

        public static int FindData(Utils.File f, byte[] data)
        {
            // ファイルからバイト列を検索し、見つかればそれが始まるオフセットを、見つからなければ-1を返します。
            // 現在の実装ではファイルを丸ごとメモリに読み込んでしまうので、あまりに巨大なファイルに対しては利用不可能です。
            if (!f.Exists())
            {
                return -1;
            }

            try
            {
                using (FileStream fs = new FileStream(f.GetFullName(), FileMode.Open))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buf = new byte[512];
                        int count;
                        while ((count = fs.Read(buf, 0, buf.Length)) != 0)
                        {
                            ms.Write(buf, 0, count);
                        }
                        byte[] world = ms.ToArray();

                        return FindData(world, data);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return -1;
            }
        }

        public static int FindData(byte[] world, byte[] data)
        {
            // world内からdataを検索し、見つかればそのオフセットを、見つからなければ-1を返します。
            // Boyer Moore法を用いる。
            int dataLen = data.Length;
            int[] skip = new int[256];
            for (int i = 0; i < 256; i++)
            {
                skip[i] = dataLen;
            }
            for (int i = 0; i < dataLen - 1; i++)
            {
                // byte -> -128 から +127 まで。
                // -1は255に、-2は254に、…、-128は128に対応する。
                // つまり、負の範囲ではそれぞれ(256 + n)に対応する。
                skip[(data[i] < 0 ? 256 + data[i] : data[i])] = dataLen - i - 1;
            }
            int limit = world.Length - data.Length;
            for (int i = 0; i <= limit; i += skip[(world[i + dataLen - 1] < 0 ? 256 + world[i + dataLen - 1] : world[i + dataLen - 1])])
            {
                if (world[i + dataLen - 1] != data[dataLen - 1])
                {
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
                {
                    return i;
                }
            }
            return -1;
        }

        public void Terminate()
        {
            // OnUnloadを読んでから、
            try
            {
                Function onUnload = dic.GetFunction("OnUnload");
                if (onUnload != null)
                {
                    onUnload.Eval(null);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }

            // 大域的名前空間をファイル .ayajava-savedata.txt に保存する。
            globalNamespace.SaveToFile(new FileInfo(Path.Combine(ghostDir.GetFullName(), ".ayajava-savedata.txt")));
        }

        public static string GetModuleName()
        {
            return "Aya";
        }

        private static readonly Regex PatReqCommand = new Regex("^(.+?)\\s+(.+)$");
        private static readonly Regex PatReqHeader = new Regex("^(.+?)\\s*:\\s*(.+)$");

        public string Request(string req)
        {
            using (StringReader sr = new StringReader(req))
            {
                // まずはヘッダを読む
                string header = null;
                try { header = sr.ReadLine(); }
                catch (Exception) { }

                // メソッドとプロトコルに分解
                Match mat;
                if ((mat = PatReqCommand.Match(header)).Success)
                {
                    lastReqMethod = mat.Groups[1].Value;
                    lastReqProtocol = mat.Groups[2].Value;
                }
                else
                {
                    return "SHIORI/3.0 400 Bad Request\r\n\r\n";
                }

                // リクエスト全体を読む
                lastReqHeadersV.Clear();
                lastReqHeadersH.Clear();
                string line = null;
                while (true)
                {
                    try { line = sr.ReadLine(); }
                    catch (Exception) { }
                    if (line == null) break;

                    if ((mat = PatReqHeader.Match(line)).Success)
                    {
                        string[] entry = { mat.Groups[1].Value, mat.Groups[2].Value };

                        lastReqHeadersV.Add(entry);
                        lastReqHeadersH[entry[0]] = entry[1];
                    }
                }
            }

            try
            {
                Function onRequest = dic.GetFunction("OnRequest");
                if (onRequest != null)
                {
                    return onRequest.Eval(null).GetString();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
            }
            return "SHIORI/3.0 500 Internal Server Error\r\n\r\n";
        }
    }

}
