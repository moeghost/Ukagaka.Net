/*============================================
 * 类名 :OperateNode
 * 描述 :
 *   
 * 创建时间: 2011-2-5 10:58:09
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{

    public enum Associativity
    {
        Left,    // 从左向右计算（如 `a + b + c`）
        Right    // 从右向左计算（如 `a = b = c`）
    }

    /// <summary>操作符节点</summary>
    abstract class OperateNode : ExpendNode
    {
        public override int MinParameterCount 
        {
            get { return 2; }
        }
        public virtual int MinCharCount
        {
            get { return 1; }
        }

        



        public Associativity Associativity { get; set; } = Associativity.Left; // 默认左结合


    }
}
