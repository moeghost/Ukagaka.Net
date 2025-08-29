using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.IO;
using System.Collections;
using Utils;
namespace Ukagaka
{
    public class SCShell
    {

        SCSession session;

        SCDescription descManager;
        SCAliasManager aliasFileNameTable;
        SCAliasNameTable aliasNameTable;
        SCSurfaceServer surfserver;
        SCSeriko seriko;
        SCBlockedDescription surfaceDescriptions; // surfaces.txtが存在しなければnullになっている。

        String dirname;
        String shellname;
        File shellRootDir;
        File readmeFile;

        String selfname;
        String keroname;

        public SCShell(SCSession session, String dirname):this(session, dirname, null)
        {
          //  this(session, dirname, null);
        }

        public SCShell(SCSession session, String dirname, SCStatusWindowController status)
        {
            this.session = session;
            this.dirname = dirname;

            shellRootDir = new File(session.GetGhostDir().GetPath(),"shell/" + dirname);
            descManager = new SCDescription(new File(shellRootDir.GetPath(), "descript.txt"));
            //descManager.setTag(session.ToString());
            shellname = descManager.GetStrValue("name");
            Printstat(status, "The shell of the ghost,\n");
            Printstat(status, "shell name: %[color,yellow]" + shellname + "%[color,default]\n");

            File surfaces_txt = new File(shellRootDir.GetPath(),"surfaces.txt");
            if (surfaces_txt.Exists())
            {
                surfaceDescriptions = new SCBlockedDescription(surfaces_txt);
            }
            else
            {
                surfaceDescriptions = null;
            }

            selfname = descManager.GetStrValue("sakura.name");
            keroname = descManager.GetStrValue("kero.name");
            File aliasTxt = new File(shellRootDir.GetPath(), "alias.txt");
            aliasFileNameTable = new SCAliasManager(aliasTxt);
            aliasNameTable = new SCAliasNameTable();
            aliasNameTable.load(aliasTxt); // alias.txtを読ませてから
            aliasNameTable.load(surfaces_txt); // surfaces.txtも読ませる。
            readmeFile = new File(shellRootDir,(descManager.Exists("readme") ? descManager.GetStrValue("readme") : "readme.txt"));

            // サーフィスサーバー起動
            Printstat(status, "%[color,orange]loading shell... ");
            long start_time = SystemTimer.GetTimeTickCount();
            surfserver = new SCSurfaceServer(this, shellRootDir);
            Printstat(status, "%[color,white]done.(" + (SystemTimer.GetTimeTickCount() - start_time) + "ms)%[color,default]\n");
            if (!surfserver.hasLoadAllSurfaces())
            {
                Printstat(status, "Actual surfaces will be loaded when required.\n");
            }
        }
        protected void Printstat(SCStatusWindowController status, String msg)
        {
           // if (status != null) status.texttype_print(msg);
        }

        public void Start()
        {
            // phase9.10以降、起動時にシェルを表示しない。

            // SERIKO起動
            seriko = new SCSeriko(session, this, shellRootDir);

            session.ResetSurfaces();
        }

        public void Close()
        {
             
            session = null;
            descManager = null;
            aliasFileNameTable = null;
            aliasNameTable = null;
            surfserver = null;
            seriko.Terminate();
            
            seriko = null;
            surfaceDescriptions = null;
        }

        public SCDescription GetDescManager()
        {
            return descManager;
        }

        public SCAliasManager GetAliasManager()
        {
            return aliasFileNameTable;
        }

        public SCAliasNameTable GetAliasNameTable()
        {
            return aliasNameTable;
        }

        public SCBlockedDescription GetSurfaceDescriptions()
        {
            return surfaceDescriptions;
        }

        public SCSurfaceServer GetSurfaceServer()
        {
            return surfserver;
        }
        
        public SCSeriko GetSeriko()
        {
            return seriko;
       }
      
        public String GetDirName()
        {
            return dirname;
        }

        public File GetRootDir()
        {
            return shellRootDir;
        }

        public File GetReadmeFile()
        {
            return readmeFile;
        }

        public String GetShellName()
        {
            String nameFromDesc = descManager.GetStrValue("name");
            return (nameFromDesc == null ? dirname : nameFromDesc);
        }

        public String GetSelfName()
        {
            // 指定されていなければnullを返します。
            return selfname;
        }

        public String GetKeroName()
        {
            // 指定されていなければnullを返します。
            return keroname;
        }

        String id;
        public String GetIdentification()
        {
            // sakura, kero共にnullであればnullを返す。
            if (selfname == null && keroname == null)
            {
                return null;
            }

            if (id == null)
            {
                StringBuilder buf = new StringBuilder(30);
                if (selfname != null)
                {
                    buf.Append(selfname);
                }
                buf.Append(',');
                if (keroname != null)
                {
                    buf.Append(keroname);
                }
                id = buf.ToString();
            }
            return id;
        }

        public String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("session: {").Append(session).Append('}');
            buf.Append("; dirname: ").Append(dirname);
           // buf.Append("; seriko: {").Append(seriko).Append('}');
            return buf.ToString();
        }

        protected void Finalize()
        {
            //Logger.log(this, Logger.DEBUG, "finalized");
        }
 
    }
}
