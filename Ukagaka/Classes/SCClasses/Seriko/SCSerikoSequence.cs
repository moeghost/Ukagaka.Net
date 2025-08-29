using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows;
namespace Ukagaka
{


    /*
     SCSerikoSequence

     ．キゥ`．ア．□ケメサ、ト、□□キ、゛、ケ。」
     タ□ィ、ミエホ、ホ、隍ヲ、ハ、筅ホ、ヌ、ケ。」

     0interval,sometimes
     0pattern0,100,5,overlay,0,0
     0pattern1,101,5,overlay,0,0
     0pattern2,100,5,overlay,0,0
     0pattern3,-1,5,overlay,0,0
     */
    public class SCSerikoSequence
    {
        SCSerikoSeqIntervalEntry interval; // intervalエントリの中身。
        SCSerikoSeqOptionEntry option; // optionエントリの中身。省略されていたらnull。
        List<SCSerikoSeqPatternEntry> patterns; // patternエントリの中身。中身はSCSerikoSeqPatternEntryまたはnull。パターンのIDと配列のIDは完全に一致します。
        int seq_id;

        public SCSerikoSequence(int seq_id)
        {
            patterns = new List<SCSerikoSeqPatternEntry>();
            this.seq_id = seq_id;
        }

        public String toString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("ID: ").Append(seq_id);
            buf.Append("; interval: {").Append(interval).Append('}');
            if (option != null)
            {
                buf.Append("; option: {").Append(option).Append('}');
            }
            return buf.ToString();
        }

        public void dump()
        {
            System.Console.Write("### Sequence ID : " + seq_id);
            interval.dump();
            if (option != null) option.dump();

            int n_pats = patterns.Count;
            for (int i = 0; i < n_pats; i++)
            {
                Object entry = patterns.ElementAt(i);
                if (entry is SCSerikoSeqPatternEntry)
                {
                    ((SCSerikoSeqPatternEntry)entry).dump();

                }
            }
        }

        public void add(int version, String dataline)
        {
            /*
              新たにエントリを追加します。
              エントリの形式は
              interval,sometimes
              pattern0,100,5,overlay,0,0
              であって、
              0interval,sometimes
              0pattern0,100,5,overlay,0,0
              ではありません。

              SERIKO/2.0の場合でも同樣に、
              animation0.pattern0,100,5,overlay,0,0
              の代はりに
              pattern0,100,5,overlay,0,0
              で指定して下さい。
            */

            if (dataline.StartsWith("interval"))
            {
                interval = new SCSerikoSeqIntervalEntry(dataline.Substring(9));
            }
            else if (dataline.StartsWith("pattern") || dataline.StartsWith("patturn"))
            {
                /*
                  閑馬たんの仕樣書の誤字で、patternがpatturnになつてゐる個所がある。
                  そのため、實際にpatturnを使つてゐるシェルも存在する。
                */

                // パターンIDを取り出す。
                int pat_id_begin = 7;
                int pat_id_end = 7;

                //stirng.Substring(index, 1) is stirng.charAt(index)

                while (Char.IsDigit(StringExtension.CharAt(dataline, pat_id_end)))
                {
                    pat_id_end++;
                }

                if (pat_id_begin == pat_id_end)
                {
                    // パターンIDが無い。
                    //throw new RuntimeException(
                    //   "Parse failed: missing pattern ID in pattern line `" + dataline + "'; version " + version);
                }

                int patId = -1;
                try
                {
                    String s = StringExtension.Substring(dataline, pat_id_begin, pat_id_end);   // dataline.Substring(pat_id_begin, pat_id_end);

                    patId = Integer.ParseInt(s);

                    // そのパターンIDに対応するSCSerikoSeqPatternEntryにデータを格納する。
                    setPatternEntry(version, patId, dataline.Substring(pat_id_end + 1));
                }
                catch (Exception e)
                {
                    // throw new RuntimeException(
                    //    "Parse failed: pattern line `" + dataline + "'; version " + version, e);
                }
            }
            else if (dataline.StartsWith("option"))
            {
                option = new SCSerikoSeqOptionEntry(dataline.Substring(7));
            }
        }

        public int id()
        {
            return seq_id;
        }

        public SCSerikoSeqIntervalEntry Interval()
        {
            return interval;
        }

        public SCSerikoSeqOptionEntry Option()
        {
            return option; // 省略されていたらnullが返ります。
        }

        public List<SCSerikoSeqPatternEntry> Patterns()
        {
            // 全てのpatternエントリを返します。
            // 配列のインディックスとパターンインディックスは一致しているので、
            // 番号が抜けていた場合はnullでパディングされています。
            return patterns;
        }

        private void setPatternEntry(int version, int patId, String pattern_param)
        {
            // patternsのpatIdに対応する位置にデータをセットします。
            // 必要ならnullでパディングします。

            if (patId < 0)
            {
                //  throw new RuntimeException(
                //  "Pattern ID is negative: `" + patId + "'; This must be zero or positive.");
            }

            SCSerikoSeqPatternEntry patEntry = new SCSerikoSeqPatternEntry(version, pattern_param);
            if (patterns.Count >= patId + 1)
            { // 既にデータが入っているので、それを押しのける。
                patterns[patId] = patEntry;
            }
            else
            {
                // 必要ならnullでパディングする。
                while (patId > patterns.Count)
                {
                    patterns.Add(null);
                }

                patterns.Add(patEntry);
            }
        }
    }
}
