using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.IO;
using Utils;
namespace Ukagaka
{
     public class SCSeriko
    {
        
        private SCSerikoLibrary seriko_lib;
        private SCSerikoSeqGroup current_hontai_group, current_unyuu_group;
        private SCSerikoBindCenter bindCenter;
        private List<SCSerikoSequenceRunner> runners; // 中身はSCSerikoSequenceRunner。
        private bool exclusive_locked = false; // option[exclusive]のシーケンスが動作中の時にtrue。executeメソッドのコールが禁止されます。

        SCSession session;
        SCShell shell;

        public SCSeriko(SCSession session, SCShell shell, File shelldir)
        {
            this.session = session;
            this.shell = shell;

            seriko_lib = new SCSerikoLibrary(shell);
            runners = new List<SCSerikoSequenceRunner>();
            bindCenter = new SCSerikoBindCenter(session, shell, this);

            //dumpLibrary();
        }

        public void DumpLibrary()
        {
            seriko_lib.Dump();
        }

        public void Stop(bool sakura_scope)
        {
            // 一時停止。
            TerminateAllRunnersThread(sakura_scope);
        }

        public void Restart(bool sakura_scope)
        {
            // 再開。
            SCSerikoSeqGroup target =
                sakura_scope ? current_hontai_group : current_unyuu_group;
            if (target == null)
            {
                return;
            }

            Execute(target.MakeSequenceArray("runonce"), sakura_scope);
            Execute(target.MakeSequenceArray("always"), sakura_scope);
            Execute(target.MakeSequenceArray("random"), sakura_scope);
        }

        public void SurfaceChanged(int new_id, bool hontai)
        {
            // サーフィスが変えられた時に呼ばれます。
            // それまでのアニメーション動作をクリーンアップし、新たなシーケンスグループの常時起動系エントリを起動します。
            // サーフィスIDとシーケンスグループIDは、当然同じ物です。

            // クリーンアップ
            Stop(hontai);
            bindCenter.DisableAll();

            // カレントグループを変更
            if (hontai)
            {
                current_hontai_group = seriko_lib.FindSeqGroup(new_id);
            }
            else
            {
                current_unyuu_group = seriko_lib.FindSeqGroup(new_id);
            }

            Restart(hontai);
            bindCenter.EnableCheckedBindGroups();
        }

        public void Execute(List<SCSerikoSequence> sequences, bool hontai)
        {
            int n_sequences = sequences.Count;
            for (int i = 0; i < n_sequences; i++)
            {
                SCSerikoSequence seq = (SCSerikoSequence)sequences.ElementAt(i);
                Execute(seq.id(), hontai);
            }
        }

        public void Execute(int seqid, bool hontai)
        {
            // 現在のグループ内の指定されたシーケンスを走らせます。
            // シーケンスタイプがbindだった場合でも正しく動作します。
            if (SCFoundation.STOP_SERIKO) return;
            //if (isExclusiveLocked()) return;

            SCSerikoSeqGroup group = (hontai ? current_hontai_group : current_unyuu_group);
            if (group == null) return;
            SCSerikoSequence seq = group.FindSequence(seqid);
            if (seq == null) return;

            // 既に終了しているランナーはリストから削除する。
            int n_runners = runners.Count;
            for (int i = 0; i < n_runners; i++)
            {
                try
                {
                    SCSerikoSequenceRunner r = (SCSerikoSequenceRunner)runners.ElementAt(i);
                    if ((r?.IsAlive() == false))
                    {
                        runners.RemoveAt(i);
                        //  n_runners--; // 1個減った。
                        i--; // iを変えずにまたループ。
                    }
                }
                catch(Exception e)
                {

                }

                n_runners = runners.Count;

            }
            SCSerikoSequenceRunner runner = new SCSerikoSequenceRunner(this, shell, seq, hontai, session);
            runners.Add(runner);
            runner.Start();
        }

        public void TerminateAllRunnersOverlay()
        {
            int n_runners = runners.Count;
            for (int i = 0; i < n_runners; i++)
            {
                SCSerikoSequenceRunner runner = (SCSerikoSequenceRunner)runners.ElementAt(i);
                runner?.TerminateOverlay();
            }
        }

        public void TerminateAllRunnersOverlay(bool hontai)
        {
            int n_runners = runners.Count;
            for (int i = 0; i < n_runners; i++)
            {
                SCSerikoSequenceRunner runner = (SCSerikoSequenceRunner)runners.ElementAt(i);
                if (runner.IsHontai() == hontai)
                {
                    runner?.TerminateOverlay();
                }
            }
        }

        public void InterruptAllRunners(bool sakura)
        {
            int n_runners = runners.Count();
            for (int i = 0; i < n_runners; i++)
            {
                SCSerikoSequenceRunner runner =
                (SCSerikoSequenceRunner)runners.ElementAt(i);
                if (runner.IsHontai() == sakura)
                {
                    runner.Interrupt();
                }
            }
        }

        public void TerminateAllRunnersThread()
        {
            int n_runners = runners.Count;
            for (int i = 0; i < n_runners; i++)
            {
                SCSerikoSequenceRunner runner = (SCSerikoSequenceRunner)runners.ElementAt(i);
                runner.TerminateThread();
            }
        }

        public void TerminateAllRunnersThread(bool hontai)
        {
            // 動作はお察しください。
            int n_runners = runners.Count;
            for (int i = 0; i < n_runners; i++)
            {
                SCSerikoSequenceRunner runner = (SCSerikoSequenceRunner)runners.ElementAt(i);
                if (runner.IsHontai() == hontai) runner.TerminateThread();
            }
        }

        public void TerminateTargetRunner(int target_id, bool hontai)
        {
            int n_runners = runners.Count;
            for (int i = 0; i < n_runners; i++)
            {
                SCSerikoSequenceRunner runner = (SCSerikoSequenceRunner)runners.ElementAt(i);
                if (runner.GetSeqID() == target_id && runner.IsHontai() == hontai) runner.TerminateThread();
            }
        }

        public void SetExclusiveLocked(bool b)
        {
            exclusive_locked = b;
        }

        public bool IsExclusiveLocked()
        {
            return exclusive_locked;
        }

        public SCSerikoSeqGroup GetCurrentSeqGroup(bool hontai)
        {
            return (hontai ? current_hontai_group : current_unyuu_group);
        }

        public void Terminate()
        {
            // SERIKOを終了します。
            seriko_lib = null;
            current_hontai_group = null;
            current_unyuu_group = null;
            bindCenter.Terminate(); 
            bindCenter = null;

            TerminateAllRunnersOverlay();
            TerminateAllRunnersThread();
            runners.Clear();

            session = null;
            shell = null;

              

        }

        public SCSerikoBindCenter GetBindCenter()
        {
            // 着せ替えメニューが存在しなければnull。
            return bindCenter;
        }

        public String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("session: {").Append(session).Append('}');
            buf.Append("; shell: {").Append(shell).Append('}');
            if (runners.Count() > 0)
            {
                buf.Append("; runners: [");

                foreach (SCSerikoSequenceRunner runner in runners)
                {
                    buf.Append('{').Append(runner).Append('}');
                     
                    buf.Append(", ");
                     
                }
                buf.Append(']');
            }
            if (exclusive_locked)
            {
                buf.Append("; exclusive locked");
            }
            return buf.ToString();
        }

        protected void Finalize()
        {
            //Logger.log(this, Logger.DEBUG, "finalized");
        }
        

    }
}
