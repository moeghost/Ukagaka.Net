using System;
using System.Collections;
using System.Text;

namespace aya.Eval
{
    public class Variable
    {
        private string separator;
        private bool readOnly;

        // ↓この二つの値は、互いのキャッシュである。
        private ArrayList array; // Value又はVariablePointerの配列。
        private Value value; // 変数の値。
                             // ↑どちらか一方はnullで良い。

        public Variable(ArrayList array)
        {
            // array: String又はVariablePointerの配列。
            this.separator = ",";
            this.readOnly = false;
            this.array = array;
            this.value = null;
        }

        public Variable(Value value)
        {
            this.separator = ",";
            this.readOnly = false;
            this.array = null;
            this.value = value;
        }

        public Variable Lock()
        {
            readOnly = true;
            return this;
        }

        public Variable Unlock()
        {
            readOnly = false;
            return this;
        }

        public Value GetValue()
        {
            if (value == null)
            {
                // フラットな値がキャッシュされていない。
                StringBuilder flat = new StringBuilder();
                for (int i = 0; i < array.Count; i++)
                {
                    if (i > 0)
                    {
                        flat.Append(separator);
                    }

                    object elem = array[i];
                    if (elem is Value)
                    {
                        flat.Append(((Value)elem).GetString());
                    }
                    else if (elem is VariablePointer)
                    {
                        flat.Append(((VariablePointer)elem).Fetch());
                    }
                    else
                    {
                        throw new InvalidOperationException($"Internal Error: an object of {elem.GetType().FullName} was in array part of Variable illegally.");
                    }
                }
                value = new Value(flat.ToString());
            }
            return value;
        }

        public Value GetValue(int index)
        {
            if (array == null)
            {
                // 配列がキャッシュされていない。
                array = new ArrayList();

                string src = value.GetString();
                for (int start = 0; ;)
                {
                    int pos = src.IndexOf(separator, start);
                    if (pos == -1)
                    {
                        // もう無い。
                        array.Add(new Value(src.Substring(start)));
                        break;
                    }

                    array.Add(new Value(src.Substring(start, pos - start)));
                    start = pos + separator.Length;
                }
            }

            // 配列の要素数をindexが越えていたら、常に空文字列を返す。
            if (index >= array.Count)
            {
                return new Value("");
            }
            else
            {
                object obj = array[index];
                if (obj is Value)
                {
                    return (Value)obj;
                }
                else if (obj is VariablePointer)
                {
                    return ((VariablePointer)obj).Fetch();
                }
                else
                {
                    throw new InvalidOperationException($"Internal Error: an object of {obj.GetType().FullName} was in array part of Variable illegally.");
                }
            }
        }

        public Variable SetValue(Value value)
        {
            if (readOnly)
            {
                throw new InvalidOperationException("This variable is read-only. Do not modify this.");
            }

            this.array = null;
            this.value = value;
            return this;
        }

        public Variable SetValue(int index, Value value)
        {
            if (readOnly)
            {
                throw new InvalidOperationException("This variable is read-only. Do not modify this.");
            }

            this.value = null;
            if (array == null)
            {
                GetValue(0); // 配列を作ってキャッシュ
            }

            // indexは実際の配列のサイズを越えているか？
            if (index >= array.Count)
            {
                // この要素が入る所まで空文字列を追加し続ける。
                for (int i = array.Count; i < index; i++)
                {
                    array.Add("");
                }
                // この要素を入れる。
                array.Add(value.GetString());
            }
            else
            {
                // 元の値がValueか暗黙のVariablePointerなら、単純に置き換える。
                // 明示的なVariablePointerなら、参照先を書き換える。
                object original = array[index];
                if (original is Value)
                {
                    array[index] = value;
                }
                else if (original is VariablePointer)
                {
                    VariablePointer vptr = (VariablePointer)original;
                    if (vptr.IsTacit())
                    {
                        array[index] = value;
                    }
                    else
                    {
                        vptr.Store(value);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Internal Error: an object of {original.GetType().FullName} was in array part of Variable illegally.");
                }
            }
            return this;
        }

        public string GetSeparator()
        {
            return separator;
        }

        public Variable SetSeparator(string sep)
        {
            if (readOnly)
            {
                throw new InvalidOperationException("This variable is read-only. Do not modify this.");
            }

            separator = sep;
            array = null; // 配列のキャッシュをクリア
            return this;
        }

        public long GetArraySize()
        {
            if (array == null)
            {
                GetValue(0); // 配列を作ってキャッシュ
            }
            return array.Count;
        }
    }
}
