using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleGrayCalculator.Cores
{
    internal class FactorialOperate:OperateNode
    {
        public override string Format
        {
            get { return "!"; }
        }

        public override Node NewObject(string value)
        {
            return new FactorialOperate();
        }

        public override int Priority
        {
            get { return 1; }
        }

        public override string Description
        {
            get { return "阶乘操作符"; }
        }

        public override BigNumber Value
        {
            get {
                try
                {
                    return Nexts[0].Value.Factorial();
                }
                catch (NumberException)
                {
                    throw new ExpressionException("阶乘函数的参数只能是正整数", Index, Format.Length);
                }
            }
        }
        public override int MinParameterCount
        {
            get { return 1; }
        }



    }
}
