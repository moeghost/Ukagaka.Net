using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cocoa.AppKit;
namespace Ukagaka
{
    /*
    ハケモテヨミ、ホbind．ー．□`．ラメサ、ト、ヌ、ケ。」
    ．ミ．、．□ノ．サ．□ソゥ`、ホウヨ、トナ菽ミトレ、ヒオヌ乕、オ、□゛、ケ。」
*/
    public class SCSerikoEnabledBindGroup
    {
        private SCSession session;
        private SCShell shell;
        private SCSeriko seriko;

        private int seqid; // バインドグループのシーケンスID。
        private bool hontai; // 本体か否か。
        private SCShellWindowController target;
        private List<SCShellLayer> layers; // 中身はSCShellLayer。

        public SCSerikoEnabledBindGroup(SCSession session, SCShell shell, SCSeriko seriko, int seqid, bool hontai)
        {
            this.session = session;
            this.shell = shell;
            this.seriko = seriko;

            this.seqid = seqid;
            this.hontai = hontai;
            target = (hontai ? session.GetHontai() : session.GetUnyuu());

            layers = new List<SCShellLayer>();

            SCSerikoSeqGroup group = seriko.GetCurrentSeqGroup(hontai);
            if (group == null) return;
            SCSerikoSequence seq = group.FindSequence(seqid);
            if (seq == null) return;

            RunSequence(seq);
        }

        private void RunSequence(SCSerikoSequence seq)
        {
            List<SCSerikoSeqPatternEntry> patterns = seq.Patterns();
            int n_patterns = patterns.Count;
            for (int i = 0; i < n_patterns; i++)
            {
                Object obj = patterns.ElementAt(i);
                if (obj == null)
                {
                    continue; // パディングされてた
                }
                SCSerikoSeqPatternEntry pat = (SCSerikoSeqPatternEntry)obj;

                // メソッド判別
                String method = pat.Method();
                if (method.Equals("bind") || method.Equals("add"))
                { // どちらも同じ動作
                    SCShellLayer layer = target.GetView().AddLayer(
                        shell.GetSurfaceServer(),
                        pat.Surfaceid(),
                        seqid,
                        new NSPoint(pat.Offsetx(), pat.Offsety()),
                        true);
                    layers.Add(layer);
                }
            }
            //target.getWindow().display();
            target.SetViewsNeedDisplay(true);
        }

        public void Disable()
        {
            // グループが消される時に呼ばれます。
            // 一度disableしたオブジェクトは二度と使えないので、もう一度使いたければ新たなインスタンスを作成して下さい。
            int n_layers = layers.Count;
            for (int i = 0; i < n_layers; i++)
            {
                SCShellLayer layer = (SCShellLayer)layers.ElementAt(i);
                target.GetView().RemoveLayer(layer);
            }
            //target.getWindow().display();
            target.SetViewsNeedDisplay(true);

            session = null;
            shell = null;
            seriko = null;
            target = null;
            layers.Clear();
        }

        public int SeqId()
        {
            return seqid;
        }

        public bool IsHontai()
        {
            return hontai;
        }
        public bool IsSakura()
        {
            return hontai;
        }

    }
}
