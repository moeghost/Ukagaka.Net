/*============================================
 * 类名 :DivisionOperate
 * 描述 :
 *   
 * 创建时间: 2011-2-6 16:16:44
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class DivisionOperate : OperateNode
    {

        public override string Format {
            get { return "/"; }
        }

        public override Node NewObject(string value) {
            return new DivisionOperate();
        }

        public override int Priority {
            get { return 3; }
        }

        public override string Description {
            get { return "除法操作符"; }
        }

        public override BigNumber Value {
            get
            { 
                
                return Nexts[0].Value / Nexts[1].Value; 
            
            
            }
        }
    }
}
