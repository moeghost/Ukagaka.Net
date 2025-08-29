using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
namespace Ukagaka
{
    public class SCSerikoLibrary
    {

        List<SCSerikoSeqGroup> seqGroups; // 中身はSCSerikoSeqGroup

        public SCSerikoLibrary(SCShell shell)
        {
            seqGroups = new List<SCSerikoSeqGroup>();

            if (shell.GetSurfaceDescriptions() != null)
            {
                // surfaces.txtが存在する。
                try
                {
                    LoadFromComprehensiveDefinitions(shell);
                }
                catch (Exception e)
                {
                    System.Console.Write("Couldn't load surfaces.txt: {" + shell + "}");
                    System.Console.Write(e.StackTrace);
                }
            }
            else
            {
                // surfaces.txtが存在しない。
                try
                {
                    LoadFromIndividualFile(shell);
                }
                catch (Exception e)
                {
                    System.Console.Write("Couldn't load indivial surface definitions: {" + shell + "}");
                    System.Console.Write(e.StackTrace);
                }
            }
        }

        protected void Finalize()
        {
            // Logger.log(this, Logger.DEBUG, "finalized");
        }

        public void Dump()
        {
            int n_groups = seqGroups.Count;
            for (int i = 0; i < n_groups; i++)
            {
                SCSerikoSeqGroup group = (SCSerikoSeqGroup)seqGroups.ElementAt(i);
                group.Dump();
            }
        }

        protected void LoadFromComprehensiveDefinitions(SCShell shell)
        {
            /*
              全てのエントリについて、次のような動作を行います。

              エントリ名をエイリアスデータベースから逆引きして、見付かったらその通りに読み込みます。
              データベースに無かったらエントリが命名規則に従っているかどうかを調べて、
              従っていたら読み込みます。
              そうでないエントリは無視します。
            */
            SCBlockedDescription compDef = shell.GetSurfaceDescriptions();

            // versionを見る
            int version = 0;
            if (compDef.isBlock("descript"))
            {


                String pat = "^version\\s*,\\s*(\\d+)$";
                Match match;

                Object element = compDef.get("descript");
                List<String> descript = new List<String>();
                if (element is List<String>)
                { 
                    descript = (List<String>)element;
                }

                foreach (String line in descript)
                {
                    match = Regex.Match(line, pat);

                    if (match.Success)
                    {
                        try
                        {
                            version = Integer.ParseInt(match.Groups[1].Value);
                        }
                        catch (Exception e)
                        {
                            System.Console.Write("Warning: invalid version line [" + line + "]");
                        }
                        break;
                    }
                }
            }

            if (version != 0 && version != 1)
            {
                System.Console.Write("Warning: unsupported SERIKO version [" + version + "]");
                return;
            }
            foreach (String key in compDef.keys())

            {
                //    String key = (String)keys.nextElement();

                String inv_result = shell.GetAliasManager().InverseSearch(key);
                if (inv_result != null)
                { // 見つかった
                  // 見つかったキーをパースする。
                  // surface[id]となっている筈。
                    int id = -1;
                    try
                    {
                        id = Integer.ParseInt(inv_result.Substring(7));
                    }
                    catch (Exception e)
                    {
                        // エラーが起きたら-1のまま。
                    }

                    if (id != -1)
                    { // 認識した。
                        Object element = compDef.get(key);
                        if (element is List<String>)
                        {
                            seqGroups.Add(new SCSerikoSeqGroup(version, (List<String>)element, id));

                        }
                    }
                }
                else
                { // 逆引きしても見つからなかった
                  // 命名規則に従っているか？
                    if (key.StartsWith("surface"))
                    {
                        int id = -1;
                        try
                        {
                            id = Integer.ParseInt(key.Substring(7));
                        }
                        catch (Exception e)
                        {
                        }

                        if (id != -1)
                        { // 認識した。
                            Object element = compDef.get(key);
                            if (element is List<String>) {
                                seqGroups.Add(new SCSerikoSeqGroup(version, (List<String>)element, id));
                            }
                        }
                    }
                }
            }
        }

        protected void LoadFromIndividualFile(SCShell shell)
        {
            /*
              全てのファイルについて、次のような動作を行います。

              ファイル名をエイリアスデータベースから逆引して、見つかったらその通りに読み込みます。
              データベースに無かったらファイルがSERIKOシーケンスデータの命名規則に従っているかどうかを調べて、
              従っていたら読み込みます。
              そうでないファイルは無視します。
            */
            String[] files = Directory.GetFileSystemEntries(shell.GetRootDir().GetPath());
                 
            for (int i = 0; i < files.Length; i++)
            {
                String fname = files[i];
                String lowercase_fname = files[i].ToLower(); // パース用に小文字に変えてしまうので、これをパスに使ってはいけません。
                if (!lowercase_fname.EndsWith("a.txt") && !lowercase_fname.EndsWith("a.dat"))
                {
                    continue;
                }
                String familyname; // ファイル名からa.txt又はa.datを除いた部分
                if (lowercase_fname.LastIndexOf("a.txt") != -1)
                {
                    familyname = fname.Substring(0, lowercase_fname.LastIndexOf("a.txt"));
                }
                else
                {
                    familyname = fname.Substring(0, lowercase_fname.LastIndexOf("a.dat"));
                }

                String inv_result = shell.GetAliasManager().InverseSearch(familyname);
                if (inv_result != null)
                { // 見つかった
                  // 見つかったキーをパースする。
                  // surface[id]となっている筈。
                    int id = -1;
                    try
                    {
                        id = Integer.ParseInt(inv_result.Substring(7));
                    }
                    catch (Exception e) { } // エラーが起きたら-1のまま。

                    if (id != -1)
                    { // 認識した。
                        seqGroups.Add(new SCSerikoSeqGroup(new FileInfo(files[i]), id));
                    }
                }
                else
                { // 逆引きしても見つからなかった
                  // 命名規則に従っているか？
                    if (lowercase_fname.StartsWith("surface"))
                    { // a.txtで終わることは分かっている。
                        int id = -1;
                        try
                        {
                            id = Integer.ParseInt(familyname.Substring(7));
                        }
                        catch (Exception e) { }

                        if (id != -1)
                        { // 認識した。
                            seqGroups.Add(new SCSerikoSeqGroup(new FileInfo(files[i]), id));
                        }
                    }
                }
            }
        }

        public SCSerikoSeqGroup FindSeqGroup(int id)
        {
            // 指定されたシーケンスグループIDで、シーケンスグループを検索します。
            // 見つからなければnullを返します。
            int n_groups = seqGroups.Count;
            for (int i = 0; i < n_groups; i++)
            {
                SCSerikoSeqGroup seqgroup = (SCSerikoSeqGroup)seqGroups.ElementAt(i);
                if (seqgroup.GroupId() == id)
                {
                    return seqgroup;
                }
            }

            return null;
        }

    }
}
