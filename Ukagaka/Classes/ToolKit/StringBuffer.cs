using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class StringBuffer
    {
        private StringBuilder builder;

        public StringBuffer(int value)
        {
            builder = new StringBuilder(value.ToString());
        }


        public StringBuffer()
        {
            builder = new StringBuilder();
        }

        public StringBuffer(string str) 
        {
            builder = new StringBuilder(str);
        }

        internal StringBuffer Append(string v)
        {
            builder.Append(v);
            return this;
        }

        internal StringBuffer Append(char v)
        {
            builder.Append(v);
            return this;
        }

        internal StringBuffer Append(object v)
        {
            builder.Append(v);
            return this;
        }
        internal char CharAt(int v)
        {
             return builder.ToString().ToCharArray().ElementAt(v);
        }

        internal void Insert(int pos, char v)
        {
            builder.Insert(pos, v);
        }

        internal void Insert(int pos, string str)
        {
            builder.Insert(pos, str);
        }

        internal int Length()
        {
            return builder.Length;
        }

        internal void Remove(int start, int length)
        {
             builder.Remove(start, length);
        }

        internal void Replace(string oldValue, string newValue)
        {
            builder.Replace(oldValue, newValue);
        }

        internal void Replace(int start, int length, string value)
        {
            builder.Replace(builder.ToString().Substring(start, length), value);
        }

        internal void SetLength(int length)
        {
             builder.Length = length;
        }

        internal string Substring(int start, int end)
        {
            return builder.ToString().Substring(start, end - start);
        }


        public override string ToString()
        {
            return builder.ToString();

        }

        internal void SetCharAt(int pos, char chr)
        {
            if (pos >= builder.Length)
            {
                return;
            }
            builder[pos] = chr;
        }
    }
}
