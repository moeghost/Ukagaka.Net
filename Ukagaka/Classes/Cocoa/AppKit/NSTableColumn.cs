using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
    public class NSTableColumn:TableViewColumn
    {
        public NSTableColumn(string identifier, string title, string bindingPath) : base(identifier, title, bindingPath)
        {
        }

        public NSTableView.DataSource DataCell()
        {
            return null;
        }

        public string GetIdentifier()
        {
            return this.Identifier;
        }

        internal void SetDataCell(NSButtonCell bc)
        {
             
        }
    }
}
