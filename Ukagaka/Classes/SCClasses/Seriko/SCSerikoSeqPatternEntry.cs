using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ukagaka
{
    public class SCSerikoSeqPatternEntry
    {



        // 基本パラメータ
        int surfaceid;
        int interval; // 単位は常にミリ秒
        String method;
        int offsetx, offsety;

        // start用
        int seq_id;

        // alternativestart用
        List<int> entries; // 中身はInteger。

        // insert用
        int bind_id;

        public SCSerikoSeqPatternEntry(int version, String dataline)
        {
            /*
              datalineには、[seqid]pattern[index],の後のデータを指定して下さい。

              例えば、
              1pattern0,50,20,overlay,10,0
              だったら
              50,20,overlay,10,0
              です。
            */
            int nextToken = 0;

            string[] st = dataline.Split(',');

            if (version == 0)
            {
                // surfaceidとintervalのフィールドは、例え0でパディングされているにしろ必ず存在する。
                try
                {
                    surfaceid = Integer.ParseInt(st[nextToken++]);
                }
                catch (Exception e)
                {
                    //   throw new RuntimeException(
                    //  "Invalid surcace ID in `" + dataline + "'; version " + version);
                }
                try
                {
                    interval = Integer.ParseInt(st[nextToken++]);
                }
                catch (Exception e)
                {
                    // throw new RuntimeException(
                    //    "Invalid interval in `" + dataline + "'; version " + version);
                }
                try
                {
                    method = st[nextToken++];
                }
                catch (Exception e)
                {
                    // throw new RuntimeException(
                    //    "Missing method in `" + dataline + "'; version " + version);
                }

                // intervalはSERIKO/2.0でなければ10ms單位になつてゐる。
                interval *= 10;
            }
            else
            {
                // methodは常に存在する。
                try
                {
                    method = st[nextToken++];
                }
                catch (Exception e)
                {
                    //throw new RuntimeException(
                    //   "Missing method in `" + dataline + "'; version " + version);
                }
            }

            // methodに従い、追加データを読み込む。
            if (method.Equals("start"))
            {
                try
                {
                    seq_id = Integer.ParseInt(st[nextToken++]);
                }
                catch (Exception e)
                {
                    // throw new RuntimeException(
                    //     "Invalid sequense ID in Start method `" + dataline + "'; version " + version);
                }
            }
            else if (method.Equals("insert"))
            {
                try
                {
                    bind_id = Integer.ParseInt(st[nextToken++]);
                }
                catch (Exception e)
                {
                    //  throw new RuntimeException(
                    //     "Invalid bind ID in insert method `" + dataline + "'; version " + version);
                }
            }
            else if (method.Equals("alternativestart"))
            {
                String altana_param = st[nextToken++]; // 殘りを全て取出す。
                if (altana_param.Length > 0 && StringExtension.CharAt(altana_param, 0) == ',')
                {
                    altana_param = altana_param.Substring(1);
                }

                // 前後の括弧を消す。SERIKO/2.0では「()」、さうでなければ「[]」。
                char blockstart = StringExtension.CharAt(altana_param, 0);
                char blockend = StringExtension.CharAt(altana_param, altana_param.Length - 1);


                if (altana_param.Length <= 2 ||
                (version == 0 && blockstart != '[' && blockend != ']') ||
                (version == 1 && blockstart != '(' && blockend != ')'))
                {
                    //  throw new RuntimeException(
                    //    "Malformed parameter of alternativestart: `" + dataline + "'; version " + version);
                }
                String entry_list = StringExtension.Substring(altana_param, 1, altana_param.Length - 1);  //altana_param.Substring(1, altana_param.Length - 1);

                entries = new List<int>();
                String[] ent_st = entry_list.Split(version == 0 ? '.' : ',');



                foreach (String token in ent_st)
                {
                    try
                    {
                        entries.Add(Integer.ParseInt(token));

                    }
                    catch (Exception e)
                    {
                        // throw new RuntimeException(
                        //  "Invalid parameter of alternative Start: `" + token + "' in `" +
                        //  dataline + "'; version " + version);
                    }
                }
            }
            else
            {
                if (version == 1)
                {
                    try
                    {
                        surfaceid = Integer.ParseInt(st[nextToken++]);
                    }
                    catch (Exception e)
                    {
                        //  throw new RuntimeException(
                        //   "Invalid surcace ID in `" + dataline + "'; version " + version);
                    }
                    try
                    {
                        interval = Integer.ParseInt(st[nextToken++]);
                    }
                    catch (Exception e)
                    {
                        // throw new RuntimeException(
                        // "Invalid interval in `" + dataline + "'; version " + version);
                    }
                    //  catch (Exception e) { } // intervalは存在しないかも知れない。
                }

                try
                {
                    offsetx = Integer.ParseInt(st[nextToken++]);
                }
                catch (Exception e)
                {
                    //  throw new RuntimeException(
                    //      "Invalid offset-x in `" + dataline + "'; version " + version);
                }
                //catch (Exception e) { } // offsetx, offsetyは存在しないかも知れない。

                try
                {
                    offsety = Integer.ParseInt(st[nextToken++]);
                }
                catch (Exception e)
                {
                    //     throw new RuntimeException(
                    //      "Invalid offset-y in `" + dataline + "'; version " + version);
                }
                // catch (Exception e) { }
            }
        }

        public void dump()
        {
            System.Console.WriteLine(surfaceid + "," + interval + "," + method + "," + offsetx + "," + offsety);
        }

        public int Surfaceid()
        {
            return surfaceid;
        }

        public int Interval()
        {
            return interval;
        }

        public String Method()
        {
            return method;
        }

        public int Offsetx()
        {
            return offsetx;
        }

        public int Offsety()
        {
            return offsety;
        }

        public int Seq_id()
        {
            return seq_id;
        }

        public List<int> Entries()
        {
            return entries;
        }

        public int Bind_id()
        {
            return bind_id;
        }











    }
}
