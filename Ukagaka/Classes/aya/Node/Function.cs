using aya.Eval;
using aya;
using System;
using System.Collections.Generic;
 
using System.Collections;

namespace aya.Node
{
    public class Function
    {
        private Aya aya;
        private string name;
        private Block block;

        public Function(Aya aya, string name, Block block)
        {
            this.aya = aya;
            this.name = name;
            this.block = block;
        }

        public Aya GetAya()
        {
            return aya;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }

        public void Clear()
        {
             
        }

        public Value Eval(ArrayList args)
        {
            // args: 要素がVariablePointerまたはValueである配列。nullでも良い。
            // 戻り値: nullである場合もある。

            // 名前空間を作り、_argvと_argcを設定
            Namespace ns = new Namespace();
            if (args != null)
            {
                ns.Put("_argv", new Variable(args));
                ns.Put("_argc", new Variable(new Value(args.Count)).Lock());
            }
            else
            {
                ns.Put("_argc", new Variable(new Value(0)).Lock());
            }

            // この名前空間を用いてブロックを通常評価
            try
            {
                return block.Eval(ns);
            }
            /* 
            catch (Statement.BreakOccurrence b)
            {
                throw new Exception("'break' used in function.");
            }
            catch (Statement.ContinueOccurrence b)
            {
                throw new Exception("'continue' used in function.");
            }
            catch (Exception e)
            {
               throw new Exception($"Couldn't evaluate function {name}", e);
            }
             */
            catch
            {
                return null;
            }
        }
    }
}
