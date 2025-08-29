/*============================================
 * 类名 :Expression
 * 描述 :
 *   
 * 创建时间: 2011-2-5 10:38:33
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>表达式</summary>
    class Expression
    {
        public Expression() 
        {
            this.Carry = 10;
        }

        private List<Node> root;

        public List<Node> Root
        {

            get { return root; }

            set 
            {
                root = value;
                Head = Syntax.Analyse(root); 

            }
        }

        public Expression( string value )
        {
            //this._format = value;
            //Head = Syntax.Analyse( lexical.Analyse( value ) );
            this.Format = value;
            this.Carry = 10;
            this.IsRadian = false;
        }
        string _format;
        /// <summary>表达式的字符串形式</summary>
        public String Format
        {
            get { return _format; }
            set
            {
                this._format = value;
                _format = _format.Replace( " ", "" );
                _format = _format.Replace( "\n", "" );
                if (_format == string.Empty)
                {
                    throw new ExpressionException("空表达式");
                }
                Head = Syntax.Analyse( lexical.Analyse( value ) );
            }
        }
        int _carry;
        public int Carry
        {
            get { return _carry; }
            set
            {

                _carry = value;
                lexical.Carry = value;

            }

        }
        bool _isRadian;
        public bool IsRadian
        {
            get { return _isRadian; }
            set
            {

                _isRadian = value;
                lexical.IsRadian = value;

            }

        }

        Lexical lexical = new Lexical();
        /// <summary>树形表达式的头节点</summary>
        public Node Head { get; set; }

        /// <summary>求值</summary>
        public BigNumber Calculator()
        {
            // sd dfasfsdfasdf
             
            return Head.Value;
        }

    }
}
