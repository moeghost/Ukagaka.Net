using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace LittleGrayCalculator.Cores
{
    internal class BigProgramer
    {
       static string hexnum = "0123456789ABCDEF";

        public static int FLOAT_PLACE = 30;
         
        public static int floatplace = 30;


        public static string CarryShiftForInt(string value, int oldCarry, int newCarry)
        {
            double endTime = 0;
            string c = "";
            List<int> ans = new List<int>();
            int k = 0;
            string a = value;
            string b = "";

            int len = a.Length;
            int j = 0;
            int sum = 1;
            for (int i = 0; i < len; i++)
            {
                if (char.IsLower(a.Substring(i, 1)[0]))
                {
                  //  ans.Add(int.Parse(a.Substring(i, 1)) - 97 + 10);
                }
                else if (char.IsUpper(a.Substring(i, 1)[0]))
                {
                   // ans.Add(int.Parse(a.Substring(i, 1)) - 65 + 10);
                }
                else
                {
                    //ans.Add(int.Parse(a.Substring(i, 1)) - 48);
                }
                // ans.Add(int.Parse(a.Substring(i, 1)));


                ans.Add((int)STRSTR(hexnum, a.Substring(i, 1), 0));
            }

            while (sum > 0)
            {
                sum = 0;
                for (int i = 0; i < len; i++)
                {
                    k = ans[i] / newCarry;
                    sum += k;
                    if (i == len - 1)
                    {
                        c += hexnum.ToCharArray()[ans[i] % newCarry];
                        j += 1;
                    }
                    else
                    {
                        ans[i + 1] += ans[i] % newCarry * oldCarry;
                    }
                    ans[i] = k;
                }
                
            }
            return ReverseString(c);
        }

        private static string ReverseString(string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static double STRSTR(string str, string key, int i)
        {
            int pos = -1;
            int rangeStartIndex = str.IndexOf(key, i);
            if (rangeStartIndex >= 0)
            {
                pos = rangeStartIndex - i;
            }
            return Convert.ToDouble(pos);
        }

        public static string CarryToDec(string value, int oldCarry)
        {
            double endTime = 0;
            BigNumber c = new BigNumber("0");
           // int[] ans = new int[0];
            int newCarry = 10;
            // let old_binary = 10
            int k = 0;
            string a = value;
            string d = value;
            string b = "";
            int N = d.ToString().IndexOf(".", 0);
            if (N > 0)
            {
                a = d.Substring(0, N);
                b = d.Substring(N + 1, d.ToString().Length - (N + 1));
            }
            string shift = CarryShiftForInt(a, oldCarry, newCarry);
            c = new BigNumber(shift);
            BigNumber sum = new BigNumber("0");
            if (b.ToString() != "" && b.ToString() != "0")
            {
                int i = 0;

                 
                while (i  < b.ToString().Length && b.ToString().Substring(i, 1) != "")
                {
                    BigNumber temp = new BigNumber((-(i + 1)).ToString());
                    BigNumber e = new BigNumber(oldCarry.ToString()) ^ temp;

                    string s = b.ToString().Substring(i, 1);

                    double hex = STRSTR(hexnum, s, 0);

                    BigNumber m = new BigNumber(hex.ToString()) * e;

                    sum = sum + m;
                    i += 1;
 
                }
                return (c + sum).ToString();
            }
            // You may need to return a value here in C# depending on your logic
            return c.ToString();
        }
        public static string SUBSTR(string str, int i, int num)
        {
            string result = "";
            if (i < str.Length && i >= 0)
            {
                if (num + i <= str.Length)
                {
                    result = str.Substring(i, num);
                }
            }
            return result;
        }
        public static string DecToCarry(string _a, int newCarry)
        {

            int MIN = floatplace > _a.Length ? floatplace : _a.Length;
            double endTime = 0;
            string c = "0";
            
            int oldCrray = 10;
            int k = 0;
            string a = _a;
            BigNumber d = new BigNumber(_a);
            string b = "";

            int N = d.ToString().IndexOf(".", 0);
            if (N > 0)
            {
                a = d.ToString().Substring(0, N);
                b = d.ToString().Substring(N + 1, d.ToString().Length - (N + 1));
            }

            c = CarryShiftForInt(a, oldCrray, newCarry);

            String sum = "";

            if (b != "")
            {
                while (b != "0" && sum.ToString().Length < MIN)
                {
                    d = new BigNumber("0." + b);

                    d = d * new BigNumber(newCarry.ToString());
                    
                    N = d.ToString().IndexOf(".", 0);
                    if (N > 0)
                    {
                        a = d.ToString().Substring(0, N);
                        b = d.ToString().Substring(N + 1, d.ToString().Length - (N + 1));
                        sum += SUBSTR(hexnum, Convert.ToInt32(a) % newCarry, 1);
                    }
                    
                }
                if (sum != "" && sum != "0")
                {
                    return c + "." + sum.ToString();
                }
            }
            return c;
        }

        static public string BigTrans(string value, int oldCarry, int newCarry)
        {


            double endTime = 0;
            string result = "";
            ArrayList ans = new ArrayList();
            int k = 0;

            var num = value.ToString();
           /* var a = num;
            var b = "";
            int N = a.IndexOf(".", 0);
            if (N > 0)
            {
                a = num.Substring(0, N);
                b = num.Substring(N + 1, a.Length);
            }
           */
            result = CarryToDec(num, oldCarry);



            result = DecToCarry(result, newCarry);



            return result;



        }

        /*
               static func bigtrans(_ _a:String, _ old_binary:Int, _ new_binary:Int) -> String{


               var endTime:Double = 0
               var c:String = ""
               var ans:[Int] = [Int]
               ()

               var k = 0

               var a = _a
               var b = ""
               let N = a.indexOf(".", 0)
               if N > 0{
                   a = a.substring(0, N)
                   b = _a.substring(N + 1, _a.count)
               }

           c = CarryToDec(_a, old_binary)


               c = DecToCarry(c, new_binary)



               return c

       }


           */



    }
}
