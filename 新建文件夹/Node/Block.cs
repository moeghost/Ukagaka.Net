using aya.Eval;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
namespace aya.Node
{
    public class Block
    {
        public const int OptNone = 0;
        public const int OptNonOverlap = 1;
        public const int OptSequential = 2;

        private Aya aya;
        private int option;
        private List<Statement> statements; // [List<Value>, ...]

        private string lastGroupPattern; // 最後に評価した時のパターン

        private HashSet<string> nonOverlapTable; // {経路}
        // 経路: 2番目、4番目、0番目の順に選んだとき、経路は"2-4-0"
        // 空の経路番号は#で表す。

        private LinkedList<string> sequentialList; // [経路]

        public Block(Aya aya)
        {
            this.aya = aya;
            this.option = 0;
            this.statements = new List<Statement>();
            this.lastGroupPattern = null;
            this.nonOverlapTable = null;
            this.sequentialList = null;
        }

        public Block SetOption(string o)
        {
            if (o != null)
            {
                if (o.Equals("nonoverlap"))
                {
                    option = OptNonOverlap;

                    lastGroupPattern = "";
                    nonOverlapTable = new HashSet<string>();
                    sequentialList = null;
                }
                else if (o.Equals("sequential"))
                {
                    option = OptSequential;

                    lastGroupPattern = "";
                    nonOverlapTable = null;
                    sequentialList = new LinkedList<string>();
                }
                else
                {
                    try
                    {
                        throw new Exception("Option of block \"" + o + "\" is not supported.");
                    }
                    catch (Exception e) { }
                }
            }
            return this;
        }

        public int GetOption()
        {
            return option;
        }

        public List<Statement> GetStatements()
        {
            return statements;
        }

        public Block Add(Statement s)
        {
            statements.Add(s);
            return this;
        }

        /*
            ブロックを評価するには四つの方法があります。
            すなわち、通常評価、nonoverlap評価、sequential評価、switch評価です。
        */

        public Value Eval(Namespace ns)
        {
            // evalはnullを返す場合もある。
            return Eval(ns, -1);
        }

        public Value Eval(Namespace ns, int switchIndex)
        {
            ArrayList resultGroups = new ArrayList(); // [List<Value>, ...]

            ArrayList currentGroup = new ArrayList(); // [Value, ...]
            resultGroups.Add(currentGroup);

            for (int i = 0; i < statements.Count; i++)
            {
               Statement statement = statements[i];

                if (statement.IsSeparator())
                {
                    // 出力確定子。
                    // このグループ終了。次のグループへ。
                    currentGroup = new ArrayList();
                    resultGroups.Add(currentGroup);
                }
                else if (statement.IsBreak())
                {
                    throw statement.NewBreakOccurrence();
                }
                else if (statement.IsContinue())
                {
                    throw statement.NewContinueOccurrence();
                }
                else if (statement.IsReturn())
                {
                    // ここで終了。
                    return ConstructResult(resultGroups, switchIndex);
                }
                else
                {
                    Object result = statement.Eval(ns);
                    if (result != null)
                    {
                        if (result is Value)
                        {
                            currentGroup.Add(result);
                        }
                        else if (result is Vector)
                        {
                            currentGroup.Add((Vector)result);
                        }
                        else
                        {
                            throw new Exception("Internal Error: Statement#eval() returned object of " + result.GetType().Name + " illegally.");
                        }
                    }
                }
            }

            return ConstructResult(resultGroups, switchIndex);
        }

        protected Value ConstructResult(ArrayList resultGroups, int switchIndex)
        {
            if (option == OptNone)
            {
                Value result = null;

                for (int i = 0; i < resultGroups.Count; i++)
                {
                    ArrayList group = (ArrayList)resultGroups[i];
                    Value valOfGroup = null;

                    if (switchIndex >= 0)
                    {
                        if (switchIndex < group.Count)
                        {
                            valOfGroup = (Value)group[switchIndex];
                        }
                        else
                        {
                            valOfGroup = new Value("");
                        }
                    }
                    else
                    {
                        if (group.Count > 0)
                        {
                            int idx = (int)(new Random().NextDouble() * int.MaxValue) % group.Count;
                            valOfGroup = (Value)group[idx];
                        }
                    }

                    if (valOfGroup != null)
                    {
                        if (result == null)
                        {
                            result = valOfGroup;
                        }
                        else
                        {
                            result.SetString(result.GetString() + valOfGroup.GetString());
                        }
                    }
                }

                return result;
            }
            else
            {
                if (option == OptNonOverlap)
                {
                    // 前回の評価時のパターンと今回のパターンが変わっているか？
                    string pat = GenerateGroupPattern(resultGroups);
                    if (!lastGroupPattern.Equals(pat))
                    {
                        // 変わっているので、重複回避テーブルを作り直す。
                        ReconstructNonOverlapTable(resultGroups);
                        lastGroupPattern = pat;
                    }

                    // resultGroupsが空でないにも関わらずnonoverlap_tableが空になっていたら、
                    // やはり重複回避テーブルを作り直す。
                    if (resultGroups.Count > 0 && nonOverlapTable.Count == 0)
                    {
                        ReconstructNonOverlapTable(resultGroups);
                    }

                    // まだ使われていない経路を一つ選ぶ。
                    string[] paths = new string[nonOverlapTable.Count];
                    nonOverlapTable.CopyTo(paths);
                    int nth = (int)(new Random().NextDouble() * int.MaxValue) % paths.Length;

                    // 今回選んだ経路は削除。
                    nonOverlapTable.Remove(paths[nth]);

                    return SelectWithPath(resultGroups, paths[nth]);
                }
                else if (option == OptSequential)
                {
                    // 前回の評価時のパターンと今回のパターンが変わっているか？
                    string pat = GenerateGroupPattern(resultGroups);
                    if (!lastGroupPattern.Equals(pat))
                    {
                        // 変わっているので、順序リストを作り直す。
                        ReconstructSequentialList(resultGroups);
                        lastGroupPattern = pat;
                    }

                    // resultGroupsが空でないにも関わらずsequential_listが空になっていたら、
                    // やはり順序リストを作り直す。
                    if (resultGroups.Count > 0 && sequentialList.Count == 0)
                    {
                        ReconstructSequentialList(resultGroups);
                    }

                    // 先頭の経路を選んで、それを削除。
                    string path = sequentialList.First.Value;
                    sequentialList.RemoveFirst();

                    return SelectWithPath(resultGroups, path);

                }
                else
                {
                    throw new Exception("Block option '" + option + "' is not supported.");
                }
            }
        }

        protected static string GenerateGroupPattern(ArrayList resultGroups)
        {
            /*
                例えば次のような結果になった場合、この関数は
                "2/10/3"という文字列を返します。

                {
                    "foo"
                    "bar"
                    --
                    1
                    2
                    ..
                    10
                    --
                    "aaa"
                    "bbb"
                    "ccc"
                }
            */
            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < resultGroups.Count; i++)
            {
                ArrayList group = (ArrayList)resultGroups[i];

                if (i != 0)
                {
                    buf.Append('/');
                }

                buf.Append(group.Count);
            }
            return buf.ToString();
        }

        protected void ReconstructNonOverlapTable(ArrayList resultGroups)
        {
            nonOverlapTable.Clear();

            // 可能な全ての場合の経路を作り、HashSetに入れる。
            if (resultGroups.Count > 0)
            {
                List<string> paths = MakeNonOverlapPath(resultGroups, 0);
                foreach (string path in paths)
                {
                    nonOverlapTable.Add(path);
                }
            }
        }

        protected void ReconstructSequentialList(ArrayList resultGroups)
        {
            sequentialList.Clear();

            // 可能な全ての場合の経路を作り、LinkedListに繋げる。
            if (resultGroups.Count > 0)
            {
                List<string> paths = MakeNonOverlapPath(resultGroups, 0);
                foreach (string path in paths)
                {
                    sequentialList.AddLast(path);
                }
            }
        }

        protected List<string> MakeNonOverlapPath(ArrayList resultGroups, int level)
        {
            List<string> result = new List<string>();
            ArrayList current = (ArrayList)resultGroups[level];

            // それより下のレベルがあるか？
            if (resultGroups.Count > level + 1)
            {
                // あるので、再帰的に経路を作る。
                List<string> lower = MakeNonOverlapPath(resultGroups, level + 1);

                // 作られた全ての経路に対して、今回のグループの要素一つ一つに対応する経路を新たに作る。
                if (current.Count > 0)
                {
                    for (int i = 0; i < lower.Count; i++)
                    {
                        for (int j = 0; j < current.Count; j++)
                        {
                            result.Add(j + "-" + lower[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < lower.Count; i++)
                    {
                        result.Add("#-" + lower[i]);
                    }
                }
            }
            else
            {
                // 無いので、今回のグループの要素一つ一つを経路とする。
                if (current.Count > 0)
                {
                    for (int i = 0; i < current.Count; i++)
                    {
                        result.Add(i.ToString());
                    }
                }
                else
                {
                    result.Add("#");
                }
            }

            return result;
        }

        protected Value SelectWithPath(ArrayList resultGroups, string path)
        {
            // pathをパース
            Value result = null;

            string[] indices = path.Split('-');
            for (int i = 0; i < indices.Length; i++)
            {
                ArrayList current = (ArrayList)resultGroups[i];
                string index = indices[i];

                if (current.Count == 1 && current[0].Equals("#"))
                {
                    // このブロックは空。次へ。
                }
                else
                {
                    try
                    {
                        Value val = (Value)current[int.Parse(index)];
                        if (result == null)
                        {
                            result = val;
                        }
                        else
                        {
                            result.SetString(result.GetString() + val.GetString());
                        }
                    }
                    catch (Exception e) { }
                }
            }

            return result;
        }
    }
}
