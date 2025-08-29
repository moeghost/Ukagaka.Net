using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using aya.Node;
namespace aya.Eval
{
    public class SystemCall : Function
    {
        private readonly MethodInfo function;
        private readonly Hashtable cache; // {FunctionName => SystemCall}

        public SystemCall(Aya aya) : base(aya, null, null)
        {
            // Factory constructor
            function = null;
            cache = new Hashtable();
        }

        protected SystemCall(Aya aya, string name, MethodInfo function) : base(aya, name, null)
        {
            this.function = function;
            cache = null;
        }

        private static readonly Regex patPeriod = new Regex("\\.");

        public SystemCall Get(string name)
        {
            if (name.IndexOf('.') != -1)
            {
                name = patPeriod.Replace(name, "__period__");
            }

            SystemCall cached = (SystemCall)cache[name];
            if (cached != null)
            {
                return cached;
            }

            try
            {
                MethodInfo function = GetType().GetMethod("Syscall_" + name, new[] { typeof(ArrayList) });
                SystemCall syscall = new SystemCall(GetAya(), name, function);
                cache[name] = syscall;
                return syscall;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Value Eval(ArrayList args)
        {
            try
            {
                return (Value)function.Invoke(this, new object[] { args });
            }
            catch (Exception e)
            {
                throw new Exception($"Calling systemcall failed: {GetName()}({(args == null ? "" : args.ToString())})", e);
            }
        }

        public Value Syscall_RAND(ArrayList args)
        {
            // RAND(n)
            // 0〜n-1 の範囲でランダム値を得ます。	
            // n は省略可能で、この場合0〜99の乱数を生成します。
            long limit;
            if (args != null && args.Count > 0)
            {
                limit = Content(args[0]).GetInteger();
            }
            else
            {
                limit = 100;
            }

            if (limit == 0)
            {
                return new Value(0);
            }
            else
            {
                return new Value((long)(new Random().NextDouble() * long.MaxValue) % limit);
            }
        }

        public Value Syscall_ASC(ArrayList args)
        {
            // ASC(c)
            // ASCII文字を返します。
            // 指定範囲は 0≦c＜0x80 で、範囲外の c を与えると半角空白(20H)に代替されます。
            long n = Content(args.ToArray().ElementAt(0)).GetInteger();
            if (n < 0 || n >= 0x80)
            {
                n = 0x20;
            }

            char c = (char)n;
            return new Value("" + c);
        }

        public Value Syscall_IASC(ArrayList args)
        {
            // IASC("foo")
            // 文字列先頭の 1 バイトの文字コードを返します。半角文字しか処理できないことに注意してください。
            // 数値や空文字列を与えると -1 が返ります。
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(Content(args[0]).GetString());
            return new Value(bytes[0]);
        }

        public Value Syscall_ISINSIDE(ArrayList args)
        {
            // ISINSIDE(val,-5,10)
            // ISINSIDE(val,"foo","bar")
            // 数値および文字列が指定範囲内にあるかチェックします。
            // 引数は 3 つです。第一引数はチェック対象の値(A)。第二、三引数は範囲を指定する値(i0、i1）です。
            // i0 ≦ A ≦ i1 で 1 、それ以外で 0 が返ります。i0 と i1 の大小は気にしなくて構いません。i0 のほうが大きくても正しく動作します。
            // チェック対象の価が文字列の場合は辞書順で比較されます。
            Value attention = Content(args[0]);
            if (attention.IsNumeric())
            {
                double n = attention.GetReal();
                double x = Content(args.ToArray().ElementAt(1)).GetReal();
                double y = Content(args.ToArray().ElementAt(2)).GetReal();

                return new Value((x <= n && n <= y) || (y <= n && n <= x) ? 1 : 0);
            }
            else
            {
                string str = attention.GetString();
                string x = Content(args.ToArray().ElementAt(1)).GetString();
                string y = Content(args.ToArray().ElementAt(2)).GetString();

                return new Value((x.CompareTo(str) <= 0 && str.CompareTo(y) <= 0) ||
                                 (y.CompareTo(str) <= 0 && str.CompareTo(x) <= 0) ? 1 : 0);
            }
        }

        // ---------------- 型チェック ---------------- //

        public Value Syscall_ISREAL(ArrayList args)
        {
            // ISREAL(n)
            // 値が数値（実数）かチェックします。実数には当然ながら整数が含まれます。
            // 戻り値　0/1 = 文字列/実数。
            return new Value(Content(args[0]).IsNumeric() ? 1 : 0);
        }

        public Value Syscall_ISINTEGER(ArrayList args)
        {
            // ISINTEGER(n)
            // 値が（小数点以下の位がない）整数かチェックします。
            // 戻り値　0/1 = 文字列 or 小数点以下の位がある数値/整数。 
            return new Value(Content(args[0]).IsInteger() ? 1 : 0);
        }

        public Value Syscall_ISFUNCTION(ArrayList args)
        {
            // ISFUNCTION("foo")
            // 文字列が関数名かチェックします。
            // 戻り値　0/1/2 = 関数名ではない/関数名/システム関数名。
            string name = Content(args[0]).GetString();

            Function f = GetAya().GetDictionary().GetFunction(name);
            if (f == null)
            {
                return new Value(0);
            }
            else if (f is SystemCall)
            {
                return new Value(2);
            }
            else if (f is Function)
            {
                return new Value(1);
            }
            else
            {
                throw new Exception();
            }
        }
        // ---------------- 型変換 ---------------- //

        public Value Syscall_TONUMBER(ArrayList args)
        {
            // TONUMBER(var)
            // システム関数 TONUMBER は、変数に格納された文字列を数値に変換します。
            VariablePointer vptr = (VariablePointer)args[0];
            vptr.Store(vptr.Fetch().SetReal());
            return null;
        }

        public Value Syscall_TOSTRING(ArrayList args)
        {
            // TOSTRING(var)
            // TOSTRING は TONUMBER と逆の操作、すなわち数値から文字列への変換を行います。
            VariablePointer vptr = (VariablePointer)args[0];
            vptr.Store(vptr.Fetch().SetString());
            return null;
        }

        public Value Syscall_TONUMBER2(ArrayList args)
        {
            // TONUMBER2("25")
            // TONUMBER2 は TONUMBER と機能は同等ですが、結果は戻り値として出力し、引数の内容にはタッチしません。
            return ((Value)Content(args[0]).Clone()).SetReal();
        }

        public Value Syscall_TOSTRING2(ArrayList args)
        {
            // TOSTRING2(12)
            // TOSTRING2 は TOSTRING と機能は同等ですが、結果は戻り値として出力し、引数の内容にはタッチしません。
            return ((Value)Content(args[0]).Clone()).SetString();
        }

        public Value Syscall_TOBINSTR(ArrayList args)
        {
            // TOBINSTR(3)
            // TOBINSTR は 数値を 2 進数表記の文字列へ変換します。
            return new Value(Convert.ToString(Content(args[0]).GetInteger(), 2));
        }

        public Value Syscall_TOHEXSTR(ArrayList args)
        {
            // TOHEXSTR(5)
            // TOBINSTR は 数値を 16 進数表記の文字列へ変換します。
            return new Value(Convert.ToString(Content(args[0]).GetInteger(), 16));
        }

        public Value Syscall_BINSTRTONUM(ArrayList args)
        {
            // BINSTRTONUM("10110")
            // BINSTRTONUM は TOBINSTR の逆操作を行います。すなわち、2 進数表記の文字列を数値へ変換します。
            return new Value(Convert.ToInt64(Content(args[0]).GetString(), 2));
        }

        public Value Syscall_HEXSTRTONUM(ArrayList args)
        {
            // HEXSTRTONUM("2f3a")
            // HEXSTRTONUM は TOHEXSTR の逆操作を行います。すなわち、16 進数表記の文字列を数値へ変換します。
            return new Value(Convert.ToInt64(Content(args[0]).GetString(), 16));
        }


        public Value Syscall_ARRAYSIZE(ArrayList args)
        {
            // ARRAYSIZE("foo,bar,baz")
            // ARRAYSIZE("foo,bar,baz", num)
            // ARRAYSIZE システム関数は、簡易配列の要素数を求めます。
            // 引数は 1 つもしくは 2 つです。第一引数は要素数を調べる文字列。第二引数は結果を取得する変数ですが、これを省略すると結果を戻り値から取得できます。
            // この関数に数値を渡してはいけません。渡した場合 0 が返ります。
            Value result = null;

            object obj = args[0];
            if (obj is Value val)
            {
                if (val.IsNumeric())
                {
                    result = new Value(0);
                }
                else
                {
                    // デフォルトはカンマ区切り
                    string str = val.GetString();
                    int nComma = str.Count(c => c == ',');
                    result = new Value(nComma + 1);
                }
            }
            else if (obj is VariablePointer vptr)
            {
                if (vptr.GetVariableReference().GetIndex() != null)
                {
                    throw new Exception("ARRAYSIZE to element of array.");
                }
                result = new Value(vptr.GetVariable().GetArraySize());
            }
            else
            {
                throw new Exception();
            }

            if (args.Count >= 2)
            {
                VariablePointer vptr = (VariablePointer)args.ToArray().ElementAt(1);
                vptr.Store(result);
            }
            return result;
        }

        public Value Syscall_SETSEPARATOR(ArrayList args)
        {
            // SETSEPARATOR(foo, "-")
            // 簡易配列のセパレータは標準では半角カンマ "," ですが、SETSEPATRATOR システム関数を使用することによって、変数ごとに異なる文字列へ変更することができます。
            // ただし、システム変数のセパレータはデフォルトの半角カンマから変更することはできません。
            // 
            // 引数は 2 つで、第一引数に新しいセパレータを設定する変数、第二引数に新しいセパレータ文字列を指定します。戻り値はありません。
            VariablePointer vptr = (VariablePointer)args[0];
            vptr.GetVariable().SetSeparator(Content(args.ToArray().ElementAt(1)).GetString());
            return null;
        }

        // ---------------- 変数/関数の間接実行 ---------------- //

        public Value Syscall_NAMETOVALUE(ArrayList args)
        {
            // NAMETOVALUE は、変数名を示す文字列からその内容を取得します。
            // 引数は 1 つもしくは 2 つです。引数に変数名を示す文字列のみを指定すると、戻り値でその変数の値が取得できます。第一引数に結果を取得する変数、第二引数に変数名を示す文字列を指定することもでき、この場合戻り値はありません。
            // 指定した変数が存在しなかった場合は空文字列が返ります.
            VariablePointer toStore = null;
            string varName = null;
            if (args.Count == 1)
            {
                varName = Content(args[0]).GetString();
            }
            else if (args.Count == 2)
            {
                toStore = (VariablePointer)args[0];
                varName = Content(args.ToArray().ElementAt(1)).GetString();
            }

            // 制限： 現状ではグローバル変数に対してのみ使用可能。
            if (varName[0] == '_')
            {
                throw new Exception("system-call NAMETOVALUE is currently restricted to be used with only global variable.");
            }

            Variable v = GetAya().GetGlobalNamespace().Get(varName);
            Value result = (v == null) ? new Value("") : v.GetValue();

            if (toStore == null)
            {
                return result;
            }
            else
            {
                toStore.Store(result);
                return null;
            }
        }

        public Value Syscall_LETTONAME(ArrayList args)
        {
            // LETTONAME("name", "さくら")
            // LETTONAME は NAMETOVALUE の逆操作で、変数名を示す文字列へ値を代入します。
            // 指定した変数が存在しない場合は変数が作成されます。
            // 戻り値はありません。
            string varName = Content(args[0]).GetString();
            Value val = Content(args.ToArray().ElementAt(1));

            // 制限： 現状ではグローバル変数に対してのみ使用可能。
            if (varName[0] == '_')
            {
                throw new Exception("system-call LETTONAME is currently restricted to be used with only global variable.");
            }

            Variable v = GetAya().GetGlobalNamespace().Define(varName);
            v.SetValue(val);
            return null;
        }

        public Value Syscall_CALLBYNAME(ArrayList args)
        {
            /*
                関数名から関数を実行します。
                実行した関数の戻り値が、そのまま CALLBYNAME の戻り値となります。

                result = CALLBYNAME("add(1,2)")

                上の例では加算を行う関数 add(1,2) を実行し、結果を result に代入しています。
            */
            // Expression.StringFactorのevalを流用する。
            string str = '%' + Content(args[0]).GetString();
            return Expression.GetFactory().NewFactor(GetAya(), str).Eval(GetAya().GetGlobalNamespace());
        }

        // ---------------- デバッグ ---------------- //

        public Value Syscall_LOGGING(ArrayList args)
        {
            // 本来は第一引数の式とその値をファイルに出力する関数だが、
            // プリプロセッサでも使わない限り実装が面倒なので、やるとしても後で。
            Console.Error.WriteLine("LOGGING: " + Content(args[0]).GetString());
            return null;
        }

        public Value Syscall_GETLASTERROR(ArrayList args)
        {
            // GETLASTERROR
            // 以下の関数が失敗した場合（0 や -1 を返してきた場合。関数により異なる）、直後に GETLASTERROR を呼ぶことで失敗した理由を読み出すことができます。
            // LOADLIB , FOPEN , FCOPY , FMOVE , FDELETE , FRENAME , FSIZE , MKDIR , RMDIR , FENUM
            return new Value(0);
        }

        // ---------------- リクエスト値の取得 ---------------- //

        public Value Syscall_REQ__period__COMMAND(ArrayList args)
        {
            // リクエストのコマンド文字列を返します。
            return new Value(GetAya().GetMethodOfLastRequest());
        }

        public Value Syscall_REQ__period__PROTOCOL(ArrayList args)
        {
            // リクエストのプロトコル名称とバージョン部分を返します。
            return new Value(GetAya().GetProtocolOfLastRequest());
        }

        public Value Syscall_REQ__period__HEADER(ArrayList args)
        {
            // n 番目のヘッダのキー名称を取得します。
            string key = GetAya().GetKeyOfLastRequest((int)Content(args[0]).GetInteger());
            return new Value(key);
        }

        public Value Syscall_REQ__period__KEY(ArrayList args)
        {
            // REQ.HEADERと同じ。
            return Syscall_REQ__period__HEADER(args);
        }

        public Value Syscall_REQ__period__VALUE(ArrayList args)
        {
            /*
                この関数はオーバーロードされており、引数にはキーの名前、もしくは数値を与えることができます。
                引数にキー名称を与えると、その値が取得できます。
                数値の場合は REQ.HEADER 関数と同じく、 n 番目のヘッダ値取得となります。n は 0 origin です。
                いずれの場合でも、イリーガルな引数が指定された場合は空文字列が返ります。
            */
            Value arg = Content(args[0]);
            if (arg.IsInteger())
            {
                string value = GetAya().GetValueOfLastRequest((int)arg.GetInteger());
                // どうやら値が数値なら数値を返さなきゃならないらしい。
                try
                {
                    return new Value(double.Parse(value));
                }
                catch (Exception e)
                {
                    return new Value(value);
                }
            }
            else if (arg.IsString())
            {
                string value = GetAya().GetValueOfLastRequest(arg.GetString());
                try
                {
                    return new Value(double.Parse(value));
                }
                catch (Exception e)
                {
                    return new Value(value);
                }
            }
            return new Value("");
        }

        // ---------------- 汎用 DLL 呼び出し ---------------- //

        public Value Syscall_LOADLIB(ArrayList args)
        {
            WarnUseOfDllFunctions();
            return null;
        }

        public Value Syscall_UNLOADLIB(ArrayList args)
        {
            WarnUseOfDllFunctions();
            return null;
        }

        public Value Syscall_REQUESTLIB(ArrayList args)
        {
            WarnUseOfDllFunctions();
            return null;
        }

        public Value Syscall_LIB__period__STATUSCODE(ArrayList args)
        {
            WarnUseOfDllFunctions();
            return new Value("");
        }

        public Value Syscall_LIB__period__PROTOCOL(ArrayList args)
        {
            WarnUseOfDllFunctions();
            return new Value("");
        }

        public Value Syscall_LIB__period__HEADER(ArrayList args)
        {
            WarnUseOfDllFunctions();
            return new Value("");
        }

        public Value Syscall_LIB__period__KEY(ArrayList args)
        {
            return Syscall_LIB__period__HEADER(args);
        }

        public Value Syscall_LIB__period__VALUE(ArrayList args)
        {
            WarnUseOfDllFunctions();
            return new Value("");
        }

        private bool haveWarnedUseOfDllFunctionsBefore = false;

        private void WarnUseOfDllFunctions()
        {
            if (!haveWarnedUseOfDllFunctionsBefore)
            {
                haveWarnedUseOfDllFunctionsBefore = true;
                Console.Error.WriteLine("Warning: This ghost of the Aya uses system functions of calling DLL. Java implementation of the Aya doesn't support them.");
            }
        }


        // ---------------- ファイル操作 ---------------- //
 
        public Value Syscall_FOPEN(ArrayList args)
        {
            /*
                ファイルをオープンします。読み取りや書き込みを行う前に、必ず対象のファイルをオープンします。
                引数は 2 つで、第一引数は対象のファイル名（aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能）、第二引数がオープンモードです。

                オープンモードは以下の 3 種類より選びます。

                モード　　　意味
                ───────────────────────────────────────
                "read"　　　読み取り
                "write" 　　書き込み。指定ファイルが既に存在している場合は上書きされる。
                "append"　　書き込み。指定ファイルが既に存在している場合はその終端に追加される。

                戻り値　0/1/2 = 失敗/成功/既に指定ファイルはオープンしている
            */
            WarnUseOfFileIOFunctions();

            string fileName = Content(args[0]).GetString();
            string mode = Content(args.ToArray().ElementAt(1)).GetString().ToLower();

            FileMode fileMode = FileMode.OpenOrCreate;
            if (mode == "read")
            {
                fileMode = FileMode.Open;
            }
            else if (mode == "write")
            {
                fileMode = FileMode.Create;
            }
            else if (mode == "append")
            {
                fileMode = FileMode.Append;
            }

            try
            {
                FileStream fileStream = new FileStream(fileName, fileMode);
                // You may want to store the fileStream reference somewhere for future use.
                return new Value(1); // Success
            }
            catch (Exception)
            {
                return new Value(0); // Failure
            }
        }

        public Value Syscall_FCLOSE(ArrayList args)
        {
            /*
                ファイルをクローズします。読み取りや書き込みを行った後は、必ず対象のファイルをクローズします。
                引数にはクローズするファイル名（aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能）を指定します。

                戻り値はありません。
            */
            WarnUseOfFileIOFunctions();

            string fileName = Content(args[0]).GetString();

            try
            {
                // Perform any necessary operations before closing the file.
                // You may want to check whether the fileStream is still open before closing it.
                // Example: if (fileStream != null && fileStream.CanRead) { /* Perform operations */ }
                // fileStream.Close();

                return null;
            }
            catch (Exception)
            {
                // Handle exceptions if needed.
                return null;
            }
        }

        public Value Syscall_FREAD(ArrayList args)
        {
            /*
                "read" でオープンしたファイルから 1 行読み取ります。
                引数にはクローズするファイル名（aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能）を指定します。

                戻り値は読み取った文字列です。読み取った文字列の終端に改行文字はありません。
                EOF（ファイルの最後）まで達したときは数値の -1 が返ります。

                例として、test.txt の内容をそのままバルーンに表示するコードを以下に示します。
                test.txt を read でオープン、1 行読み取って \n を付け加えるのを繰り返し、最後まで読み終わったらファイルをクローズ、という流れです。
            */
            WarnUseOfFileIOFunctions();

            string fileName = Content(args[0]).GetString();

            try
            {
                StreamReader reader = new StreamReader(fileName);
                string line = reader.ReadLine();
                if (line == null)
                {
                    reader.Close();
                    return new Value(-1); // EOF
                }

                return new Value(line);
            }
            catch (Exception)
            {
                return new Value("");
            }
        }

        public Value Syscall_FWRITE(ArrayList args)
        {
            /*
                "write"、"append" でオープンしたファイルへ 1 行書き込みます（処理の最後に改行コードを自動的に書き込みます）。
                引数は 2 つで、第一引数は対象のファイル名（aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能）、第二引数が書き込む内容です。

                戻り値はありません。
            */
            WarnUseOfFileIOFunctions();

            string fileName = Content(args[0]).GetString();
            string content = Content(args.ToArray().ElementAt(1)).GetString();

            try
            {
                StreamWriter writer = new StreamWriter(fileName, true);
                writer.WriteLine(content);
                writer.Close();
                return null;
            }
            catch (Exception)
            {
                // Handle exceptions if needed.
                return null;
            }
        }

        public Value Syscall_FWRITE2(ArrayList args)
        {
            /*
                "write"、"append" でオープンしたファイルへデータを書き込みます。
                FWRITE と異なり、改行コードは書き込みません。
            */
            WarnUseOfFileIOFunctions();
            return null;
        }

        public Value Syscall_FCOPY(ArrayList args)
        {
            /*
                ファイルを別のディレクトリにコピーします。
                第一引数にコピー対象のファイル名、第二引数にコピー先のディレクトリを指定します。パス指定は aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能です。
                戻り値　0/1 = 失敗/成功
            */
            WarnUseOfFileIOFunctions();

            string sourceFileName = Content(args[0]).GetString();
            string destinationDirectory = Content(args.ToArray().ElementAt(1)).GetString();

            try
            {
                string destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(sourceFileName));
                File.Copy(sourceFileName, destinationPath);
                return new Value(1); // Success
            }
            catch (Exception)
            {
                return new Value(0); // Failure
            }
        }

        public Value Syscall_FMOVE(ArrayList args)
        {
            /*
                ファイルを別のディレクトリに移動します。
                第一引数に移動対象のファイル名、第二引数に移動先のディレクトリを指定します。パス指定は aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能です。
                戻り値　0/1 = 失敗/成功
            */
            WarnUseOfFileIOFunctions();

            string sourceFileName = Content(args[0]).GetString();
            string destinationDirectory = Content(args.ToArray().ElementAt(1)).GetString();

            try
            {
                string destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(sourceFileName));
                File.Move(sourceFileName, destinationPath);
                return new Value(1); // Success
            }
            catch (Exception)
            {
                return new Value(0); // Failure
            }
        }

        public Value Syscall_FDELETE(ArrayList args)
        {
            /*
                ファイルを削除します。
                引数に削除対象のファイル名を指定します（aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能）。
                戻り値　0/1 = 失敗/成功
            */
            WarnUseOfFileIOFunctions();

            string fileName = Content(args[0]).GetString();

            try
            {
                File.Delete(fileName);
                return new Value(1); // Success
            }
            catch (Exception)
            {
                return new Value(0); // Failure
            }
        }

        public Value Syscall_FRENAME(ArrayList args)
        {
            /*
                ファイルの名前を変更します。
                引数は 2 つで、第一引数が変更前、第二引数が変更後のファイル名です（aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能）。
                戻り値　0/1 = 失敗/成功
            */
            WarnUseOfFileIOFunctions();

            string oldFileName = Content(args[0]).GetString();
            string newFileName = Content(args.ToArray().ElementAt(1)).GetString();

            try
            {
                File.Move(oldFileName, newFileName);
                return new Value(1); // Success
            }
            catch (Exception)
            {
                return new Value(0); // Failure
            }
        }

        public Value Syscall_FSIZE(ArrayList args)
        {
            /*
                ファイルサイズを単位バイトで取得します。
                引数に対象のファイル名を指定します（aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能）。
                結果は戻り値で得られます。処理に失敗した場合は -1 が返ります。
            */
            WarnUseOfFileIOFunctions();

            string fileName = Content(args[0]).GetString();

            try
            {
                long fileSize = new FileInfo(fileName).Length;
                return new Value(fileSize);
            }
            catch (Exception)
            {
                return new Value(-1); // Failure
            }
        }

        public Value Syscall_MKDIR(ArrayList args)
        {
            /*
                ディレクトリを作成します。
                引数に作成するディレクトリ名を指定します。aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能です。
                戻り値　0/1 = 失敗/成功
            */
            WarnUseOfFileIOFunctions();

            string directoryName = Content(args[0]).GetString();

            try
            {
                Directory.CreateDirectory(directoryName);
                return new Value(1); // Success
            }
            catch (Exception)
            {
                return new Value(0); // Failure
            }
        }

        public Value Syscall_RMDIR(ArrayList args)
        {
            /*
                空のディレクトリを削除します。空でないディレクトリは削除できません。
                引数に削除するディレクトリ名を指定します。aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能です。
                戻り値　0/1 = 失敗/成功
            */
            WarnUseOfFileIOFunctions();

            string directoryName = Content(args[0]).GetString();

            try
            {
                Directory.Delete(directoryName);
                return new Value(1); // Success
            }
            catch (Exception)
            {
                return new Value(0); // Failure
            }
        }

        public Value Syscall_FENUM(ArrayList args)
        {
            /*
                指定されたディレクトリ内のファイル/ディレクトリのリストを作成します。
                引数に対象のディレクトリ位置を aya.dll からの相対位置で指定します。aya.dll 直下であれば "" です。パス指定は aya.dll 位置からの相対パス指定、絶対パス指定のいずれも可能です。
                結果は戻り値で得られます。内容は半角カンマで区切られたファイル/ディレクトリ名のリストです。ディレクトリ名の先頭には \ が付されており、これを見ることで通常のファイルと区別できます。

                区切り文字は半角カンマから他の文字(列)へ変更することができます。第二引数で指定してください。
            */
            WarnUseOfFileIOFunctions();

            string directoryName = Content(args[0]).GetString();

            try
            {
                string[] files = Directory.GetFiles(directoryName);
                string[] directories = Directory.GetDirectories(directoryName);
                string[] allEntries = new string[files.Length + directories.Length];

                Array.Copy(files, allEntries, files.Length);
                Array.Copy(directories, 0, allEntries, files.Length, directories.Length);

                // You can customize the separator as needed.
                string separator = ",";
                string result = string.Join(separator, allEntries);
                return new Value(result);
            }
            catch (Exception)
            {
                return new Value("");
            }
        }



        bool haveWarnedUseOfFileIOFunctionsBefore = false;

        public void WarnUseOfFileIOFunctions()
        {
            if (!haveWarnedUseOfFileIOFunctionsBefore)
            {
                haveWarnedUseOfFileIOFunctionsBefore = true;
                Console.Error.WriteLine("Warning: This ghost of the Aya uses system functions of File IO. C# implementation of the Aya doesn't support them yet.");
            }
        }

        public Value Syscall_STRLEN(ArrayList args)
        {
            /*
                文字列長を取得します。単位はバイトです。
                第一引数に長さを調べる文字列を指定します。結果は戻り値で取得します。
                第二引数に結果を取得する変数を指定することもできます。この場合戻り値はありません。
            */
            string str = Content(args[0]).GetString();
            Value result = new Value(Encoding.Default.GetBytes(str).Length);

            if (args.Count >= 2)
            {
                VariablePointer vptr = (VariablePointer)args.ToArray().ElementAt(1);
                vptr.Store(result);
                return null;
            }
            else
            {
                return result;
            }
        }

        public Value Syscall_STRSTR(ArrayList args)
        {
            /*
                STRSTRは文字列の中から部分文字列を検索し、見つかったバイト位置を返します。
                引数は 3 つ、もしくは 4 つです。第一引数は検索される文字列、第二引数は検索する文字列、第三引数は探し始めるバイト位置です。第四引数は結果を取得する変数ですが、これを省略すると結果を戻り値で取得できます。
                第三引数が被検索対象の文字列長より大きい場合、および部分文字列が見つからなかった場合は -1 が返ります。
            */
            string all = Content(args[0]).GetString();
            string part = Content(args.ToArray().ElementAt(1)).GetString();
            int beginning = (int)Content(args.ToArray().ElementAt(2)).GetInteger();

            Value result;
            int pos = all.IndexOf(part, beginning);
            if (pos == -1)
            {
                result = new Value(-1);
            }
            else
            {
                result = new Value(Encoding.Default.GetBytes(all.Substring(0, pos)).Length);
            }

            if (args.Count >= 4)
            {
                VariablePointer vptr = (VariablePointer)args.ToArray().ElementAt(3);
                vptr.Store(result);
                return null;
            }
            else
            {
                return result;
            }
        }

        public Value Syscall_SUBSTR(ArrayList args)
        {
            /*
                SUBSTRは文字列から部分文字列を抽出します。
                引数は 3 つ。第一引数が抽出元の文字列、第二引数は抽出を開始するバイト位置、第三引数は抽出するバイト数です。
                結果は戻り値として取得します。
            */
            byte[] source = Encoding.Default.GetBytes(Content(args[0]).GetString());
            int beginning = (int)Content(args.ToArray().ElementAt(1)).GetInteger();
            int numBytes = (int)Content(args.ToArray().ElementAt(2)).GetInteger();

            if (beginning + numBytes >= source.Length || beginning < 0 || numBytes < 0)
            {
                return new Value("");
            }
            else
            {
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(source, beginning, numBytes);
                return new Value(Encoding.Default.GetString(memoryStream.ToArray()));
            }
        }

        public Value Syscall_REPLACE(ArrayList args)
        {
            /*
                REPLACE は、文字列中に出現する部分文字列のすべてを、別の文字列に置換えます。
                引数は 3 つで第一引数が被置換文字列、第二引数は置換前の部分文字列、第三引数は置換後の部分文字列です。
                結果は戻り値として取得します。
            */
            string source = Content(args[0]).GetString();
            string before = Content(args.ToArray().ElementAt(1)).GetString();
            string after = Content(args.ToArray().ElementAt(2)).GetString();
            return new Value(Regex.Replace(source, Regex.Escape(before), after));
        }

        public Value Syscall_ERASE(ArrayList args)
        {
            /*
                ERASEは文字列の一部を削除します。
                引数は 3 つで、第一引数が元の文字列、第二引数は削除を開始するバイト位置、第三引数は削除するバイト数です。
                結果は戻り値として取得します。
            */
            byte[] source = Encoding.Default.GetBytes(Content(args[0]).GetString());
            int beginning = (int)Content(args.ToArray().ElementAt(1)).GetInteger();
            int numBytes = (int)Content(args.ToArray().ElementAt(2)).GetInteger();

            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(source, 0, beginning);
            memoryStream.Write(source, beginning + numBytes, source.Length - (beginning + numBytes));
            return new Value(Encoding.Default.GetString(memoryStream.ToArray()));
        }

        public Value Syscall_INSERT(ArrayList args)
        {
            /*
                INSERTは文字列に部分文字列を挿入します。
                引数は 3 つで、第一引数が元の文字列、第二引数は部分文字列を挿入するバイト位置、第三引数は挿入する部分文字列です。
                結果は戻り値として取得します。
                挿入バイト位置が負数の場合は先頭に挿入されます。挿入元文字列長より大きい値を与えた場合は終端に追加されます。
            */
            byte[] source = Encoding.Default.GetBytes(Content(args[0]).GetString());
            int beginning = (int)Content(args.ToArray().ElementAt(1)).GetInteger();
            byte[] part = Encoding.Default.GetBytes(Content(args.ToArray().ElementAt(2)).GetString());

            if (beginning < 0)
            {
                beginning = 0;
            }
            else if (beginning > source.Length)
            {
                beginning = source.Length;
            }

            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(source, 0, beginning);
            memoryStream.Write(part, 0, part.Length);
            memoryStream.Write(source, beginning, source.Length - beginning);
            return new Value(Encoding.Default.GetString(memoryStream.ToArray()));
        }

        public Value Syscall_TOUPPER(ArrayList args)
        {
            /*
                TOUPPER は文字列に含まれるすべての半角英小文字を半角英大文字に変換します。
                引数に変換元文字列を指定します。変換結果は戻り値として取得します。
            */
            return new Value(Content(args[0]).GetString().ToUpper());
        }

        public Value Syscall_TOLOWER(ArrayList args)
        {
            /*
                TOLOWER は文字列に含まれるすべての半角英大文字を半角英小文字に変換します。
                引数に変換元文字列を指定します。変換結果は戻り値として取得します。
            */
            return new Value(Content(args[0]).GetString().ToLower());
        }

        public Value Syscall_CUTSPACE(ArrayList args)
        {
            /*
                文字列前後の空白文字だけからなる部分を削除します。
                該当する空白文字は半角スペース、全角スペース、タブ文字です。
                引数に変換元文字列を指定します。変換結果は戻り値として取得します。
            */
            return new Value(Content(args[0]).GetString().Trim());
        }

        public Value Syscall_MSTRLEN(ArrayList args)
        {
            /*
                文字列の文字数を取得します。
                第一引数に長さを調べる文字列を指定します。結果は戻り値で取得します。
            */
            return new Value(Content(args[0]).GetString().Length);
        }

        public Value Syscall_MSTRSTR(ArrayList args)
        {
            /*
                文字列の中から部分文字列を検索し、見つかった文字位置を返します。
                引数は 3 つです。第一引数は検索される文字列、第二引数は検索する文字列、第三引数は探し始める文字位置です。
            */
            string part = Content(args[0]).GetString();
            string all = Content(args.ToArray().ElementAt(1)).GetString();
            int beginning = (int)Content(args.ToArray().ElementAt(2)).GetInteger();

            return new Value(all.IndexOf(part, beginning));
        }

        public Value Syscall_MSUBSTR(ArrayList args)
        {
            /*
                SUBSTRは文字列から部分文字列を文字単位で抽出します。
                引数は 3 つ。第一引数が抽出元の文字列、第二引数は抽出を開始する文字位置、第三引数は抽出する文字数です。
                結果は戻り値として取得します。
            */
            string source = Content(args[0]).GetString();
            int beginning = (int)Content(args.ToArray().ElementAt(1)).GetInteger();
            int numBytes = (int)Content(args.ToArray().ElementAt(2)).GetInteger();

            return new Value(source.Substring(beginning, numBytes));
        }

        public Value Syscall_MERASE(ArrayList args)
        {
            /*
                ERASEは文字列の一部を削除します。
                引数は 3 つで、第一引数が元の文字列、第二引数は削除を開始する文字位置、第三引数は削除する文字数です。
                結果は戻り値として取得します。
            */
            string source = Content(args[0]).GetString();
            int beginning = (int)Content(args.ToArray().ElementAt(1)).GetInteger();
            int numBytes = (int)Content(args.ToArray().ElementAt(2)).GetInteger();

            StringBuilder buf = new StringBuilder(source);
            buf.Remove(beginning, numBytes);
            return new Value(buf.ToString());
        }

        public Value Syscall_MINSERT(ArrayList args)
        {
            /*
                INSERTは文字列に部分文字列を挿入します。
                引数は 3 つで、第一引数が元の文字列、第二引数は部分文字列を挿入する文字位置、第三引数は挿入する部分文字列です。
                結果は戻り値として取得します。
            */
            string source = Content(args[0]).GetString();
            int beginning = (int)Content(args.ToArray().ElementAt(1)).GetInteger();
            string part = Content(args.ToArray().ElementAt(2)).GetString();

            if (beginning < 0)
            {
                beginning = 0;
            }
            else if (beginning > source.Length)
            {
                beginning = source.Length;
            }

            StringBuilder buf = new StringBuilder(source);
            buf.Insert(beginning, part);
            return new Value(buf.ToString());
        }

        public Value Syscall_SIN(ArrayList args)
        {
            /*
                sin を計算します。引数の単位はラジアンです。
                結果は戻り値で取得します。
            */
            double rad = Content(args[0]).GetReal();
            return new Value(Math.Sin(rad));
        }

        public Value Syscall_COS(ArrayList args)
        {
            /*
                cos を計算します。引数の単位はラジアンです。
                結果は戻り値で取得します。
            */
            double rad = Content(args[0]).GetReal();
            return new Value(Math.Cos(rad));
        }

        public Value Syscall_TAN(ArrayList args)
        {
            /*
                tan を計算します。引数の単位はラジアンです。
                結果は戻り値で取得します。
            */
            double rad = Content(args[0]).GetReal();
            return new Value(Math.Tan(rad));
        }

        public Value Syscall_LOG(ArrayList args)
        {
            /*
                e を底とする対数（自然対数）を計算します。結果は戻り値で取得します。
            */
            double n = Content(args[0]).GetReal();
            return new Value(Math.Log(n));
        }

        public Value Syscall_LOG10(ArrayList args)
        {
            /*
                10 を底とする対数（常用対数）を計算します。結果は戻り値で取得します。
            */
            double n = Content(args[0]).GetReal();
            return new Value(Math.Log10(n));
        }

        public Value Syscall_POW(ArrayList args)
        {
            /*
                第一引数の第二引数乗を計算します。結果は戻り値で取得します。
            */
            double a = Content(args[0]).GetReal();
            double b = Content(args.ToArray().ElementAt(1)).GetReal();
            return new Value(Math.Pow(a, b));
        }

        public Value Syscall_FLOOR(ArrayList args)
        {
            double n = Content(args[0]).GetReal();
            return new Value(Math.Floor(n));
        }

        public Value Syscall_CEIL(ArrayList args)
        {
            double n = Content(args[0]).GetReal();
            return new Value(Math.Ceiling(n));
        }

        public Value Syscall_ROUND(ArrayList args)
        {
            double n = Content(args[0]).GetReal();
            return new Value(Math.Round(n));
        }

        public Value Syscall_SQRT(ArrayList args)
        {
            /*
                平方根を計算します。結果は戻り値で取得します。
                引数に負数は与えられません。イリーガルコードとして -1 を返します。
            */
            double n = Content(args[0]).GetReal();
            if (n < 0)
            {
                return new Value(-1);
            }
            else
            {
                return new Value(Math.Sqrt(n));
            }
        }

        public Value Content(object obj)
        {
            // objがValueならそのまま、VariablePointerならデリファレンスして返す。
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
                throw new RuntimeBinderException("Internal Error: an object of " + obj.GetType().FullName + " was in arguments of system call illegally.");
            }
        }




    }
}