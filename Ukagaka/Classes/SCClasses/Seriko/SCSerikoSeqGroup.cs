using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
namespace Ukagaka
{
    public class SCSerikoSeqGroup
    {
        List<SCSerikoSequence> sequences; // 中身はSCSerikoSequence
        int seqGroupId;

        public SCSerikoSeqGroup(FileInfo f, int seqGroupId)
        {
            // f : *a.txt
            sequences = new List<SCSerikoSequence>();
            this.seqGroupId = seqGroupId;
            if (!f.Exists || f.Attributes == FileAttributes.Directory)
            {
                return; // ありえないけど。
            }
            try
            {
                StreamReader sr = null;
                sr = new StreamReader(new FileStream(f.FullName,FileMode.Open),Encoding.UTF8);

                List<String> definitions = new List<String>();
                while (true)
                {
                    String line = sr.ReadLine();

                    if (line == null)
                    {
                        break; // EOF
                    }
                    if (line.Length == 0)
                    {
                        continue;
                    }
                    if (!Char.IsDigit(StringExtension.CharAt(line, 0)))
                    {
                        continue; // 数字で始まらない行はスキップ。
                    }
                    definitions.Add(line);
                }
                Load(0, definitions); // *a.txtから讀んだ場合は常に舊SERIKO。

                sr.Close();
            }
            catch (Exception e)
            {
              //  throw new RuntimeException(
              //  "Load failed: " + f.GetName(), e);
            }
        }

        public void Dump()
        {
            System.Console.WriteLine("*************");
            System.Console.WriteLine("SequenceGroup ID : " + seqGroupId);
            int n_sequences = sequences.Count;
            for (int i = 0; i < n_sequences; i++)
            {
                SCSerikoSequence seq = (SCSerikoSequence)sequences.ElementAt(i);
                seq.dump();
            }
            System.Console.WriteLine("");
        }

        public SCSerikoSeqGroup(int version, List<String> definitions, int seqGroupId)
        {
            // definitions : <String>シーケンス定義行の集合
            // ただしシーケンス定義でない項目が含まれていたら正しく無視する。
            sequences = new List<SCSerikoSequence>();
            this.seqGroupId = seqGroupId;

            List<String> valid_defs = new List<String>();
            int n_defs = definitions.Count;
            for (int i = 0; i < n_defs; i++)
            {
                String def = (String)definitions.ElementAt(i);

                if (version == 0)
                {
                    // 数値で始まっていればシーケンス定義行と見做す。
                    char c = StringExtension.CharAt(def, 0);
                    if (Char.IsDigit(c))
                    {
                        valid_defs.Add(def);
                    }
                }
                else
                {
                    // animationで始まつてゐればシーケンス定義行と見做す。
                    if (def.StartsWith("animation"))
                    {
                        valid_defs.Add(def);
                    }
                }
            }

            Load(version, valid_defs);
        }

        static String pat_seqid = "^animation(\\d+)\\.";
         protected void Load(int version, List<String> definitions)
        {
            // definitions : <String>シーケンス定義行の集合
            int n_lines = definitions.Count;
            for (int i = 0; i < n_lines; i++)
            {
                String line = (String)definitions.ElementAt(i);

                if (version == 0)
                {
                    // シーケンスIDを取り出す。
                    // 先頭から数字が続くかぎり、それはシーケンスIDである。
                    int seq_id_length = 0;
                    while (Char.IsDigit(StringExtension.CharAt(line, seq_id_length)))
                    {
                        seq_id_length++;
                    }
                    int seqId = -1;
                    try
                    {
                        seqId = Integer.ParseInt(line.Substring(0, seq_id_length));
                    }
                    catch (Exception e)
                    {
                    }
                    // シーケンスIDに対応したSCSerikoSequenceに格納する。
                    SCSerikoSequence seq = FindSequence(seqId, true); // 初めて見るシーケンスIDなら新たに作成する。
                    seq.add(version, line.Substring(seq_id_length));
                }
                else
                {
                    // シーケンスIDを取出す。
                     Match match = Regex.Match(line, pat_seqid);
                   // MatchCollection match = Regex.Matches(line, pat_seqid);
                    // Matcher matcher = pat_seqid.matcher(line);
                    if (match.Success)
                    {
                        try
                        {
                            int seqId = Integer.ParseInt(match.Groups[1].Value);

                            // シーケンスIDに対応したSCSerikoSequenceに格納する。
                            SCSerikoSequence seq = FindSequence(seqId, true); // 初めて見るシーケンスIDなら新たに作成する。
                            seq.add(version, line.Substring(match.Groups.Count));
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine("Warning: invalid sequence [" + line + "]");
                            System.Console.WriteLine(e.StackTrace);
                        }
                    }
                }
            }
        }

        public int GroupId()
        {
            return seqGroupId;
        }

        public SCSerikoSequence FindSequence(int seqid)
        {
            // シーケンスIDを元にSCSerikoSequenceを返します。
            // 見つからなければnullを返します。
            return FindSequence(seqid, false);
        }

        private SCSerikoSequence FindSequence(int seqid, bool makenew)
        {
            // シーケンスIDを元にSCSerikoSequenceを検索し、
            // もし見つからなかったらmakenewがtrueなら新たに作成してリストに追加してから返し、
            // falseならnullを返します。
            int n_sequences = sequences.Count;
            for (int i = 0; i < n_sequences; i++)
            {
                SCSerikoSequence seq = (SCSerikoSequence)sequences.ElementAt(i);
                if (seq.id() == seqid)
                {
                    return seq;
                }
            }

            // Not Found
            if (makenew)
            {
                SCSerikoSequence new_seq = new SCSerikoSequence(seqid);
                sequences.Add(new_seq);
                return new_seq;
            }
            else
            {
                return null;
            }
        }

        public List<SCSerikoSequence> MakeSequenceArray(String interval)
        {
            // 指定されたintervalタイプを持つ全てのシーケンス（SCSerikoSequenceクラス）を順番は考慮せずに並べたVectorを作成して返します。
            // runonceやbindで使います。
            List<SCSerikoSequence> result = new List<SCSerikoSequence>();

            int n_sequences = sequences.Count;
            for (int i = 0; i < n_sequences; i++)
            {
                SCSerikoSequence seq = (SCSerikoSequence)sequences.ElementAt(i);
                try
                {
                    if (seq.Interval().type().Equals(interval))
                    {
                        result.Add(seq);
                    }
                }
                catch (Exception e)
                {
                    // 何か變な事が起きた。
                    System.Console.WriteLine(e.StackTrace);
                    //e.printStackTrace();
                }
            }

            return result;
        }

    }
}
