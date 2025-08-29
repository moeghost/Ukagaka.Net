using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
    public class CGFloat
    {
        private float value;

        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }

        }

        public CGFloat()
        {
            this.value = 0;
        }
        public CGFloat(float value)
        {
            this.value = value;
        }

        public CGFloat(double value)
        {
            this.value = (float)value;
        }

        /*
        public float Value()
        {
            return value;
        }
         */


        public int IntValue()
        {
            return (int)value;
        }
        //重载运算符"+"，计算年龄总和.
        public static CGFloat operator +(CGFloat lhs, CGFloat rhs)
        {
            return new CGFloat(lhs.value + rhs.value);
        }

        //重载运算符"-"，计算年龄差.
        public static CGFloat operator -(CGFloat lhs, CGFloat rhs)
        {
            return new CGFloat(lhs.value - rhs.value);
        }

        //重载==运算符，Id相同则视为相等.
        public static bool operator ==(CGFloat lhs, CGFloat rhs)
        {
            return lhs.value == rhs.value;
        }

        //比较运算符必须成对重载.
        public static bool operator !=(CGFloat lhs, CGFloat rhs)
        {
            return !(lhs == rhs);
        }

        //比较运算符必须成对重载.
        public static bool operator >(CGFloat lhs, CGFloat rhs)
        {
            return !(lhs.Value > rhs.Value);
        }
        //比较运算符必须成对重载.
        public static bool operator <(CGFloat lhs, CGFloat rhs)
        {
            return !(lhs.Value < rhs.Value);
        }
    }
}
