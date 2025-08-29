using aya.Eval;
using aya.Node;
using aya;
using System;
using System.Collections;
using System.IO;
using System.Text;
using Utils;
namespace aya
{
    public class Dictionary
    {
        private Aya aya;
        private Hashtable functions; // {FunctionName => Function}

        public Dictionary(Aya aya)
        {
            this.aya = aya;
            this.functions = new Hashtable();
        }

        public void LoadFile(FileInfo dicFile)
        {
            LexicalAnalyzer lex = null;
            try
            {
                //; new StreamReader(new FileStream(dicFile.FullName, FileMode.Open))


                TextReader reader;
                if (dicFile.Extension.ToLower() == ".ayc") // Encrypted
                    reader = new DataDecodingInputStream(new FileStream(dicFile.FullName, FileMode.Open));
                else
                    reader = new BufferReader(new FileStream(dicFile.FullName, FileMode.Open), Encoding.GetEncoding(Aya.CHARSET));

                string preprocessed = new Preprocessor(reader).DoIt();
                lex = new LexicalAnalyzer(preprocessed);
                Parser parser = new Parser(aya, lex);
                while (true)
                {
                    try
                    {
                        Function f = parser.ParseFunction();
                        if (f == null)
                        {
                            throw new IllegalTokenException(lex.NextToken());
                        }
                        else
                        {
                            functions[f.GetName()] = f;
                        }
                    }
                    catch (NoMoreTokensException e)
                    {
                        break;
                    }
                }
            }
            catch (IllegalTokenException e)
            {
                Console.Error.WriteLine(dicFile.Name + ":"
                                       + e.GetToken().GetPosition.Row
                                       + ": Syntax error occurred near \""
                                       + e.GetToken().GetToken + "\"");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Couldn't load file " + dicFile.FullName);
                Console.Error.WriteLine("lex: " + lex);
                Console.Error.WriteLine(e.StackTrace);
            }
        }


        public void LoadString(string dicString)
        {
            LexicalAnalyzer lex = null;
            //try
            {
                //; new StreamReader(new FileStream(dicFile.FullName, FileMode.Open))



                string preprocessed = dicString;
                lex = new LexicalAnalyzer(preprocessed);
                Parser parser = new Parser(aya, lex);
                while (true)
                {
                    try
                    {
                        Function f = parser.ParseFunction();
                        if (f == null)
                        {
                            throw new IllegalTokenException(lex.NextToken());
                        }
                        else
                        {
                            functions[f.GetName()] = f;
                        }
                    }
                    catch (NoMoreTokensException e)
                    {
                        break;
                    }
                }
            }
           // catch (IllegalTokenException e)
            {
               
            }
            //catch (Exception e)
            {

                Console.Error.WriteLine("lex: " + lex);
               // Console.Error.WriteLine(e.StackTrace);
            }


        }


        public Function GetFunction()
        {

            int count = functions.Keys.Count;
            string name;
            foreach (string key in functions.Keys)
            {
                name = key;


                Function f = (Function)functions[name];
                if (f != null)
                {
                    return f;
                }

                SystemCall syscall = aya.GetSystemCall().Get(name);
                if (syscall != null)
                {
                    return syscall;
                }
            }
            
            // User-defined functions take precedence over system calls.
           

            return null;
        }



        public Function GetFunction(string name)
        {
            // User-defined functions take precedence over system calls.
            Function f = (Function)functions[name];
            if (f != null)
            {
                return f;
            }

            SystemCall syscall = aya.GetSystemCall().Get(name);
            if (syscall != null)
            {
                return syscall;
            }

            return null;
        }

        public void UndefFunction(string name)
        {
            functions.Remove(name);
        }

        public void Clear()
        {
            if (functions != null && functions.Count > 0)
            {
                // this.functions = new Hashtable();
                
 


                 functions.Clear();
            }
        }
    }
}
