/*============================================
 * 类名 :ExpConstant
 * 描述 :
 *   
 * 创建时间: 2011-2-6 16:18:18
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class ExpConstant :ConstantNode 
    {
        public override string Format 
        {
            get { return "e"; }
        }

        public override Node NewObject( string value )
        {
            return new ExpConstant();
        } 

        public override string Description
        {
            get { return "表示自然常量e，精确到小数点后14位"; }
        }

        public override BigNumber Value
        {
            get {   return new BigNumber( Math.E.ToString() ); }
        }
    }
}
