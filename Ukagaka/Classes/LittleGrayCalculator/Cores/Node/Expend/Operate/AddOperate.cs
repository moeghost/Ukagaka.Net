/*============================================
 * 类名 :AddOperate
 * 描述 :
 *   
 * 创建时间: 2011-2-6 16:13:23
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class AddOperate : OperateNode
    { 
        public override string Format
        {
            get { return "+"; }
        }

        public override Node NewObject( string value )
        {
            return new AddOperate(  );
        }
 
        public override int Priority
        {
            get { return 2; }
        }

        public override string Description
        {
            get { return "加法操作符"; }
        }

        public override BigNumber Value
        {
            get {   return Nexts[0].Value + Nexts[1].Value;  }
        }
    }
}
