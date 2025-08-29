using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Security.Policy;
using System.Xml.Linq;

namespace aya.Eval
{
    public class GlobalNamespace : Namespace
    {
        private static Hashtable methodCache = new Hashtable();

        private Hashtable sysvarCache = null;

        public void SaveToFile(FileInfo file)
        {
            try
            {
                SaveToStream(new FileStream(file.FullName, FileMode.Create));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Couldn't save global namespace.");
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        private static readonly Regex PatDoubleQuote = new Regex("\"");

        public void SaveToStream(Stream outputStream)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(outputStream, Encoding.GetEncoding(Aya.CHARSET)))
                {
                    writer.Write("@@@SAVEDATA@@@ {\r\n");
                    foreach (DictionaryEntry entry in hash)
                    {
                        string varname = (string)entry.Key;
                        Variable v = (Variable)entry.Value;

                        Value val = v.GetValue();
                        if (val.IsString())
                        {
                            string content = PatDoubleQuote.Replace(val.GetString(), "%ASC(34)");
                            writer.Write($"    {varname} = \"{content}\";\r\n");

                            if (v.GetArraySize() > 1)
                            {
                                string delim = PatDoubleQuote.Replace(v.GetSeparator(), "%ASC(34)");
                                writer.Write($"    SETSEPARATOR({varname},\"{delim}\");\r\n");
                            }
                        }
                        else
                        {
                            writer.Write($"    {varname} := {val.GetReal()};\r\n");
                        }
                    }
                    writer.Write("}\r\n");
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Couldn't save global namespace.");
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        public Variable Get(string name)
        {
            string methodName = "SysVar_" + name;

            // Check if the method is cached
            MethodInfo method = (MethodInfo)methodCache[methodName];
            if (method == null)
            {
                // If not cached, check if the method exists
                try
                {
                    method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
                    methodCache[methodName] = method;
                }
                catch (Exception)
                {
                    // Method does not exist
                }
            }

            // If the method exists, invoke it
            if (method != null)
            {
                try
                {
                    return (Variable)method.Invoke(this, null);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Couldn't get value of system variable {name}.");
                    Console.Error.WriteLine(e.StackTrace);
                    return null;
                }
            }

            // If method does not exist, treat it as a regular variable
            return base.Get(name);
        }

        public Variable Define(string name)
        {
            Variable v = Get(name);
            return v ?? base.Define(name);
        }

        protected Variable DefineSysVar(string name)
        {
            // Create sysvar_cache if it's null
            if (sysvarCache == null)
            {
                sysvarCache = new Hashtable();
            }

            Variable v = (Variable)sysvarCache[name];
            if (v == null)
            {
                v = new Variable(new Value(""));
                sysvarCache[name] = v;
            }
            return v;
        }

        // System Variable Methods
        public Variable SysVar_Year()
        {
           // Calendar cal = Calendar.GetInstance();

            Variable v = DefineSysVar("year");
            v.Unlock();
         //   v.SetValue(new Value(cal.Get(Calendar.Year)));
            v.Lock();
            return v;
        }

        public Variable SysVar_Month()
        {
            //Calendar cal = Calendar.GetInstance();

            Variable v = DefineSysVar("month");
            v.Unlock();
           // v.SetValue(new Value(cal.Get(Calendar.Month) + 1));
            v.Lock();
            return v;
        }

        public Variable SysVar_Day()
        {
            //Calendar cal = Calendar.GetInstance();

            Variable v = DefineSysVar("day");
            v.Unlock();
           // v.SetValue(new Value(cal.Get(Calendar.DayOfMonth)));
            v.Lock();
            return v;
        }

        public Variable SysVar_Weekday()
        {
            //Calendar cal = Calendar.GetInstance();
           // cal.FirstDayOfWeek = Calendar.Sunday;

            Variable v = DefineSysVar("weekday");
            v.Unlock();
           // v.SetValue(new Value(cal.Get(Calendar.DayOfWeek)));
            v.Lock();
            return v;
        }

        public Variable SysVar_Hour()
        {
           // Calendar cal = Calendar.GetInstance();

            Variable v = DefineSysVar("hour");
            v.Unlock();
           // v.SetValue(new Value(cal.Get(Calendar.HourOfDay)));
            v.Lock();
            return v;
        }

        public Variable SysVar_12Hour()
        {
           // Calendar cal = Calendar.GetInstance();

            Variable v = DefineSysVar("12hour");
            v.Unlock();
           // v.SetValue(new Value(cal.Get(Calendar.HourOfDay) % 12));
            v.Lock();
            return v;
        }

        public Variable SysVar_Ampm()
        {
           // Calendar cal = Calendar.GetInstance();

            Variable v = DefineSysVar("ampm");
            v.Unlock();
          //  v.SetValue(new Value(cal.Get(Calendar.HourOfDay) >= 12 ? 1 : 0)); // 0 for AM, 1 for PM
            v.Lock();
            return v;
        }

        public Variable SysVar_Minute()
        {
           // Calendar cal = Calendar.GetInstance();

            Variable v = DefineSysVar("minute");
            v.Unlock();
        //    v.SetValue(new Value(cal.Get(Calendar.Minute)));
            v.Lock();
            return v;
        }

        public Variable SysVar_Second()
        {
            //Calendar cal = Calendar.GetInstance();

            Variable v = DefineSysVar("second");
            v.Unlock();
           // v.SetValue(new Value(cal.Get(Calendar.Second)));
            v.Lock();
            return v;
        }

        // Unimplemented System Variable Methods
        private Variable Unimplemented(string name)
        {
            Variable v = DefineSysVar(name);
            v.Unlock();
            v.SetValue(new Value(0));
            v.Lock();
            return v;
        }

        public Variable SysVar_SystemUpTickCount() => Unimplemented("systemuptickcount");

        public Variable SysVar_SystemUptime() => Unimplemented("systemuptime");

        public Variable SysVar_SystemUpHour() => Unimplemented("systemuphour");

        public Variable SysVar_SystemUpMinute() => Unimplemented("systemupminute");

        public Variable SysVar_SystemUpSecond() => Unimplemented("systemupsecond");

        public Variable SysVar_MemoryLoad() => Unimplemented("memoryload");

        public Variable SysVar_MemoryTotalPhys() => Unimplemented("memorytotalphys");

        public Variable SysVar_MemoryTotalVirtual() => Unimplemented("memorytotalvirtual");

        public Variable SysVar_MemoryAvailVirtual() => Unimplemented("memoryavailvirtual");
    }
}
