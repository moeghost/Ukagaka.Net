using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
    public class NSMutableAttributedString:NSAttributedString
    {

        
        private NSMutableAttributedString attributed;

        public NSMutableAttributedString(NSMutableAttributedString attributed):base(attributed.ToString())
        {
           
        }

        public NSMutableAttributedString(string baseString):base(baseString)
        {
             
        }

        public void AddAttribute(string AttributeString,NSFont font,NSRange range)
        {
            this.baseString = AttributeString;

        }

        public void AddAttribute(string AttributeString, NSColor font, NSRange range)
        {


        }

        internal void AddAttributeInRange(string fontAttributeName, object value, NSRange range_whole)
        {
            throw new NotImplementedException();
        }
    }
}
