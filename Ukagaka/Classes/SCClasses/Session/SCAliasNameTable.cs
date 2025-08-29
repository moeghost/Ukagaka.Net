using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
 
namespace Ukagaka
{
    public  class SCAliasNameTable
    {
        Hashtable root; // この中にもエントリ分のHashtableが入り、その中にはVector、その中にIntegerが入ります。

        public SCAliasNameTable()
        {
            root = new Hashtable();
        }

        public void load(Utils.File f)
        {
            // f : alias.txt
            // 「sakura.surface.alias」等の記述を読んで、テーブルを作成します。
            if (!f.Exists() || f.IsDirectory())
            {
                return;
            }

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(new FileStream(f.GetPath(),FileMode.Open), Encoding.UTF8);
            }
            catch (Exception e)
            {
                return;
            }

            String line = null;
            while (true)
            {
                try
                {
                    line = sr.ReadLine();
                }
                catch (Exception e)
                {

                }

                if (line == null)
                {
                    break;
                }
                line = line.Trim();
                if (line.Length == 0)
                {
                    continue; // 空の行はスキップ。
                }
                if (line.StartsWith("//"))
                {
                    continue; // コメント行
                }
                if (line.IndexOf(',') != -1)
                {
                    continue; // カンマを含んだ行はブロック指定でないのでスキップ。
                }
                // さて、ここでlineはrootに入れるべきエントリ名になっている。
                // 次の行は括弧の始まりであるべき。そうでなければテーブル作成はここで強制終了。
                // エントリ名は「.surface.alias」で終わっていなければならない。
                String entryname = line;
                try {
                    line = sr.ReadLine();
                } catch (Exception e)
                {

                }
                if (line == null || !line.Equals("{"))
                {
                    break;
                }
                // その次の行から括弧が閉じられるまでブロックデータ。
                Hashtable current_table = null;
                if (entryname.EndsWith(".surface.alias"))
                {
                    current_table = new Hashtable();
                }
                while (true)
                {
                    try
                    {
                        line = sr.ReadLine();
                    }
                    catch (Exception e)
                    {
                    }

                    if (line == null) break;
                    line = line.Trim();
                    if (line.Equals("}"))
                    {
                        break; // ブロック終了
                    }
                    if (current_table == null) {
                        continue;
                    }
                    if (line.StartsWith("//"))
                    {
                        continue;
                    }
                    if (line.IndexOf(',') == -1)
                    {
                        continue;
                    }
                    int comma_pos = line.IndexOf(',');
                    String label = StringExtension.Substring(line, 0, comma_pos); // line.Substring(0, comma_pos);
                    String data = line.Substring(comma_pos + 1);
                    // dataは[id,id,id,...]形式なのでこれをバラす。

                    data = data.Substring(1, data.Length - 1); // 前後の括弧を消す。

                    List<int> array = new List<int>();
                    string[] st = data.Split(',');

                    foreach(string s in st) 
                    {
                        array.Add(Integer.ParseInt(s));
                    }
                     
                    // テーブルにセット
                    current_table.Add(label, array);
                }

                // ルートテーブルにセット
                if (current_table != null)
                {
                    root.Add(entryname, current_table);
                }
            }

            try
            {
                sr.Close();
            }
            catch (IOException e)
            {
            }
        }

        public int lookup(String entry, String label)
        {
            // entry : sakura.surface.alias等
            // label : angry等
            // 該当するデータが無ければ-1を返します。
            Hashtable table = (Hashtable)root[entry];
            if (table == null)
            {
                return -1;
            }

            List<int> array = (List<int>)table[label];
            if (array == null)
            {
                return -1;
            }
            if (array.Count == 0)
            {
                return -1;
            }
            if (array.Count == 1)
            {
                return array.ElementAt(0);
            }
            int item_to_select = new Random().Next(0,array.Count);
            if (item_to_select == array.Count)
            {
                item_to_select--;
            }
            return array.ElementAt(item_to_select);
        }

    }
}
