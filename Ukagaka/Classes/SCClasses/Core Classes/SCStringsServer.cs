using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cocoa.AppKit;
namespace Ukagaka
{
    public class SCStringsServer
    {
         public static String GetStrFromMainDic(String label)
        {
            // 見つからなかったらラベルそのものを返します。
            return NSBundle.MainBundle().LocalizedStringForKey(label, label, "MainStrDic");
        }

        public static String GetStrFromMainDic(String label, String[] paras)
        {
            // データベース内のメッセージに含まれていた#1、#2といった文字列をパラメータで置き換えます。
            // 下限は#1、上限はありません。
            // 見つからなかったらラベルそのものを返します。
            StringBuffer buf = new StringBuffer(NSBundle.MainBundle().LocalizedStringForKey(label, label, "MainStrDic"));

            for (int i = 0; i < paras.Length; i++)
		{
                String meta = "#" + (i + 1);

                while (true) // 同じ番号で何度でも置き換えられる。
                {
                    int blockstart = buf.ToString().IndexOf(meta);
                    int blockend = blockstart + meta.Length;
                    if (blockstart == -1) break;

                    buf.Replace(blockstart, blockend,paras[i]);
                }
            }

            return buf.ToString();
        }


    }
}
