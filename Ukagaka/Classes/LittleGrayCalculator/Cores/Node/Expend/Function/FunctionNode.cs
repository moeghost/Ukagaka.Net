/*============================================
 * 类名 :FunctionNode
 * 描述 :函数节点
 *   
 * 创建时间: 2011-2-5 10:59:32
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>函数节点</summary>
    abstract class FunctionNode : ExpendNode
    {

        public override int Priority {
            get { return 5; }
        }

        bool _isRadian;
        public override bool IsRadian
        {
            get { return _isRadian; }
            set
            {

                _isRadian = value;

            }

        }

    }
}
