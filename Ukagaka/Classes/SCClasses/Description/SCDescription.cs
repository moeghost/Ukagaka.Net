using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
 
using System.Collections;
 
using System.IO;
namespace Ukagaka
{
    public class SCDescription
    {
        Dictionary<string,string> table; // 中身はstring。

        public SCDescription()
        {
            // 空のインスタンスを作成します。
        }

        public SCDescription(Utils.File f)
        {
            // fはdescript.txtのファイル。
            // ゴーストのルートにあったりバルーンスキンにあったりするファイルです。
            // フォーマットは同じなのでどちらにも使えます。
            // ファイルが存在しなかったら省略されたものとします。
            try
            {


                Load(new FileStream(f.GetPath(), FileMode.Open));
            }
            catch (FileNotFoundException e) 
            {


                ;

            }
        }

        public SCDescription(FileStream FS)
        {
            Load(FS);
        }

        public SCDescription(SCDescription parent, Utils.File f)
        {
            // parentをオーバーライドしたSCDescriptionを作成します。
            // ファイルが存在しなかったら省略されたものとします。

            // コピー
            if (parent.GetHashtable() != null)
            {
                table = (Dictionary<string, string>)parent.GetHashtable();
            }

            try
            {
                Load(new FileStream(f.GetPath(), FileMode.Open));
            }
            catch (FileNotFoundException e) { }
        }

        public SCDescription(SCDescription parent, FileStream FS)
        {
            // コピー
            if (parent.GetHashtable() != null)
            {
                table = parent.GetHashtable();
            }

            Load(FS);
        }

        private void Prepare()
        {
            // これがインライン展開されるのなら…
            if (table == null)
            {
                table = new Dictionary<string, string>();
            }
        }

        private void Load(FileStream IS)
        {

            StreamReader sr = null;



            // BufferedReader br = null;
            try
            {
                sr = new StreamReader(IS, Encoding.UTF8);
            }
            catch (Exception e)
            {
                return;
            }

            string line = null;
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
                if (line.IndexOf(',') == -1)
                {
                    continue;
                }
                // 大括弧で囲まれている範囲は無視する。
                if (line.Equals("{"))
                {
                    while (true)
                    {
                        try
                        {
                            line = sr.ReadLine();
                        }
                        catch (Exception e)
                        {
                        }
                        if (line == null || line.Equals("}"))
                        {
                            break;
                        }
                    }
                    // 最終的にcontinueして閉じ括弧をスキップ。
                    continue;
                }

                // コンマを境に前後で分割。
                int comma_pos = line.IndexOf(',');
                string label = line.Substring(0, comma_pos).Trim(); // 9 septiembre 2002
                string data = line.Substring(comma_pos + 1).Trim(); // 9 septiembre 2002

                // セット
                if (table == null)
                {
                    Prepare();
                }
                // table.Add(label, data);
                table[label] = data;
            }

            try
            {
                sr.Close();
            }
            catch (IOException e)
            {
            }
        }

        public void Save(FileInfo dest)
        {
            if (dest.Exists)
            {
                dest.Delete();
            }
            StreamWriter sw = new StreamWriter(new FileStream(dest.FullName, FileMode.Create), Encoding.UTF8);

            if (table != null)
            {

                foreach (string key in table.Keys)
                {

                    sw.Write(key);
                    sw.Write(',');
                    sw.Write(table[key]);
                    sw.Write("\r\n");
                }

            }

            sw.Flush();
            sw.Close();
        }

        public bool Exists(string key)
        {
            return table != null && table.ContainsKey(key);
        }

        public string GetStrValue(string key)
        {
            if (!Exists(key))
            {
                return "";
            }

            return table == null ?
                null : (string)table[key];
        }

        public void SetStrValue(string key, string data)
        {
            if (table == null)
            {
                Prepare();
            }
            // table.Add(key, data);
            table[key] = data;
        }

        public void SetIntValue(string key, int data)
        {
            if (table == null)
            {
                Prepare();
            }
            // table.put(key, Integer.Tostring(data));

            table[key] = data.ToString();
        }

        public int GetIntValue(string key)
        {
            // データが存在しなかったり数値でなかったら0を返します。
            if (table == null)
            {
                return 0;
            }

            string strval = GetStrValue(key);
            if (strval == null) return 0;

            int val = 0;
            try
            {
                val = Integer.ParseInt(strval);
            }
            catch (Exception e) { }

            return val;
        }

        public void Remove(string key)
        {
            if (table != null)
            {
                table.Remove(key);
            }
        }

        public object InverseSearch(object data)
        {
            // ハッシュテーブル内を逆引きして、見つけたキーを一つ返します。
            if (table == null)
            {
                return null;
            }

            foreach (string key in table.Keys)
            {
                
                if (table[key].Equals(data))
                {

                    return key;
                }
                

            }

            return null;
        }

        public List<string> GetKeysStartsWith(string str)
        {
            // 指定された文字列で始まるキーを全て抜き出して返します。
            // 返された配列を元に必要に応じてデータを参照するなどして下さい。
            // 見つからなければ空の配列が返されます。
            List<string> result = new List<string>();
            if (table != null)
            {


                foreach (string key in table.Keys)
                {
                    
                    if (key.StartsWith(str))
                    {
                        result.Add(key);

                    }
                    
                }

            }
            return result;
        }

        public List<string> GetAllKeys()
        {


            List<string> result = new List<string>();
            if (table != null)
            {

                foreach (string key in table.Keys)
                {
 

                    result.Add(key);

                }

            }

            return result;
        }

        protected Dictionary<string, string> GetHashtable()
        {
            // nullを返すかも知れない事に注意。
            return table;
        }

        Object tag;
        public void SetTag(object tag)
        {
            /* デバッグ用 */
            this.tag = tag;
        }

        public string Tostring()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("content: ");
            if (table != null && table.Count > 0)
            {
                buf.Append('{');

                foreach (string key in table.Keys)
                {
                    buf.Append(key).Append(" => {").
                       Append(table[key]).Append('}');

                    buf.Append(", ");

                }

                buf.Append('}');
            }
            else
            {
                buf.Append("empty");
            }
            if (tag != null)
            {
                buf.Append("; tag: " + tag);
            }
            return buf.ToString();
        }

        protected void Finalize()
        {
            if (tag != null)
            {
                // Logger.log(this, Logger.DEBUG, "finalized");
            }
        }



    }
}
