using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace aya
{
    public class Preprocessor
    {
        protected static Hashtable Global = new Hashtable(); // {SearchString => ReplaceString}

        private readonly TextReader _reader;
        private readonly Hashtable _local; // {SearchString => ReplaceString}

        public Preprocessor(TextReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _local = new Hashtable();
        }

        public string DoIt()
        {
            StringBuilder result = new StringBuilder();

            Regex define = new Regex(@"^\s*#define\s+(.+?)\s+(.+?)\s*$");
            Regex globalDefine = new Regex(@"^\s*#globaldefine\s+(.+?)\s+(.+?)\s*$");

            Regex brokenStringA = new Regex(@"^\s*\""([^\\""]*?)\s*$");
            Regex brokenStringB = new Regex(@"^\s*([^\\""]*?)\""\s*$");

            while (true)
            {
                string line = _reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                // If the line ends with '/' and is not a comment, consider it as line continuation.
                while (line.TrimEnd().EndsWith("/") &&
                       !(line.TrimEnd().EndsWith("//") || line.TrimEnd().EndsWith("*/")))
                {
                    line = line.Substring(0, line.Length - 1);

                    string next = _reader.ReadLine();
                    if (next == null)
                    {
                        break;
                    }
                    else
                    {
                        // Remove leading white spaces.
                        for (int pos = 0; pos < next.Length; pos++)
                        {
                            if (!char.IsWhiteSpace(next[pos]))
                            {
                                line += next.Substring(pos);
                                break;
                            }
                        }
                    }
                }

                // If the line consists only of a string constant, complete it even if the double quotes are missing.
                // The mighty GWAHHHHHHHHHHHHH!
                // It's okay to go to the goal now, right...
                Match matcher;
                if ((matcher = brokenStringA.Match(line)).Success)
                {
                    line = '"' + matcher.Groups[1].Value + '"';
                }
                else if ((matcher = brokenStringB.Match(line)).Success)
                {
                    line = '"' + matcher.Groups[1].Value + '"';
                }

                if ((matcher = define.Match(line)).Success)
                {
                    _local[matcher.Groups[1].Value] = matcher.Groups[2].Value;
                }
                else if ((matcher = globalDefine.Match(line)).Success)
                {
                    Global[matcher.Groups[1].Value] = matcher.Groups[2].Value;
                }
                else
                {
                    StringBuilder buf = new StringBuilder(line);

                    Hashtable[] tables = { _local, Global };
                    foreach (var table in tables)
                    {
                        foreach (DictionaryEntry entry in table)
                        {
                            string key = (string)entry.Key;
                            string value = (string)entry.Value;

                            int pos = 0;
                            while (pos < buf.Length)
                            {
                                int idx = buf.ToString().IndexOf(key, pos);
                                if (idx == -1)
                                {
                                    break;
                                }
                                else
                                {
                                    buf.Remove(idx, key.Length);
                                    buf.Insert(idx, value);
                                    pos = idx + value.Length;
                                }
                            }
                        }
                    }

                    result.Append(buf.ToString()).Append('\n');
                }
            }

            return result.ToString();
        }
    }
}
