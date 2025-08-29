using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Ukagaka.Classes.Cocoa.AppKit;
namespace Cocoa.AppKit
{
    public  class NSRange:NSObject
    {

         
        public int First { get; set; }
        public int Length { get; set; }

        public NSRange(int First, int Length)
        {
            this.First = First;
            this.Length = Length;
            
        }
        public override bool Equals(object obj)
        {
            return (NSRange)obj == this;
        }

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }


        public static bool operator == (NSRange cr1, NSRange cr2)
        {
            return cr1.First == cr2.First && cr1.Length == cr2.Length;
        }
        public static bool operator != (NSRange cr1, NSRange cr2)
        {
            return !(cr1 == cr2);
        }


        public CharacterRange ToCharacterRange()
        {
            return new CharacterRange(First,Length);

        }

    }
}
