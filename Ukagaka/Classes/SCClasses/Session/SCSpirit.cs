using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.IO;
using System.Collections;
using Utils;
using System.IO;
namespace Ukagaka
{
    public class SCSpirit
    {
        public static int STAT_OK = 0; // アクセス可能
        public static int STAT_PREPARING = 1; // 準備中

        protected volatile int spirit_status = STAT_PREPARING;
        SCSession session;

        SCDescription descManager;
        List<SCMakotoAccessor> makoto_mods;
        SCShioriAccessor shiori;
        int shiori_main_version; // 2 or 3
        int shiori_sub_version; // 常に0

        String dirname;
        Utils.File spiritRootDir;

        String name;
        String spirit_identification;
        String selfname;
        String keroname;

        public SCSpirit(SCSession session, String dirname)
        {
            this.session = session;
            this.dirname = dirname;

            _Construct(session.GetGhostDir(), dirname, null, session.IsLightMode());
        }

        public SCSpirit(SCSession session, String dirname, SCStatusWindowController status)
        {
            this.session = session;
            this.dirname = dirname;

            _Construct(session.GetGhostDir(), dirname, status, session.IsLightMode());
        }

        public SCSpirit(Utils.File ghost_root, String dirname, bool light_mode)
        {
            this.session = null;
            this.dirname = dirname;

            _Construct(ghost_root, dirname, null, light_mode);
        }
        /*
        public String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("session: {").Append(session).Append('}');
            buf.Append("; shiori: {").Append(shiori).Append('}');
            buf.Append("; shiori protocol version: ").Append(shiori_main_version).
                Append('.').Append(shiori_sub_version);
            if (makoto_mods.Length> 0)
            {
                buf.Append("; makoto: [");
                for (Iterator ite = makoto_mods.iterator(); ite.hasNext();)
                {
                    buf.Append('{').Append(ite.next()).Append('}');
                    if (ite.hasNext())
                    {
                        buf.Append(", ");
                    }
                }
                buf.Append(']');
            }
            return buf.ToString();
        }
        */
        protected void Ize()
        {
            //  Logger.log(this, Logger.DEBUG, "ized");
        }

        public void ReloadAll()
        {
            // SHIORIとMAKOTOをリロードします。
            // みだりに使うべからず。
            spirit_status = STAT_PREPARING;

            SCSession session_backup = session;
            Close();
            session = session_backup;
            _Construct(session.GetGhostDir(), dirname, null, session.IsLightMode());
        }

        protected void _Construct(Utils.File ghost_root, String dirname, SCStatusWindowController status, bool light_mode)
        {
            spiritRootDir = new Utils.File(ghost_root, "ghost/" + dirname);
            if (!(spiritRootDir.IsDirectory()))
            {
                return;
            }
            descManager = new SCDescription(new Utils.File(spiritRootDir, "descript.txt"));
            //descManager.setTag(ghost_root.ToString());

            name = descManager.GetStrValue("name");
            selfname = descManager.GetStrValue("sakura.name");
            keroname = descManager.GetStrValue("kero.name");
            if (selfname == null) selfname = "";
            if (keroname == null) keroname = "";
            spirit_identification = selfname + ',' + keroname;

            PrintStat(status, "scope #0: " + (selfname.Length == 0 ? "none" : "%[color,white]" + selfname + "%[color,default]") + "\n");
            PrintStat(status, "scope #1: " + (keroname.Length == 0 ? "none" : "%[color,white]" + keroname + "%[color,default]") + "\n");

            // SHIORIモジュール起動
            if (light_mode)
            {
                // ライトモードではSHIORIをロードしない。
                shiori = null;
            }
            else
            {
                PrintStat(status, "The spirit of the ghost,\n");
                PrintStat(status, "%[color,orange]loading SHIORI subsystem... ");
                long time_shiori = SystemTimer.GetTimeTickCount();
                shiori = SCShioriLoader.LoadShioriModule(spiritRootDir);

                // SHIORI/2.xかSHIORI/3.xかをGET Versionで判別。
                spirit_status = STAT_OK;
                SCShioriSessionResponce getversion_resp = DoShioriSession(
                "GET Version SHIORI/2.0\r\nSender: " +
                SCFoundation.STRING_FOR_SENDER +
                "\r\nSecurityLevel: local\r\n\r\n");
                spirit_status = STAT_PREPARING;

                if (getversion_resp != null && getversion_resp.GetHeader() != null && getversion_resp.GetHeader().StartsWith("SHIORI/3."))
                {
                    shiori_main_version = 3;
                }
                else
                {
                    shiori_main_version = 2;
                }
                shiori_sub_version = 0;

                var elapsedMs = (DateTime.Now.Ticks - time_shiori) / TimeSpan.TicksPerMillisecond;
                PrintStat(status, $"%[color,white]done.({elapsedMs}ms)%[color,default]\n");
            }

            // MAKOTOモジュール起動
            if (SCMakotoLoader.GetModuleNames(spiritRootDir).Count == 0 ||
                light_mode)
            {
                // ライトモードではMAKOTOをロードしない。
                //printstat(status, "no MAKOTO subsystems will be loaded.\n");
                makoto_mods = new List<SCMakotoAccessor>();
            }
            else
            {
                // printstat(status, "%[color,orange]loading MAKOTO subsystems... ");
                var timeMakoto = DateTime.Now.Ticks;
                makoto_mods = SCMakotoLoader.LoadMakotoModules(spiritRootDir);
                var elapsedMs = (DateTime.Now.Ticks - timeMakoto) / TimeSpan.TicksPerMillisecond;
                PrintStat(status, $"%[color,white]done.({elapsedMs}ms)%[color,default]\n");
            }

            spirit_status = STAT_OK;


            PrintStat(status, "This SHIORI subsystem uses protocol SHIORI/" + shiori_main_version + ".0.\n");
        }
        protected void PrintStat(SCStatusWindowController status, String msg)
        {
            // if (status != null) status.texttype_print(msg);
        }

        public int GetShioriProtocolVersion()
        {
            return shiori_main_version;
        }

        private void ShioriSessionLog(String data)
        {
            if (!SCFoundation.LOG_SHIORI_SESSION) return;

            using (var reader = new StringReader(data))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }
        public SCShioriSessionResponce DoShioriSession(String request)
        {
            // このメソッドはSpiritが準備中の時は準備が完了するまで呼び出し元をブロックしますのでご注意下さい。

            if (shiori == null)
            {
                // shioriがロードされていなければ、常にSHIORI/2.0 500 Internal Server Errorを返す。
                return new SCShioriSessionResponce("SHIORI/2.0 500 Internal Server Error", new Hashtable());
            }

            if (spirit_status == STAT_PREPARING)
            {
                while (spirit_status != STAT_OK)
                {
                    try
                    {
                        //  Thread.currentThread().Sleep(300);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
            }
            ShioriSessionLog(request);
            var response = shiori.Request(request);

            if (response == null || response == "")
            {
                return null;
            }

            using (var reader = new StringReader(response))
            {
                // Get response header
                var header = reader.ReadLine();
                ShioriSessionLog(header);

                // Read entire response
                var headers = new Hashtable();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ShioriSessionLog(line);

                    var colonIndex = line.IndexOf(':');
                    if (colonIndex == -1) continue;

                    var label = line.Substring(0, colonIndex);
                    var data = line.Substring(colonIndex + 2); // Skip space after colon

                    headers[label] = data;
                }

                return new SCShioriSessionResponce(header, headers);
            }
            
        }

        public String GetStringFromShiori(String id)
        {
            // Issue GET String to SHIORI
            // Returns null on failure
            SCShioriSessionResponce resp;
            if (GetShioriProtocolVersion() == 3)
            {
                resp = DoShioriSession(
                    $"GET SHIORI/3.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\n" +
                    $"ID: {id}\r\nSecurityLevel: local\r\n\r\n");
            }
            else
            {
                resp = DoShioriSession(
                    $"GET String SHIORI/2.5\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\n" +
                    $"ID: {id}\r\nSecurityLevel: local\r\n\r\n");
            }

            if (resp == null || !resp.GetHeader().Contains("200 OK"))
                return null;

            return GetShioriProtocolVersion() == 3
                ? resp.GetResponce()["Value"].ToString()
                : resp.GetResponce()["String"].ToString();
        }

        public String TranslateWithShiori(String src)
        {
            if (string.IsNullOrEmpty(src))
                return src;

            SCShioriSessionResponce resp;
            if (shiori_main_version == 3)
            {
                resp = DoShioriSession(
                    $"GET SHIORI/3.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\n" +
                    $"ID: OnTranslate\r\nReference0: {src}\r\nSecurityLevel: local\r\n\r\n");
            }
            else
            {
                resp = DoShioriSession(
                    $"TRANSLATE Sentence SHIORI/2.6\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\n" +
                    $"Sentence: {src}\r\nSecurityLevel: local\r\n\r\n");
            }

            if (resp == null || !resp.GetHeader().Contains("200 OK"))
                return src;

            return shiori_main_version == 3
                ? resp.GetResponce()["Value"].ToString()
                : resp.GetResponce()["Sentence"].ToString();
        }

        public String TranslateWithMakoto(String src)
        {
            String current_str = src;

             int n_makoto = makoto_mods.Count;
            for (int i = 0; i < n_makoto; i++)
            {
                 current_str = TranslateWithMakoto(current_str, (SCMakotoAccessor)makoto_mods.ElementAt(i));
            }

            return current_str;
        }

        protected void MakotoSessionLog(String data)
        {
            if (!SCFoundation.LOG_MAKOTO_SESSION) return;

            using (var reader = new StringReader(data))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }
        protected string TranslateWithMakoto(string src, SCMakotoAccessor makoto)
        {
            var request = $"EXECUTE MAKOTO/2.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\nString: {src}\r\n";
            MakotoSessionLog(request);

            var response = makoto.Request(request);
            using (var reader = new StringReader(response))
            {
                // Get response header
                var header = reader.ReadLine();
                MakotoSessionLog(header);

                // Read entire response
                var headers = new Dictionary<string, string>();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    MakotoSessionLog(line);

                    var colonIndex = line.IndexOf(':');
                    if (colonIndex == -1) continue;

                    var label = line.Substring(0, colonIndex);
                    var data = line.Substring(colonIndex + 2); // Skip space after colon

                    headers[label] = data;
                }

                // Return src if not 200 OK, otherwise return String: content
                return header.Contains("200 OK")
                    ? headers.TryGetValue("String", out var str) ? str:null
                    : src;
            }
        }
        public void Close()
        {
            session = null;
            descManager = null;

            foreach (var makoto in makoto_mods)
            {
                makoto.Terminate();
            }
            makoto_mods.Clear();

            shiori?.Terminate();
            shiori = null;
        }

        public SCDescription GetDescManager()
        {
            return descManager;
        }

        public SCShioriAccessor GetShiori()
        {
            return shiori;
        }

        public List<SCMakotoAccessor> GetMakotoModules()
        {
            return makoto_mods;
        }

        public String GetName()
        {
            return name;
        }

        public String GetIdentification()
        {
            return spirit_identification;
        }

        public String GetSelfName()
        {
            return selfname;
        }

        public String GetKeroName()
        {
            return keroname;
        }

        public Utils.File GetRootDir()
        {
            return spiritRootDir;
        }


    }
}
