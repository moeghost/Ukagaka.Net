/*============================================
 * 类名 :NumberNode
 * 描述 :
 *   
 * 创建时间: 2011-2-5 10:58:26
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class NumberNode : Node
    {
        public NumberNode() { }
        public NumberNode(string value) : base(value) { }


        int _carry;
        public int Carry
        {
            get { return _carry; }
            set
            {
                _carry = value;

            }
        }
         

        public override BigNumber Value {
            get 
            {
                try 
                {
                    if (Carry != 10)
                    {
                         
                        string result = BigProgramer.BigTrans(Format, Carry, 10);

                        return new BigNumber(result);
                    }

                   


                    return new BigNumber(Format);
                } 
                catch (NumberException e) 
                {
                    throw new ExpressionException(e.Message, Index + e.Index, 1);
                }
            }
        }

        public override int Priority {
            get { return 6; }
        }

        public override string Format {
            get { return _format; }
        }

        public override int MinParameterCount {
            get { return 0; }
        }
    }
}
