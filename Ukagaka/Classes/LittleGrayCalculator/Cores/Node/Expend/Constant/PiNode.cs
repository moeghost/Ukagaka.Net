/*============================================
 * 类名 :PINode
 * 描述 :
 *   
 * 创建时间: 2011-2-6 11:11:51
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text; 

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class PiNode :ConstantNode 
    {
        public override string Format 
        {
            get { return "PI"; }
        }  

        public override Node NewObject( string value )
        {
            return new PiNode();
        }

        public override string Description
        {
            get { return "表示圆周率，精确到小数点后14位"; }
        }

        public override  BigNumber  Value
        {
            get {
                // return new BigNumber( Math.PI.ToString() );  
                return new BigNumber("3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679821");
                }
        }
    }
}
