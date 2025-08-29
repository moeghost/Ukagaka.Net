using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace Ukagaka
{
    public class SCBlockedDescription
    {
        Hashtable root; // key : (String)キー , content : (String)値 or (Vector)値の集合

        public SCBlockedDescription()
        {
            // 空のオブジェクトを作成する。
            root = new Hashtable();
        }

        public SCBlockedDescription(Utils.File f)
        {
            // ファイルから読み込む。
            // 存在しなければ空のオブジェクトを作成する。
            root = new Hashtable();
            if (f.Exists())
            {
                try
                {
                    new Parser(root, new FileStream(f.GetPath(), FileMode.Open)).parse();
                }
                catch (Exception e)
                {
                    // System.err.println("load failed: " + f.getAbsolutePath());
                    //  e.printStackTrace();
                }
            }
        }

        public SCBlockedDescription(FileStream fs)
        {
            // ストリームから読み込む。
            root = new Hashtable();
            try
            {
                new Parser(root, fs).parse();
            }
            catch (Exception e)
            {
                // System.err.println("load failed");
                // e.printStackTrace();
            }
        }

        public Object get(String key)
        {
            // 返されるのはStringかVectorのどちらかである。
            return root[key];
        }

        public bool exists(String key)
        {
            return (root[key] != null);
        }

        public bool isBlock(String key)
        {
            Object obj = root[key];
            return obj != null && obj is List<String>;
        }

        public ICollection keys()
        {
            return root.Keys;
        }

        public IDictionaryEnumerator elements()
        {
            return root.GetEnumerator();
        }

        protected class Parser
        {
            Hashtable root;

            List<String> overread;
            StreamReader sr;

            public Parser(Hashtable root, FileStream fs)
            {
                this.root = root;

                overread = new List<String>(); // 読み過ぎた行
                sr = new StreamReader(fs, Encoding.UTF8);
            }

            protected String nextLine()
            {
                String line;
                if (overread.Count > 0)
                {
                    line = (String)overread[0];
                    overread.RemoveAt(0);
                }
                else
                {
                    line = sr.ReadLine();
                }
                return line;
            }

            protected void rollBack(String line)
            {
                if (line != null)
                {
                    overread.Insert(0, line);

                }
            }

            public void parse()
            {
                while (true)
                {
                    String line = nextLine();
                    if (line == null) break; // eof
                    line = line.Trim();
                    if (line.Length == 0)
                    {
                        continue; // 空の行は無視。
                    }
                    if (line.StartsWith("//"))
                    {
                        continue; // コメント行は無視。
                    }

                    List<String> entryname = new List<String>();

                    if (line.IndexOf(',') != -1)
                    { // 単一行エントリっぽい
                      // 次の行が「{」だったらエントリ複製。
                      // そうでなければ単一行エントリ。
                        String next = nextLine();
                        // 読み過ぎたので巻戻し
                        rollBack(next);

                        if (next != null && next.Trim().Equals("{"))
                        {

                            string[] entries = line.Split(',');
                            foreach (string entrie in entries)
                            {
                                entryname.Add(entrie);

                            }

                        }
                        else
                        {
                            int commapos = line.IndexOf(',');
                            String key = line.Substring(0, commapos);
                            String value = line.Substring(commapos + 1);
                            root.Add(key, value);
                            continue;
                        }
                    }
                    else
                    {
                        entryname.Add(line);
                    }

                    // 単一行でなければ複数行に渡るエントリなので
                    // ブレスの始まりから終わりまでを読み込む。
                    List<String> elements = new List<String>();

                    try
                    {
                        if (!nextLine().Trim().Equals("{"))
                        {
                            break; // ブレスで始まっていなければエラー。
                        }

                    }
                    catch (Exception e)
                    {
                        break;
                    }
                    while (true)
                    {
                        String inner_line = nextLine();
                        if (inner_line == null)
                        {
                            break; // ブロックが終了する前にEOFに達した。エラー。
                        }
                        if (inner_line.Length == 0)
                        {
                            continue; // 空の行は無視。
                        }
                        if (inner_line.StartsWith("//"))
                        {
                            continue; // コメント行は無視。

                        }
                        if (inner_line.Trim().Equals("}"))
                        {
                            break; // ブロックが正常に終了した。
                        }
                        elements.Add(inner_line);
                    }

                    foreach (string e in entryname)
                    {
                        root.Add(e, elements);

                    }

                }
                sr.Close();
            }
        }



    }
}
